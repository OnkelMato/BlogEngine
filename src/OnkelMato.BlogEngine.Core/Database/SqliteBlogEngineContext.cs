using Microsoft.EntityFrameworkCore;

namespace OnkelMato.BlogEngine.Core.Database;

public class SqliteBlogEngineContext(string? connectionString = null) : BlogEngineContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite(connectionString ?? "Data Source=BlogEngine.SQLite.db");
    }
}