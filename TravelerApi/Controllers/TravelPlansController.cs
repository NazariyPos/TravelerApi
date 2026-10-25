using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelerApi.Data;
using TravelerApi.DTOs;
using TravelerApi.Models;

namespace TravelerApi.Controllers;

[ApiController]
[Route("api/travel-plans")]
public sealed class TravelPlansController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var plans = await db.TravelPlans.AsNoTracking().OrderByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                x.Id, x.Title, x.Description, x.StartDate, x.EndDate, x.Budget, x.Currency,
                x.IsPublic, x.Version, x.CreatedAt, x.UpdatedAt,
                LocationCount = x.Locations.Count
            }).ToListAsync(cancellationToken);
        return Ok(plans);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTravelPlanRequest request, CancellationToken cancellationToken)
    {
        var plan = new TravelPlan
        {
            Id = Guid.NewGuid(), Title = request.Title!.Trim(), Description = request.Description,
            StartDate = request.StartDate, EndDate = request.EndDate, Budget = request.Budget,
            Currency = request.Currency, IsPublic = request.IsPublic, Version = 1,
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        };
        db.TravelPlans.Add(plan);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = plan.Id }, plan.ToResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var plan = await db.TravelPlans.AsNoTracking().Include(x => x.Locations)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return plan is null
            ? NotFound(new { error = "Travel plan not found" })
            : Ok(plan.ToDetailsResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTravelPlanRequest request, CancellationToken cancellationToken)
    {
        var exists = await db.TravelPlans.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
        if (!exists) return NotFound(new { error = "Travel plan not found" });

        var newVersion = request.Version + 1;
        var affected = await db.TravelPlans
            .Where(x => x.Id == id && x.Version == request.Version)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Title, request.Title!.Trim())
                .SetProperty(x => x.Description, request.Description)
                .SetProperty(x => x.StartDate, request.StartDate)
                .SetProperty(x => x.EndDate, request.EndDate)
                .SetProperty(x => x.Budget, request.Budget)
                .SetProperty(x => x.Currency, request.Currency)
                .SetProperty(x => x.IsPublic, request.IsPublic)
                .SetProperty(x => x.Version, newVersion)
                .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow), cancellationToken);
        if (affected == 0)
        {
            var currentVersion = await db.TravelPlans.AsNoTracking().Where(x => x.Id == id)
                .Select(x => (int?)x.Version).FirstAsync(cancellationToken);
            return Conflict(new
            {
                error = "Conflict: travel plan was modified by another user",
                current_version = currentVersion
            });
        }

        var updated = await db.TravelPlans.AsNoTracking().FirstAsync(x => x.Id == id, cancellationToken);
        return Ok(updated.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var plan = await db.TravelPlans.FindAsync([id], cancellationToken);
        if (plan is null) return NotFound(new { error = "Travel plan not found" });
        db.TravelPlans.Remove(plan);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
