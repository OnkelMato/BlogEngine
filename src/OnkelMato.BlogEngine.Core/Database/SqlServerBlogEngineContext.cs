using Microsoft.EntityFrameworkCore;

namespace OnkelMato.BlogEngine.Core.Database;

public class SqlServerBlogEngineContext(string? connectionString = null) : BlogEngineContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer(connectionString ?? @"Server=(localdb)\mssqllocaldb;Database=BlogEngine.MSSQL;Trusted_Connection=True;MultipleActiveResultSets=true");
    }
}