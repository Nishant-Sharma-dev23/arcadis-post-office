using Microsoft.AspNetCore.Mvc;
using Postal.Api.Contracts;
using Postal.Api.Services;

namespace Postal.Api.Controllers;

[ApiController]
[Route("api/shipments")]
public class ShipmentsController(ShipmentService service) : ControllerBase
{
    [HttpGet]
    public async Task<PagedResult<ShipmentDto>> List([FromQuery] ShipmentQuery query,
        CancellationToken cancellationToken) =>
        await service.ListAsync(query, cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShipmentDto>> Get(int id, CancellationToken cancellationToken) =>
        await service.GetAsync(id, cancellationToken) is { } shipment ? shipment : NotFound();

    [HttpPost]
    public async Task<ActionResult<ShipmentDto>> Create(ShipmentRequest request, CancellationToken cancellationToken)
    {
        var shipment = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = shipment.Id }, shipment);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ShipmentDto>> Update(int id, ShipmentRequest request,
        CancellationToken cancellationToken) =>
        await service.UpdateAsync(id, request, cancellationToken) is { } shipment ? shipment : NotFound();

    [HttpPost("{id:int}/advance-status")]
    public async Task<ActionResult<ShipmentDto>> AdvanceStatus(int id, CancellationToken cancellationToken) =>
        await service.AdvanceStatusAsync(id, cancellationToken) is { } shipment ? shipment : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
