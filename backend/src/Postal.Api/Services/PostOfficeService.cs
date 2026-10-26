using Microsoft.EntityFrameworkCore;
using Postal.Api.Contracts;
using Postal.Api.Data;
using Postal.Api.Domain;

namespace Postal.Api.Services;

public class PostOfficeService(AppDbContext db)
{
    public async Task<IReadOnlyList<PostOfficeDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await db.PostOffices.AsNoTracking()
            .OrderBy(p => p.ZipCode)
            .Select(p => new PostOfficeDto(p.Id, p.ZipCode, p.Name))
            .ToListAsync(cancellationToken);

    public async Task<PostOfficeDto?> GetAsync(int id, CancellationToken cancellationToken) =>
        await db.PostOffices.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PostOfficeDto(p.Id, p.ZipCode, p.Name))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PostOfficeDto> CreateAsync(PostOfficeRequest request, CancellationToken cancellationToken)
    {
        await EnsureZipCodeIsFreeAsync(request.ZipCode, excludeId: null, cancellationToken);

        var postOffice = new PostOffice(request.ZipCode, request.Name);
        db.PostOffices.Add(postOffice);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(postOffice);
    }

    public async Task<PostOfficeDto?> UpdateAsync(int id, PostOfficeRequest request, CancellationToken cancellationToken)
    {
        var postOffice = await db.PostOffices.FindAsync([id], cancellationToken);
        if (postOffice is null)
            return null;

        await EnsureZipCodeIsFreeAsync(request.ZipCode, excludeId: id, cancellationToken);

        postOffice.Update(request.ZipCode, request.Name);
        await db.SaveChangesAsync(cancellationToken);

        return ToDto(postOffice);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var postOffice = await db.PostOffices.FindAsync([id], cancellationToken);
        if (postOffice is null)
            return false;

        var inUse = await db.Shipments.AnyAsync(
            s => s.OriginPostOfficeId == id || s.DestinationPostOfficeId == id, cancellationToken);
        if (inUse)
            throw new DomainException($"Post office {postOffice.ZipCode} is used by shipments and can't be deleted.");

        db.PostOffices.Remove(postOffice);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureZipCodeIsFreeAsync(string zipCode, int? excludeId, CancellationToken cancellationToken)
    {
        var taken = await db.PostOffices.AnyAsync(p => p.ZipCode == zipCode && p.Id != excludeId, cancellationToken);
        if (taken)
            throw new DomainException($"A post office with ZIP code {zipCode} already exists.");
    }

    private static PostOfficeDto ToDto(PostOffice p) => new(p.Id, p.ZipCode, p.Name);
}
