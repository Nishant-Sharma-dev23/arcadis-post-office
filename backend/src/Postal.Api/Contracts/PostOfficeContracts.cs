using System.ComponentModel.DataAnnotations;

namespace Postal.Api.Contracts;

public record PostOfficeDto(int Id, string ZipCode, string Name);

public class PostOfficeRequest
{
    [RegularExpression(@"^\d{5}$", ErrorMessage = "ZIP code must be exactly 5 digits.")]
    public required string ZipCode { get; init; }

    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; init; }
}
