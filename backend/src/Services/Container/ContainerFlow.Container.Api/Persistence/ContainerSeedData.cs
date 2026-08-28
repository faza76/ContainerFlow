using ContainerFlow.Contracts.Enums;
using Microsoft.EntityFrameworkCore;
using DomainContainer = ContainerFlow.Container.Api.Domain.Container;

namespace ContainerFlow.Container.Api.Persistence;

/// <summary>
/// Seeds ~120 containers when the database is empty.
/// Status distribution: ~40 AVAILABLE, ~10 ALLOCATED, ~10 IN_YARD,
/// ~15 LOADED, ~20 IN_TRANSIT, ~15 DISCHARGED, ~10 DELIVERED.
/// </summary>
public static class ContainerSeedData
{
    public static void Seed(ContainerDbContext db)
    {
        if (db.Containers.Any()) return;

        var rng = new Random(42);
        var now = DateTime.UtcNow;

        string?[] locations = [
            "Tanjung Priok Yard A", "Tanjung Priok Yard B", "Surabaya Terminal",
            "Jakarta Warehouse 1", "Jakarta Warehouse 2", "Singapore PSA",
            "Onboard MV Nusantara Express", "Onboard MV Samudra Raya",
            "Shanghai Port", "Busan Terminal", null, null, null
        ];

        var statusWeights = new (ContainerStatus s, int weight)[]
        {
            (ContainerStatus.AVAILABLE, 40),
            (ContainerStatus.ALLOCATED, 10),
            (ContainerStatus.IN_YARD, 10),
            (ContainerStatus.LOADED, 15),
            (ContainerStatus.IN_TRANSIT, 20),
            (ContainerStatus.DISCHARGED, 15),
            (ContainerStatus.DELIVERED, 10),
        };
        var statusPool = statusWeights.SelectMany(x => Enumerable.Repeat(x.s, x.weight)).ToArray();

        var containers = new List<DomainContainer>();
        for (int i = 1; i <= 120; i++)
        {
            var type = GetRandomType(rng);
            var status = statusPool[rng.Next(statusPool.Length)];
            var daysAgo = rng.Next(5, 90);
            var created = now.AddDays(-daysAgo).AddHours(-rng.Next(0, 23));

            var c = new DomainContainer
            {
                Id = Guid.NewGuid(),
                ContainerNumber = GenerateContainerNumber(rng, i),
                Type = type,
                Status = status,
                Teu = type switch
                {
                    ContainerType.FT20 => 1,
                    _ => 2,
                },
                BookingId = status == ContainerStatus.AVAILABLE ? null : Guid.NewGuid(),
                BookingNumber = status == ContainerStatus.AVAILABLE ? null : $"BK-2026-{rng.Next(1, 101):D5}",
                Location = GetLocationForStatus(status, locations, rng),
                CreatedAtUtc = created,
                UpdatedAtUtc = created.AddHours(rng.Next(1, 72)),
            };

            containers.Add(c);
        }

        db.Containers.AddRange(containers);
        db.SaveChanges();
    }

    private static ContainerType GetRandomType(Random rng) => rng.Next(100) switch
    {
        < 30 => ContainerType.FT20,
        < 60 => ContainerType.FT40,
        < 85 => ContainerType.FT40HC,
        _ => ContainerType.REEFER,
    };

    private static string? GetLocationForStatus(ContainerStatus status, string[] locations, Random rng) => status switch
    {
        ContainerStatus.AVAILABLE => null,
        ContainerStatus.ALLOCATED => locations[rng.Next(3)], // yard locations
        ContainerStatus.IN_YARD => locations[rng.Next(3)],
        ContainerStatus.LOADED => locations[6 + rng.Next(2)], // onboard
        ContainerStatus.IN_TRANSIT => locations[6 + rng.Next(2)],
        ContainerStatus.DISCHARGED => locations[4 + rng.Next(2)], // destination port
        ContainerStatus.DELIVERED => locations[4 + rng.Next(2)],
        _ => null,
    };

    private static string GenerateContainerNumber(Random rng, int seq)
    {
        // Real container prefixes: MSCU, MSKU (MSC), TCLU, CMAU (CMA CGM), SEGU, KKFU (Evergreen), OOLU (OOCL), EISU, CSNU, PCIU
        string[] prefixes = ["MSCU", "MSKU", "TCLU", "CMAU", "SEGU", "KKFU", "OOLU", "EISU", "CSNU", "PCIU"];
        // Use seq to ensure uniqueness while looking random
        return $"{prefixes[seq % prefixes.Length]}{7000000 + seq}";
    }
}
