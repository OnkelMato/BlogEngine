using Microsoft.EntityFrameworkCore;

namespace OnkelMato.BlogEngine.Database;

public class BlogEngineContext : DbContext
{
    public BlogEngineContext(DbContextOptions<BlogEngineContext> options) : base(options) { }
    public BlogEngineContext() : base() { }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostImage> PostImages { get; set; }

}

public class SqliteBlogEngineContext(string? connectionString = null) : BlogEngineContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite(connectionString ?? "Data Source=BlogEngine.SQLite.db");
    }
}

public class SqlServerBlogEngineContext(string? connectionString = null) : BlogEngineContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer(connectionString ?? @"Server=(localdb)\\mssqllocaldb;Database=BlogEngine.MSSQL;Trusted_Connection=True;MultipleActiveResultSets=true");
    }
}
