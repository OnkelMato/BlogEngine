using Microsoft.EntityFrameworkCore;

namespace Softwarekueche.Web.Infrastructure.Data;

public class SoftwarekuecheHomeContext(DbContextOptions<SoftwarekuecheHomeContext> options)
    : DbContext(options)
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<PostImage> PostImages { get; set; }
}