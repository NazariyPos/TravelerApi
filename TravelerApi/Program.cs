using System.Diagnostics;
using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelerApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var details = context.ModelState.Values.SelectMany(x => x.Errors)
            .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Invalid value" : x.ErrorMessage)
            .Distinct().ToArray();
        return new BadRequestObjectResult(new
        {
            error = "Validation error",
            details,
            timestamp = DateTimeOffset.UtcNow
        });
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/health", async (AppDbContext db, CancellationToken cancellationToken) =>
{
    var started = Stopwatch.GetTimestamp();
    try
    {
        await db.Database.CanConnectAsync(cancellationToken);
        return Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTimeOffset.UtcNow,
            uptime = Environment.TickCount64,
            database = new
            {
                status = "healthy",
                responseTime = Stopwatch.GetElapsedTime(started).TotalMilliseconds
            }
        });
    }
    catch (DbException)
    {
        return Results.Json(new
        {
            status = "unhealthy",
            timestamp = DateTimeOffset.UtcNow,
            database = new { status = "unhealthy" }
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();

public partial class Program;
