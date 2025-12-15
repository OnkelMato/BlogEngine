using Microsoft.EntityFrameworkCore;
using OnkelMato.BlogEngine.Core.Database.Entity;

namespace OnkelMato.BlogEngine.Core.Database;

public class BlogEngineContext : DbContext
{
    public BlogEngineContext(DbContextOptions<BlogEngineContext> options) : base(options) { }
    public BlogEngineContext() : base() { }

    public DbSet<BlogDb> Blogs { get; set; }
    public DbSet<PostDb> Posts { get; set; }
    public DbSet<PostImageDb> PostImages { get; set; }
    public DbSet<PostTagDb> PostTags { get; set; }

}