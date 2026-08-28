using ContainerFlow.Notification.Api.Domain;
using Microsoft.EntityFrameworkCore;
using DomainNotification = ContainerFlow.Notification.Api.Domain.Notification;

namespace ContainerFlow.Notification.Api.Persistence;

/// <summary>
/// Seeds ~100 notifications when the database is empty.
/// Mirrors what the MassTransit consumers would have produced.
/// </summary>
public static class NotificationSeedData
{
    public static void Seed(NotificationDbContext db)
    {
        if (db.Notifications.Any()) return;

        var rng = new Random(42);
        var now = DateTime.UtcNow;

        var templates = new (string EventType, string Title, string Message)[]
        {
            ("BookingCreated", "New Booking Created", "Booking {0} has been created for {1}."),
            ("BookingConfirmed", "Booking Confirmed", "Booking {0} has been confirmed. Shipment is ready for container allocation."),
            ("ContainerAllocated", "Container Allocated", "Container {2} has been allocated to booking {0}."),
            ("ShipmentStatusChanged", "Shipment Departed", "Booking {0} vessel has departed from {3}."),
            ("ShipmentStatusChanged", "Shipment Arrived", "Booking {0} has arrived at {4}."),
            ("ContainerStatusChanged", "Container Loaded", "Container {2} has been loaded onto vessel for booking {0}."),
            ("ContainerStatusChanged", "Container Discharged", "Container {2} has been discharged at destination port."),
            ("ContainerStatusChanged", "Container Delivered", "Container {2} has been delivered to final destination."),
        };

        string[] bookingNumbers = Enumerable.Range(1, 100).Select(i => $"BK-2026-{i:D5}").ToArray();
        string[] customers = [
            "PT Nusantara Logistik", "PT Samudra Perkasa", "PT Jaya Maritime",
            "PT Pelangi Cargo", "PT Garuda Shipping", "PT Bahari Sentosa",
            "PT Trikora Freight", "PT Mulia Lautan", "PT Cipta Niaga", "PT Bintang Laut"
        ];
        string[] origins = ["Jakarta", "Surabaya", "Semarang", "Makassar", "Belawan"];
        string[] destinations = ["Singapore", "Shanghai", "Tokyo", "Busan", "Hong Kong"];
        string[] containerNumbers = Enumerable.Range(1, 50)
            .Select(i => $"MSCU{7000000 + i}")
            .ToArray();

        var notifications = new List<DomainNotification>();
        int seq = 0;

        for (int i = 0; i < 100; i++)
        {
            var bookingNum = bookingNumbers[rng.Next(bookingNumbers.Length)];
            var template = templates[rng.Next(templates.Length)];
            var customer = customers[rng.Next(customers.Length)];
            var containerNum = containerNumbers[rng.Next(containerNumbers.Length)];
            var origin = origins[rng.Next(origins.Length)];
            var dest = destinations[rng.Next(destinations.Length)];
            var daysAgo = rng.Next(1, 60);
            var created = now.AddDays(-daysAgo).AddHours(-rng.Next(0, 23));

            var title = template.Title;
            var message = string.Format(template.Message, bookingNum, customer, containerNum, origin, dest);
            var idempotencyKey = $"{template.EventType}:{bookingNum}:{containerNum}:{created:yyyyMMddHHmmss}:{seq++}";

            notifications.Add(new DomainNotification
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                BookingId = Guid.NewGuid(),
                ContainerId = Guid.NewGuid(),
                EventType = template.EventType,
                Title = title,
                Message = message,
                IdempotencyKey = idempotencyKey,
                CreatedAtUtc = created,
                IsRead = rng.Next(100) < 40, // ~40% read
            });
        }

        db.Notifications.AddRange(notifications);
        db.SaveChanges();
    }
}
