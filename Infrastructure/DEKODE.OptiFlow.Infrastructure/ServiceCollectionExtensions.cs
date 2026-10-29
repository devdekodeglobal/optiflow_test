using DEKODE.OptiFlow.DAL.Repository;
using DEKODE.OptiFlow.Domain.BusinessServices.Engine;
using DEKODE.OptiFlow.Domain.BusinessServices.Services;
using DEKODE.OptiFlow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DEKODE.OptiFlow.Infrastructure;

/// <summary>
/// Composition Root module for wiring Persistence (PostgreSQL / EF Core)
/// and Domain Business Services into the DI container without exposing
/// DAL or database internals to Application / Web API layers.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOptiFlowInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Persistence & DbContext registration (with PostgreSQL snake_case naming conventions)
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<OptiFlowDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Contains("optiflow_demo"))
            {
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
                    npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "public");
                });
            }
            else
            {
                // In-Memory fallback for test execution
                options.UseInMemoryDatabase("OptiFlow_InMemory_DevDb")
                       .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DEKODE.OptiFlow.Domain.Interfaces.Auth.IUserRepository, UserRepository>();

        // 2. Business Services registration
        services.AddScoped<IItemAttributeParser, ItemAttributeParser>();
        services.AddScoped<ISimilarityScorer, WeightedAttributeSimilarityScorer>();
        services.AddScoped<IAllocationEngine, OptiFlowAllocationEngine>();
        services.AddScoped<IPlanogramImporter, PlanogramCsvImporter>();
        services.AddScoped<DEKODE.OptiFlow.Domain.Interfaces.Parsing.IPlanogramCsvParser, PlanogramCsvImporter>();
        services.AddScoped<IPlanogramService, PlanogramService>();
        services.AddScoped<IAllocationService, AllocationService>();
        services.AddScoped<DEKODE.OptiFlow.Domain.Interfaces.Auth.IAuthenticationService, AuthenticationService>();

        return services;
    }

    public static async Task MigrateOptiFlowDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<OptiFlowDbContext>();
        if (dbContext != null && dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync();
        }
    }
}
