using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelerApi.Data;
using TravelerApi.DTOs;
using TravelerApi.Models;

namespace TravelerApi.Controllers;

[ApiController]
public sealed class LocationsController(AppDbContext db) : ControllerBase
{
    [HttpPost("api/travel-plans/{planId:guid}/locations")]
    public async Task<IActionResult> Create(Guid planId, CreateLocationRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var plan = await db.TravelPlans.FromSqlInterpolated(
            $"SELECT * FROM travel_plans WHERE id = {planId} FOR UPDATE")
            .AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (plan is null) return NotFound(new { error = "Travel plan not found" });

        var nextOrder = (await db.Locations.Where(x => x.TravelPlanId == planId)
            .MaxAsync(x => (int?)x.VisitOrder, cancellationToken) ?? 0) + 1;
        var location = new Location
        {
            Id = Guid.NewGuid(), TravelPlanId = planId, Name = request.Name!.Trim(),
            Address = request.Address, Latitude = request.Latitude, Longitude = request.Longitude,
            VisitOrder = nextOrder, ArrivalDate = request.ArrivalDate,
            DepartureDate = request.DepartureDate, Budget = request.Budget, Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Locations.Add(location);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Created($"/api/locations/{location.Id}", location.ToResponse());
    }

    [HttpPut("api/locations/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateLocationRequest request, CancellationToken cancellationToken)
    {
        var location = await db.Locations.FindAsync([id], cancellationToken);
        if (location is null) return NotFound(new { error = "Location not found" });
        location.Name = request.Name?.Trim() ?? location.Name;
        location.Address = request.Address;
        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;
        location.ArrivalDate = request.ArrivalDate;
        location.DepartureDate = request.DepartureDate;
        location.Budget = request.Budget;
        location.Notes = request.Notes;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(location.ToResponse());
    }

    [HttpDelete("api/locations/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var location = await db.Locations.FindAsync([id], cancellationToken);
        if (location is null) return NotFound(new { error = "Location not found" });
        db.Locations.Remove(location);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
