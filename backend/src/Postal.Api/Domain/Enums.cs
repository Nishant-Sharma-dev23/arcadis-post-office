namespace Postal.Api.Domain;

public enum ShipmentType
{
    Letter,
    Package
}

public enum ShipmentStatus
{
    ReceivedAtOrigin,
    ReceivedAtDestination,
    Delivered
}

public enum WeightCategory
{
    UnderOneKg,
    OneToFiveKg,
    OverFiveKg
}
