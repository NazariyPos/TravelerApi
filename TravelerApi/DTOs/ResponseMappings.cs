using TravelerApi.Models;

namespace TravelerApi.DTOs;

public static class ResponseMappings
{
    public static object ToResponse(this TravelPlan plan) => new
    {
        plan.Id, plan.Title, plan.Description, plan.StartDate, plan.EndDate, plan.Budget,
        plan.Currency, plan.IsPublic, plan.Version, plan.CreatedAt, plan.UpdatedAt
    };

    public static object ToDetailsResponse(this TravelPlan plan) => new
    {
        plan.Id, plan.Title, plan.Description, plan.StartDate, plan.EndDate, plan.Budget,
        plan.Currency, plan.IsPublic, plan.Version, plan.CreatedAt, plan.UpdatedAt,
        Locations = plan.Locations.OrderBy(x => x.VisitOrder).Select(x => x.ToResponse())
    };

    public static object ToResponse(this Location location) => new
    {
        location.Id, location.TravelPlanId, location.Version, location.Name, location.Address, location.Latitude,
        location.Longitude, location.VisitOrder, location.ArrivalDate, location.DepartureDate,
        location.Budget, location.Notes, location.CreatedAt
    };
}
