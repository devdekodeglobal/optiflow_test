using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DEKODE.OptiFlow.DAL.Repository;

/// <summary>
/// Design-time factory for EF Core CLI tool to discover and generate migrations
/// without requiring a running web host or active connection.
/// </summary>
public class OptiFlowDbContextFactory : IDesignTimeDbContextFactory<OptiFlowDbContext>
{
    public OptiFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OptiFlowDbContext>();

        // Default connection string used for PostgreSQL schema generation
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
            ?? "Host=localhost;Port=5433;Database=optiflow_db;Username=optiflow_user;Password=optiflow_password_2026";
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(OptiFlowDbContext).Assembly.FullName));

        return new OptiFlowDbContext(optionsBuilder.Options);
    }
}
