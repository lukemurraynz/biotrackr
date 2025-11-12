using Biotrackr.Food.Api.EndpointHandlers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Biotrackr.Food.Api.Extensions
{
    public static class EndpointRouteBuilderExtensions
    {
        public static void RegisterFoodEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var foodEndpoints = endpointRouteBuilder.MapGroup("/");

            foodEndpoints.MapGet("/", FoodHandlers.GetAllFoodLogs)
                .WithName("GetAllFoodLogs")
                .WithOpenApi()
                .WithSummary("Get all Food Logs")
                .WithDescription("You can get all food logs via this endpoint");

            foodEndpoints.MapGet("/{date}", FoodHandlers.GetFoodLogByDate)
                .WithName("GetFoodLogByDate")
                .WithOpenApi()
                .WithSummary("Get a Food Log by providing a date")
                .WithDescription("You can get a specific food log via this endpoint by providing the date in the following format (YYYY-MM-DD)");

            foodEndpoints.MapGet("/range/{startDate}/{endDate}", FoodHandlers.GetFoodLogsByDateRange)
                .WithName("GetFoodLogsByDateRange")
                .WithOpenApi()
                .WithSummary("Gets food log documents within a date range with pagination")
                .WithDescription("Gets paginated food log documents between the specified start and end dates (inclusive). Supports optional pageNumber and pageSize query parameters.");
        }

        public static void RegisterHealthCheckEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            var healthEndpoints = endpointRouteBuilder.MapGroup("/healthz");

            healthEndpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            });
        }
    }
}
