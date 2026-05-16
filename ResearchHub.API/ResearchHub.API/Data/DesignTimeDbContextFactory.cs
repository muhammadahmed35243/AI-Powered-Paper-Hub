using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ResearchHub.Shared;

namespace ResearchHub.API.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        EnvLoader.Load();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=ResearchHubDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
