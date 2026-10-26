using Microsoft.AspNetCore.Mvc;
using Postal.Api.Contracts;
using Postal.Api.Services;

namespace Postal.Api.Controllers;

[ApiController]
[Route("api/post-offices")]
public class PostOfficesController(PostOfficeService service) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<PostOfficeDto>> GetAll(CancellationToken cancellationToken) =>
        await service.GetAllAsync(cancellationToken);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostOfficeDto>> Get(int id, CancellationToken cancellationToken) =>
        await service.GetAsync(id, cancellationToken) is { } postOffice ? postOffice : NotFound();

    [HttpPost]
    public async Task<ActionResult<PostOfficeDto>> Create(PostOfficeRequest request, CancellationToken cancellationToken)
    {
        var postOffice = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = postOffice.Id }, postOffice);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PostOfficeDto>> Update(int id, PostOfficeRequest request,
        CancellationToken cancellationToken) =>
        await service.UpdateAsync(id, request, cancellationToken) is { } postOffice ? postOffice : NotFound();

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken) =>
        await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
