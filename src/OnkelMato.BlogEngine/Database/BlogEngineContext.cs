using Microsoft.EntityFrameworkCore;

namespace OnkelMato.BlogEngine.Database;

public class BlogEngineContext(DbContextOptions<BlogEngineContext> options)
    : DbContext(options)
{

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostImage> PostImages { get; set; }

}