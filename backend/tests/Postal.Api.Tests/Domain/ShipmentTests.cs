using Postal.Api.Domain;

namespace Postal.Api.Tests.Domain;

public class ShipmentTests
{
    [Theory]
    [InlineData(0.5, WeightCategory.UnderOneKg)]
    [InlineData(0.999, WeightCategory.UnderOneKg)]
    [InlineData(1, WeightCategory.OneToFiveKg)]
    [InlineData(5, WeightCategory.OneToFiveKg)]
    [InlineData(5.01, WeightCategory.OverFiveKg)]
    public void WeightCategory_is_derived_from_weight(decimal weightKg, WeightCategory expected)
    {
        var shipment = new Shipment(ShipmentType.Package, weightKg, 1, 2);

        Assert.Equal(expected, shipment.WeightCategory);
    }

    [Fact]
    public void New_shipment_starts_at_origin_with_generated_number()
    {
        var shipment = new Shipment(ShipmentType.Letter, 0.2m, 1, 2);

        Assert.Equal(ShipmentStatus.ReceivedAtOrigin, shipment.Status);
        Assert.Matches("^SHP-[A-Z0-9]{8}$", shipment.ShipmentNumber);
        Assert.Equal(1, shipment.CurrentPostOfficeId);
    }

    [Fact]
    public void AdvanceStatus_moves_forward_and_updates_current_location()
    {
        var shipment = new Shipment(ShipmentType.Letter, 0.2m, 1, 2);

        shipment.AdvanceStatus();
        Assert.Equal(ShipmentStatus.ReceivedAtDestination, shipment.Status);
        Assert.Equal(2, shipment.CurrentPostOfficeId);

        shipment.AdvanceStatus();
        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
    }

    [Fact]
    public void AdvanceStatus_throws_when_already_delivered()
    {
        var shipment = new Shipment(ShipmentType.Letter, 0.2m, 1, 2);
        shipment.AdvanceStatus();
        shipment.AdvanceStatus();

        Assert.Throws<DomainException>(shipment.AdvanceStatus);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Weight_must_be_positive(decimal weightKg)
    {
        Assert.Throws<ArgumentException>(() => new Shipment(ShipmentType.Package, weightKg, 1, 2));
    }

    [Fact]
    public void Origin_and_destination_must_differ()
    {
        Assert.Throws<ArgumentException>(() => new Shipment(ShipmentType.Package, 2m, 1, 1));
    }
}
