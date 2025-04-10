using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine;

public static class RegistrationExtensions {
    public static WebApplicationBuilder AddBlogEngine(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages().AddApplicationPart(typeof(RegistrationExtensions).Assembly);
        
            // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        var databaseProvider = builder.Configuration.GetConnectionString("DefaultProvider");//?? "mssql";

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

        return app;
    }
}