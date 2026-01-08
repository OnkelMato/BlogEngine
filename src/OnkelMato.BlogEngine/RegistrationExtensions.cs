using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Database;
using OnkelMato.BlogEngine.Core.Repository;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine;

public static class RegistrationExtensions
{
    public static WebApplicationBuilder AddRssFeed(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IBlogEngineRssProvider, BlogEngineRssProvider>();
        return builder;
    }

    public static WebApplicationBuilder AddBlogEngine(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages().AddApplicationPart(typeof(RegistrationExtensions).Assembly);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                               throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        var databaseProvider = builder.Configuration.GetConnectionString("DefaultProvider") ?? "mssql";
        var useBlogSelectorMiddleware = builder.Configuration.GetValue<bool>("Blog:EnableBlogSelection");

        if (string.Compare(databaseProvider, "mssql", StringComparison.InvariantCultureIgnoreCase) == 0)
        {
            builder.Services
                .AddDbContext<BlogEngineContext>(options =>
                    options.UseSqlServer(connectionString));
            // here we need the concrete context because of automatic DCL script creation and deployment
            EnsureDatabase(new SqlServerBlogEngineContext(connectionString));
        }
        else if (string.Compare(databaseProvider, "sqlite", StringComparison.InvariantCultureIgnoreCase) == 0)
        {
            builder.Services
                .AddDbContext<BlogEngineContext>(options =>
                    options.UseSqlite(connectionString));
            // here we need the concrete context because of automatic DCL script creation and deployment
            EnsureDatabase(new SqliteBlogEngineContext(connectionString));
        }
        else
        {
            throw new InvalidOperationException($"Database provider '{databaseProvider}' is not supported.");
        }

        //builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.Configure<BlogConfiguration>(builder.Configuration.GetSection("Blog"));
        builder.Services.Configure<ImportExportConfiguration>(builder.Configuration.GetSection("ImportExport"));

        builder.Services.AddScoped<BlogEngineReadRepository>(); // scoped because DbContext (dependency) is scoped
        builder.Services.AddScoped<BlogEngineEditRepository>(); // scoped because DbContext (dependency) is scoped
        builder.Services.AddScoped<BlogEngineMgmtRepository>(); // scoped because DbContext (dependency) is scoped
        builder.Services.AddScoped<BlogEngineImportExportRepository>(); // scoped because DbContext (dependency) is scoped

        // by using a middleware which is only enabled when configured, it cannot be used accidentally
        if (useBlogSelectorMiddleware)
        // use the session based blog id provider that is maintained via middleware
        {
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(10);
                options.Cookie.IsEssential = true;
            });
            builder.Services.AddScoped<IBlogIdProvider, SessionBlogIdProvider>();
        }
        else
            // use the configured blog id provider that reads from configuration
            builder.Services.AddScoped<IBlogIdProvider, ConfiguredBlogIdProvider>();


        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromDays(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddAntiforgery(options =>
        {
            options.Cookie.Expiration = TimeSpan.FromDays(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
        return builder;
    }

    /// <summary>
    /// Parameter must be the concrete type depending on database provider
    /// </summary>
    /// <param name="db"></param>
    public static void EnsureDatabase(BlogEngineContext db)
    {
        var missing = db.Database.GetPendingMigrations();
        if (missing.Any())
            db.Database.Migrate();
    }

    public static WebApplication EnsureDatabase(this WebApplication app)
    {
        // create database if not exists
        var s = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var db = s.ServiceProvider.GetRequiredService<BlogEngineContext>();
        var missing = db.Database.GetPendingMigrations();
        if (missing.Any())
            db.Database.Migrate();
        s.Dispose();

        app.EnsureBlog();

        return app;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>It is important that with the default settings, no fallback is used.</remarks>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static WebApplication EnsureBlog(this WebApplication app)
    {
        using var s = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var db = s.ServiceProvider.GetRequiredService<BlogEngineContext>();
        var mgmtContext = s.ServiceProvider.GetService<BlogEngineMgmtRepository>()!;
        var settings = s.ServiceProvider.GetService<IOptionsMonitor<BlogConfiguration>>() ??
                       throw new ArgumentException("Cannot get settings for posts");
        var blogId = s.ServiceProvider.GetService<IBlogIdProvider>() ??
                     throw new ArgumentException("Cannot get blog id provider");
        blogId = new StaticFallbackBlogIdProviderDecorator(blogId, settings.CurrentValue.BlogUniqueId ?? Guid.Empty);

        app.UseSession();

        // todo logging with selected strategy

        // rewrite blog unique id in case it is configured. This requires another IBlogIdProvider implementation
        app.OverwriteEmptyBlogIdInSettings(); // blogid rewrite needs to be before
        if (settings.CurrentValue.EnableBlogSelection)
            app.UseBlogSelector();

        // this is the perfect way and blog is configured correctly
        if (blogId.Id != Guid.Empty && db.Blogs.Any(x => x.UniqueId == blogId.Id))
            return app;

        // in case no id is set and there is no blog, we will create one
        if (blogId.Id == Guid.Empty && !db.Blogs.Any())
        {
            // create blog with new random guid
            _ = mgmtContext.CreateBlog(Guid.NewGuid()).Result;
            // let's continue with the next if-statement to load the blog :)
        }

        // Blog id is not set but in case there is only one DB, this should be used
        if (blogId.Id == Guid.Empty && db.Blogs.Count() > 1)
        {
            // use blog if not set
            var blog = db.Blogs.First();

            // add id to session. 
            settings.CurrentValue.BlogUniqueId = blog.UniqueId;
            return app;
        }

        // A blog id is set (or was set by code), and cannot be found it should be created
        if (blogId.Id != Guid.Empty)
        {
            _ = mgmtContext.CreateBlog(blogId.Id).Result;
            return app;
        }

        throw new InvalidOperationException("BlogUniqueId is not set in configuration and no blog exists. Please create a blog or set BlogUniqueId in configuration.");
    }
}
