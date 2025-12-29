using Microsoft.EntityFrameworkCore;
using OnkelMato.BlogEngine.Core.Database.Entity;

namespace OnkelMato.BlogEngine.Core.Database;

public class SqlServerBlogEngineContext(string? connectionString = null) : BlogEngineContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlServer(connectionString ?? @"Server=(localdb)\mssqllocaldb;Database=BlogEngine.MSSQL;Trusted_Connection=True;MultipleActiveResultSets=true");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostTagDb>()
            .HasOne(pt => pt.Blog)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PostTagDb>()
            .HasOne(pt => pt.Post)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

        base.OnModelCreating(modelBuilder);
    }
}