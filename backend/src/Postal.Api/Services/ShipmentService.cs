using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Postal.Api.Contracts;
using Postal.Api.Data;
using Postal.Api.Domain;

namespace Postal.Api.Services;

public class ShipmentService(AppDbContext db)
{
    public async Task<PagedResult<ShipmentDto>> ListAsync(ShipmentQuery query, CancellationToken cancellationToken)
    {
        var shipments = db.Shipments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.ShipmentNumber))
        {
            // Numbers are stored upper-case, so upper-casing the search term makes it case-insensitive.
            var term = query.ShipmentNumber.Trim().ToUpperInvariant();
            shipments = shipments.Where(s => s.ShipmentNumber.Contains(term));
        }

        if (query.Status is { } status)
            shipments = shipments.Where(s => s.Status == status);

        if (query.Type is { } type)
            shipments = shipments.Where(s => s.Type == type);

        if (query.PostOfficeId is { } postOfficeId)
        {
            // Current location: origin while ReceivedAtOrigin, destination afterwards.
            shipments = shipments.Where(s =>
                (s.Status == ShipmentStatus.ReceivedAtOrigin ? s.OriginPostOfficeId : s.DestinationPostOfficeId)
                == postOfficeId);
        }

        shipments = query.WeightCategory switch
        {
            WeightCategory.UnderOneKg => shipments.Where(s => s.WeightKg < Shipment.LightLimitKg),
            WeightCategory.OneToFiveKg => shipments.Where(s =>
                s.WeightKg >= Shipment.LightLimitKg && s.WeightKg <= Shipment.HeavyLimitKg),
            WeightCategory.OverFiveKg => shipments.Where(s => s.WeightKg > Shipment.HeavyLimitKg),
            _ => shipments
        };

        var totalCount = await shipments.CountAsync(cancellationToken);

        var items = await shipments
            .OrderByDescending(s => s.CreatedAt).ThenByDescending(s => s.Id) // stable order for paging
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ToDto)
            .ToListAsync(cancellationToken);

        return new PagedResult<ShipmentDto>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<ShipmentDto?> GetAsync(int id, CancellationToken cancellationToken) =>
        await db.Shipments.AsNoTracking()
            .Where(s => s.Id == id)
            .Select(ToDto)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ShipmentDto> CreateAsync(ShipmentRequest request, CancellationToken cancellationToken)
    {
        await EnsurePostOfficesExistAsync(request, cancellationToken);

        var shipment = new Shipment(request.Type, request.WeightKg, request.OriginPostOfficeId,
            request.DestinationPostOfficeId);
        db.Shipments.Add(shipment);
        await db.SaveChangesAsync(cancellationToken);

        return (await GetAsync(shipment.Id, cancellationToken))!;
    }

    public async Task<ShipmentDto?> UpdateAsync(int id, ShipmentRequest request, CancellationToken cancellationToken)
    {
        var shipment = await db.Shipments.FindAsync([id], cancellationToken);
        if (shipment is null)
            return null;

        await EnsurePostOfficesExistAsync(request, cancellationToken);

        shipment.Update(request.Type, request.WeightKg, request.OriginPostOfficeId, request.DestinationPostOfficeId);
        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(id, cancellationToken);
    }

    public async Task<ShipmentDto?> AdvanceStatusAsync(int id, CancellationToken cancellationToken)
    {
        var shipment = await db.Shipments.FindAsync([id], cancellationToken);
        if (shipment is null)
            return null;

        shipment.AdvanceStatus();
        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var shipment = await db.Shipments.FindAsync([id], cancellationToken);
        if (shipment is null)
            return false;

        db.Shipments.Remove(shipment);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsurePostOfficesExistAsync(ShipmentRequest request, CancellationToken cancellationToken)
    {
        int[] ids = [request.OriginPostOfficeId, request.DestinationPostOfficeId];
        var found = await db.PostOffices.CountAsync(p => ids.Contains(p.Id), cancellationToken);
        if (found != ids.Distinct().Count())
            throw new ArgumentException("Origin or destination post office does not exist.");
    }

    // Translated to SQL by EF Core, so only the needed columns are loaded.
    private static readonly Expression<Func<Shipment, ShipmentDto>> ToDto = s => new ShipmentDto(
        s.Id,
        s.ShipmentNumber,
        s.Type,
        s.WeightKg,
        Shipment.CategoryFor(s.WeightKg),
        s.Status,
        new PostOfficeDto(s.Origin!.Id, s.Origin.ZipCode, s.Origin.Name),
        new PostOfficeDto(s.Destination!.Id, s.Destination.ZipCode, s.Destination.Name),
        s.Status == ShipmentStatus.ReceivedAtOrigin
            ? new PostOfficeDto(s.Origin.Id, s.Origin.ZipCode, s.Origin.Name)
            : new PostOfficeDto(s.Destination.Id, s.Destination.ZipCode, s.Destination.Name),
        s.CreatedAt,
        s.UpdatedAt);
}
