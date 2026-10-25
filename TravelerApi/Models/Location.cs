namespace TravelerApi.Models;

public sealed class Location
{
    public Guid Id { get; set; }
    public Guid TravelPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public int VisitOrder { get; set; }
    public DateTimeOffset? ArrivalDate { get; set; }
    public DateTimeOffset? DepartureDate { get; set; }
    public decimal? Budget { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public TravelPlan TravelPlan { get; set; } = null!;
}
