using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnkelMato.BlogEngine.Database;

public class BlogEngineContextFactory : IDesignTimeDbContextFactory<BlogEngineContext>
{
    // cf: https://learn.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli
    public BlogEngineContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogEngineContext>();
        optionsBuilder.UseSqlServer(@"Server=(localdb)\\mssqllocaldb;Database=OnkelMatoBlogEngine;Trusted_Connection=True;MultipleActiveResultSets=true");
        //optionsBuilder.UseSqlite("Data Source=OnkelMato.BlogEngine.db");

        return new BlogEngineContext(optionsBuilder.Options);
    }
}
