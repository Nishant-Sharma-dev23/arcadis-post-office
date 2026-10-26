using Microsoft.EntityFrameworkCore;
using Postal.Api.Domain;

namespace Postal.Api.Data;

/// <summary>Demo data so paging and filters have something to show. Only runs on an empty database.</summary>
public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.PostOffices.AnyAsync(cancellationToken))
            return;

        PostOffice[] offices =
        [
            new("10115", "Berlin Mitte"),
            new("20095", "Hamburg Altstadt"),
            new("50667", "Cologne Centre"),
            new("80331", "Munich Old Town")
        ];
        db.PostOffices.AddRange(offices);
        await db.SaveChangesAsync(cancellationToken);

        decimal[] weights = [0.02m, 0.3m, 0.75m, 1m, 2.4m, 4.9m, 5m, 7.5m, 12m, 0.1m, 3.3m, 18.25m];
        var shipments = new List<Shipment>();
        for (var i = 0; i < 25; i++)
        {
            var origin = offices[i % offices.Length];
            var destination = offices[(i + 1 + i / offices.Length) % offices.Length];
            if (destination == origin)
                destination = offices[(i + 2) % offices.Length];

            var weight = weights[i % weights.Length];
            var type = weight < 0.5m ? ShipmentType.Letter : ShipmentType.Package;
            var shipment = new Shipment(type, weight, origin.Id, destination.Id);

            // Spread statuses: some at origin, some at destination, some delivered.
            for (var step = 0; step < i % 3; step++)
                shipment.AdvanceStatus();

            shipments.Add(shipment);
        }

        db.Shipments.AddRange(shipments);
        await db.SaveChangesAsync(cancellationToken);
    }
}
