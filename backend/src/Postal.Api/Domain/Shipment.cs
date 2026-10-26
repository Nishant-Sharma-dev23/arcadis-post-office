using System.Security.Cryptography;

namespace Postal.Api.Domain;

public class Shipment
{
    // Weight category boundaries in kg: < 1, 1–5 inclusive, > 5.
    public const decimal LightLimitKg = 1m;
    public const decimal HeavyLimitKg = 5m;

    private const string NumberAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no 0/O or 1/I

    public int Id { get; private set; }
    public string ShipmentNumber { get; private set; } = string.Empty;
    public ShipmentType Type { get; private set; }
    public decimal WeightKg { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public int OriginPostOfficeId { get; private set; }
    public PostOffice? Origin { get; private set; }
    public int DestinationPostOfficeId { get; private set; }
    public PostOffice? Destination { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public WeightCategory WeightCategory => CategoryFor(WeightKg);

    public int CurrentPostOfficeId =>
        Status == ShipmentStatus.ReceivedAtOrigin ? OriginPostOfficeId : DestinationPostOfficeId;

    private Shipment() { } // for EF Core

    public Shipment(ShipmentType type, decimal weightKg, int originPostOfficeId, int destinationPostOfficeId)
    {
        ShipmentNumber = "SHP-" + RandomNumberGenerator.GetString(NumberAlphabet, 8);
        Status = ShipmentStatus.ReceivedAtOrigin;
        CreatedAt = DateTimeOffset.UtcNow;
        Update(type, weightKg, originPostOfficeId, destinationPostOfficeId);
    }

    public void Update(ShipmentType type, decimal weightKg, int originPostOfficeId, int destinationPostOfficeId)
    {
        if (weightKg <= 0)
            throw new ArgumentException("Weight must be greater than 0 kg.", nameof(weightKg));
        if (originPostOfficeId == destinationPostOfficeId)
            throw new ArgumentException("Origin and destination must be different post offices.", nameof(destinationPostOfficeId));

        Type = type;
        WeightKg = weightKg;
        OriginPostOfficeId = originPostOfficeId;
        DestinationPostOfficeId = destinationPostOfficeId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Moves the shipment one step forward: ReceivedAtOrigin → ReceivedAtDestination → Delivered.</summary>
    public void AdvanceStatus()
    {
        Status = Status switch
        {
            ShipmentStatus.ReceivedAtOrigin => ShipmentStatus.ReceivedAtDestination,
            ShipmentStatus.ReceivedAtDestination => ShipmentStatus.Delivered,
            _ => throw new DomainException($"Shipment {ShipmentNumber} is already delivered.")
        };
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static WeightCategory CategoryFor(decimal weightKg) => weightKg switch
    {
        < LightLimitKg => WeightCategory.UnderOneKg,
        <= HeavyLimitKg => WeightCategory.OneToFiveKg,
        _ => WeightCategory.OverFiveKg
    };
}
