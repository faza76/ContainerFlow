using ContainerFlow.Contracts.Enums;
using Microsoft.EntityFrameworkCore;
using DomainBooking = ContainerFlow.Booking.Api.Domain.Booking;
using DomainBookingContainer = ContainerFlow.Booking.Api.Domain.BookingContainer;

namespace ContainerFlow.Booking.Api.Persistence;

/// <summary>
/// Seeds ~100 realistic bookings when the database is empty.
/// Runs once on startup after EnsureCreated(); no-ops if data exists.
/// </summary>
public static class BookingSeedData
{
    public static void Seed(BookingDbContext db)
    {
        if (db.Bookings.Any()) return;

        var rng = new Random(42); // deterministic
        var now = DateTime.UtcNow;

        string[] customers = [
            "PT Nusantara Logistik", "PT Samudra Perkasa", "PT Jaya Maritime",
            "PT Pelangi Cargo", "PT Garuda Shipping", "PT Nusantara Logistik",
            "PT Bahari Sentosa", "PT Trikora Freight", "PT Mulia Lautan",
            "PT Cipta Niaga", "PT Bintang Laut", "PT Panca Samudera",
            "PT Sinar Samudera", "PT Tunas Maritim", "PT Wirata Lines"
        ];

        string[] origins = [
            "Jakarta", "Surabaya", "Semarang", "Makassar", "Belawan",
            "Tanjung Priok", "Tanjung Perak", "Balikpapan", "Batam", "Palembang"
        ];

        string[] destinations = [
            "Singapore", "Shanghai", "Tokyo", "Busan", "Hong Kong",
            "Laem Chabang", "Port Klang", "Tanjung Pelepas", "Colombo", "Kaohsiung"
        ];

        string[] cargoes = [
            "Automotive parts", "Palm oil", "Rubber products", "Textiles",
            "Electronics components", "Steel coils", "Cement", "Rice",
            "Coffee beans", "Tobacco leaf", "Paper products", "Furniture",
            "Ceramic tiles", "Plastic granules", "Chemical drums"
        ];

        string[] vessels = [
            "MV Nusantara Express", "MV Samudra Raya", "MV Garuda Nusantara",
            "MV Trikora Spirit", "MV Pelangi Jaya"
        ];

        string[] voyagePrefixes = ["CF", "NE", "SR", "GN", "TS"];

        var statuses = new (BookingStatus s, int weight)[]
        {
            (BookingStatus.PENDING, 10),
            (BookingStatus.CONFIRMED, 15),
            (BookingStatus.CANCELLED, 5),
            (BookingStatus.IN_PROGRESS, 45),
            (BookingStatus.COMPLETED, 25),
        };
        var statusPool = statuses.SelectMany(x => Enumerable.Repeat(x.s, x.weight)).ToArray();

        var containerTypes = new[] { ContainerType.FT20, ContainerType.FT40, ContainerType.FT40HC, ContainerType.REEFER };

        var bookings = new List<DomainBooking>();
        for (int i = 1; i <= 100; i++)
        {
            var daysAgo = rng.Next(5, 90);
            var created = now.AddDays(-daysAgo).AddHours(-rng.Next(0, 23));
            var status = statusPool[rng.Next(statusPool.Length)];
            var vesselIdx = rng.Next(vessels.Length);

            var b = new DomainBooking
            {
                Id = Guid.NewGuid(),
                BookingNumber = $"BK-2026-{i:D5}",
                CustomerId = Guid.NewGuid(),
                CustomerName = customers[rng.Next(customers.Length)],
                Origin = origins[rng.Next(origins.Length)],
                Destination = destinations[rng.Next(destinations.Length)],
                Cargo = cargoes[rng.Next(cargoes.Length)],
                ContainerType = containerTypes[rng.Next(containerTypes.Length)],
                ContainerCount = rng.Next(1, 6),
                Vessel = rng.Next(100) < 70 ? vessels[vesselIdx] : null,
                Voyage = rng.Next(100) < 70 ? $"{voyagePrefixes[vesselIdx]}-{rng.Next(100, 999)}" : null,
                Status = status,
                CreatedAtUtc = created,
                UpdatedAtUtc = created.AddHours(rng.Next(1, 48)),
            };

            // Attach some containers to in-progress/completed bookings
            if (status is BookingStatus.IN_PROGRESS or BookingStatus.COMPLETED)
            {
                int attachCount = rng.Next(1, Math.Min(b.ContainerCount, 3) + 1);
                for (int c = 0; c < attachCount; c++)
                {
                    b.Containers.Add(new DomainBookingContainer
                    {
                        Id = Guid.NewGuid(),
                        BookingId = b.Id,
                        ContainerId = Guid.NewGuid(),
                        ContainerNumber = GenerateContainerNumber(rng),
                    });
                }
            }

            bookings.Add(b);
        }

        db.Bookings.AddRange(bookings);
        db.SaveChanges();
    }

    private static string GenerateContainerNumber(Random rng)
    {
        string[] prefixes = ["MSCU", "MSKU", "TCLU", "CMAU", "SEGU", "KKFU", "OOLU", "EISU", "CSNU", "PCIU"];
        return $"{prefixes[rng.Next(prefixes.Length)]}{rng.Next(1000000, 9999999)}";
    }
}
