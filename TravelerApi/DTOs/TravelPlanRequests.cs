using System.ComponentModel.DataAnnotations;

namespace TravelerApi.DTOs;

public class CreateTravelPlanRequest : IValidatableObject
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [Range(0, 99999999.99)] public decimal? Budget { get; set; }
    [RegularExpression("^[A-Z]{3}$")] public string Currency { get; set; } = "USD";
    public bool IsPublic { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
            yield return new ValidationResult("Title is required", [nameof(Title)]);
        if (Budget.HasValue && decimal.Round(Budget.Value, 2) != Budget.Value)
            yield return new ValidationResult("Budget must have no more than 2 decimal places", [nameof(Budget)]);
        if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
            yield return new ValidationResult("End date must be after start date", [nameof(EndDate)]);
    }
}

public sealed class UpdateTravelPlanRequest : CreateTravelPlanRequest
{
    [Range(1, int.MaxValue)] public int Version { get; set; }
}
