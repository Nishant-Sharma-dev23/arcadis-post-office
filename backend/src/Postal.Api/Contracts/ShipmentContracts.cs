using System.ComponentModel.DataAnnotations;
using Postal.Api.Domain;

namespace Postal.Api.Contracts;

public record ShipmentDto(
    int Id,
    string ShipmentNumber,
    ShipmentType Type,
    decimal WeightKg,
    WeightCategory WeightCategory,
    ShipmentStatus Status,
    PostOfficeDto Origin,
    PostOfficeDto Destination,
    PostOfficeDto CurrentPostOffice,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public class ShipmentRequest : IValidatableObject
{
    public required ShipmentType Type { get; init; }

    [Range(0.001, 99999.999)]
    public required decimal WeightKg { get; init; }

    public required int OriginPostOfficeId { get; init; }

    public required int DestinationPostOfficeId { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OriginPostOfficeId == DestinationPostOfficeId)
            yield return new ValidationResult("Origin and destination must be different post offices.",
                [nameof(DestinationPostOfficeId)]);
    }
}

/// <summary>Query string for GET /api/shipments. All filters are optional.</summary>
public class ShipmentQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    public string? ShipmentNumber { get; init; }
    public ShipmentStatus? Status { get; init; }
    public int? PostOfficeId { get; init; }
    public WeightCategory? WeightCategory { get; init; }
    public ShipmentType? Type { get; init; }
}
