using Microsoft.EntityFrameworkCore;
using Postal.Api.Contracts;
using Postal.Api.Data;
using Postal.Api.Domain;
using Postal.Api.Services;

namespace Postal.Api.Tests.Services;

public class ShipmentServiceTests
{
    private readonly AppDbContext _db;
    private readonly ShipmentService _shipments;
    private readonly PostOffice _berlin = new("10115", "Berlin");
    private readonly PostOffice _hamburg = new("20095", "Hamburg");

    public ShipmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // fresh database per test
            .Options;
        _db = new AppDbContext(options);
        _db.PostOffices.AddRange(_berlin, _hamburg);
        _db.SaveChanges();
        _shipments = new ShipmentService(_db);
    }

    private Shipment AddShipment(decimal weightKg, int advanceSteps = 0, ShipmentType type = ShipmentType.Package)
    {
        var shipment = new Shipment(type, weightKg, _berlin.Id, _hamburg.Id);
        for (var i = 0; i < advanceSteps; i++)
            shipment.AdvanceStatus();
        _db.Shipments.Add(shipment);
        _db.SaveChanges();
        return shipment;
    }

    private Task<PagedResult<ShipmentDto>> List(ShipmentQuery query) =>
        _shipments.ListAsync(query, CancellationToken.None);

    [Fact]
    public async Task Create_generates_unique_number_and_starts_at_origin()
    {
        var request = new ShipmentRequest
        {
            Type = ShipmentType.Letter, WeightKg = 0.1m,
            OriginPostOfficeId = _berlin.Id, DestinationPostOfficeId = _hamburg.Id
        };

        var first = await _shipments.CreateAsync(request, CancellationToken.None);
        var second = await _shipments.CreateAsync(request, CancellationToken.None);

        Assert.NotEqual(first.ShipmentNumber, second.ShipmentNumber);
        Assert.Equal(ShipmentStatus.ReceivedAtOrigin, first.Status);
        Assert.Equal(_berlin.Id, first.CurrentPostOffice.Id);
    }

    [Fact]
    public async Task Create_rejects_unknown_post_office()
    {
        var request = new ShipmentRequest
        {
            Type = ShipmentType.Letter, WeightKg = 0.1m,
            OriginPostOfficeId = _berlin.Id, DestinationPostOfficeId = 999
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _shipments.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task Filters_by_status()
    {
        AddShipment(1m);
        var delivered = AddShipment(1m, advanceSteps: 2);

        var result = await List(new ShipmentQuery { Status = ShipmentStatus.Delivered });

        Assert.Equal(delivered.Id, Assert.Single(result.Items).Id);
    }

    [Fact]
    public async Task Filters_by_weight_category()
    {
        AddShipment(0.5m);
        AddShipment(1m);
        AddShipment(5m);
        AddShipment(5.5m);

        var result = await List(new ShipmentQuery { WeightCategory = WeightCategory.OneToFiveKg });

        Assert.Equal([1m, 5m], result.Items.Select(s => s.WeightKg).Order());
    }

    [Fact]
    public async Task Filters_by_current_location()
    {
        var atOrigin = AddShipment(1m);
        var atDestination = AddShipment(1m, advanceSteps: 1);

        var inBerlin = await List(new ShipmentQuery { PostOfficeId = _berlin.Id });
        var inHamburg = await List(new ShipmentQuery { PostOfficeId = _hamburg.Id });

        Assert.Equal(atOrigin.Id, Assert.Single(inBerlin.Items).Id);
        Assert.Equal(atDestination.Id, Assert.Single(inHamburg.Items).Id);
    }

    [Fact]
    public async Task Filters_by_shipment_number_case_insensitive()
    {
        var target = AddShipment(1m);
        AddShipment(1m);

        var result = await List(new ShipmentQuery { ShipmentNumber = target.ShipmentNumber[4..].ToLowerInvariant() });

        Assert.Equal(target.Id, Assert.Single(result.Items).Id);
    }

    [Fact]
    public async Task Paging_returns_requested_page_and_total_count()
    {
        for (var i = 0; i < 12; i++)
            AddShipment(1m);

        var page2 = await List(new ShipmentQuery { Page = 2, PageSize = 5 });
        var page3 = await List(new ShipmentQuery { Page = 3, PageSize = 5 });

        Assert.Equal(12, page2.TotalCount);
        Assert.Equal(5, page2.Items.Count);
        Assert.Equal(2, page3.Items.Count);
        Assert.Empty(page2.Items.Select(s => s.Id).Intersect(page3.Items.Select(s => s.Id)));
    }

    [Fact]
    public async Task Deleting_a_post_office_in_use_is_rejected()
    {
        AddShipment(1m);
        var postOffices = new PostOfficeService(_db);

        await Assert.ThrowsAsync<DomainException>(() => postOffices.DeleteAsync(_berlin.Id, CancellationToken.None));
    }
}
