using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine;

public static class RegistrationExtensions
{
    public static WebApplicationBuilder AddBlogEngine(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages().AddApplicationPart(typeof(RegistrationExtensions).Assembly);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        var databaseProvider = builder.Configuration.GetConnectionString("DefaultProvider") ?? "mssql";

        // strategy pattern?
        if (string.Compare(databaseProvider, "mssql", StringComparison.InvariantCultureIgnoreCase) == 0)
        {
            builder.Services
                .AddDbContext<BlogEngineContext>(options =>
                    options.UseSqlServer(connectionString));
        }
        else if (string.Compare(databaseProvider, "sqlite", StringComparison.InvariantCultureIgnoreCase) == 0)
        {
            builder.Services
                .AddDbContext<BlogEngineContext>(options =>
                    options.UseSqlite(connectionString));
        }
        else
        {
            throw new InvalidOperationException($"Database provider '{databaseProvider}' is not supported.");
        }

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.Configure<PostsConfiguration>(builder.Configuration.GetSection("Posts"));

        return builder;
    }

    public static WebApplication EnsureDatabase(this WebApplication app)
    {
        // create database if not exists
        var s = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var db = s.ServiceProvider.GetRequiredService<BlogEngineContext>();
        var missing = db.Database.GetPendingMigrations();
        if (missing.Any())
            db.Database.Migrate();
        var cfg = s.ServiceProvider.GetRequiredService<IOptions<PostsConfiguration>>();
        s.Dispose();

        app.EnsureBlog();

        return app;
    }

    public static WebApplication EnsureBlog(this WebApplication app)
    {
        using var s = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var db = s.ServiceProvider.GetRequiredService<BlogEngineContext>();
        var settings = s.ServiceProvider.GetService<IOptionsMonitor<PostsConfiguration>>() ?? throw new ArgumentException("Cannot get settings for posts");

        // this is the perfect way and blog is configured correctly
        if (db.Blogs.Count(x => x.UniqueId == settings.CurrentValue.BlogUniqueId) == 1)
            return app;

        // there is a new DB without blog
        if (!db.Blogs.Any() && settings.CurrentValue.CreateBlogIfNotExist)
        {
            var id = settings.CurrentValue.BlogUniqueId == Guid.Empty ? Guid.NewGuid() : settings.CurrentValue.BlogUniqueId;
            // create blog if not exists and use it
            var blog = new Blog()
            {
                UniqueId = id,
                Title = "My Blog",
                Description = "My Blog",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            db.Blogs.Add(blog);
            db.SaveChanges();

            Console.WriteLine($@"New blog created with id '{blog.UniqueId}'");

            settings.CurrentValue.BlogUniqueId = blog.UniqueId;
        }
        // if there is exactly one blog, and we are allowed to use it, do so
        else if (settings.CurrentValue.BlogUniqueId == Guid.Empty && db.Blogs.Count() == 1 && settings.CurrentValue.UseSingleBlog)
        {
            // use blog if not set
            var blog = db.Blogs.Single();
            Console.WriteLine($@"Existing single blog used with id '{blog.UniqueId}'");

            settings.CurrentValue.BlogUniqueId = blog.UniqueId;
        }

        // validate of blog is set correctly
        if (settings.CurrentValue.BlogUniqueId == Guid.Empty)
            throw new InvalidOperationException("BlogUniqueId is not set in configuration and no blog exists. Please create a blog or set BlogUniqueId in configuration.");
        if (!db.Blogs.Any(x => x.UniqueId == settings.CurrentValue.BlogUniqueId))
            throw new InvalidOperationException($"Blog with id '{settings.CurrentValue.BlogUniqueId}' does not exist. Please create a blog or set BlogUniqueId in configuration.");

        return app;
    }
}