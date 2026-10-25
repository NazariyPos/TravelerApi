using System.ComponentModel.DataAnnotations;

namespace TravelerApi.DTOs;

public class CreateLocationRequest : IValidatableObject
{
    [Required, StringLength(200, MinimumLength = 1)] public string? Name { get; set; }
    public string? Address { get; set; }
    [Range(-90, 90)] public decimal? Latitude { get; set; }
    [Range(-180, 180)] public decimal? Longitude { get; set; }
    public DateTimeOffset? ArrivalDate { get; set; }
    public DateTimeOffset? DepartureDate { get; set; }
    [Range(0, 99999999.99)] public decimal? Budget { get; set; }
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
            yield return new ValidationResult("Name is required", [nameof(Name)]);
        if (Budget.HasValue && decimal.Round(Budget.Value, 2) != Budget.Value)
            yield return new ValidationResult("Budget must have no more than 2 decimal places", [nameof(Budget)]);
        if (ArrivalDate.HasValue && DepartureDate.HasValue && DepartureDate < ArrivalDate)
            yield return new ValidationResult("Departure date must be after arrival date", [nameof(DepartureDate)]);
    }
}

public sealed class UpdateLocationRequest : CreateLocationRequest
{
    [Range(1, int.MaxValue)]
    public int Version { get; set; }
}
