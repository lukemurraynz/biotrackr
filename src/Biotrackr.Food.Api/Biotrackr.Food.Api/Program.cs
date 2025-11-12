using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Biotrackr.Food.Api.Extensions;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.OpenApi.Models;

[ExcludeFromCodeCoverage]
public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();
        var managedIdentityClientId = builder.Configuration.GetValue<string>("managedidentityclientid");
        var azureAppConfigEndpoint = builder.Configuration.GetValue<string>("azureappconfigendpoint");

        // Only load Azure App Configuration if endpoint is provided (not in test environment)
        if (!string.IsNullOrWhiteSpace(azureAppConfigEndpoint))
        {
            builder.Configuration.AddAzureAppConfiguration(config =>
            {
                config.Connect(new Uri(azureAppConfigEndpoint),
                    new ManagedIdentityCredential(managedIdentityClientId))
                .Select(KeyFilter.Any, LabelFilter.Null);
            });
        }

        // Add Cosmos DB services
        builder.Services.AddCosmosDb(builder.Configuration);

        // Add API documentation
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "1.0.0",
                Title = "Biotrackr Food API",
                Description = "Web API for Food and Nutrition data",
                Contact = new OpenApiContact
                {
                    Name = "Biotrackr",
                    Url = new Uri("https://github.com/willvelida/biotrackr")
                }
            });
        });

        // Add health checks
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        // Configure Swagger
        app.UseSwagger();
        app.UseSwaggerUI();

        // Map endpoints
        app.RegisterFoodEndpoints();
        app.RegisterHealthCheckEndpoints();

        app.Run();
    }
}

public partial class Program { }
