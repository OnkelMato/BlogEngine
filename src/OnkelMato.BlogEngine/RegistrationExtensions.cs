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

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.Configure<PostsConfiguration>(builder.Configuration.GetSection("Posts"));
        builder.Services.AddScoped<BlogEngineRepository>(); // scoped because DbContext (dependency) is scoped

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
        var settings = s.ServiceProvider.GetService<IOptionsMonitor<PostsConfiguration>>() ?? throw new ArgumentException("Cannot get settings for posts");

        // this is the perfect way and blog is configured correctly
        if (settings.CurrentValue.BlogUniqueId != Guid.Empty && db.Blogs.Count(x => x.UniqueId == settings.CurrentValue.BlogUniqueId) == 1)
            return app;

        // Blog id is not set but in case there is only one DB, this should be used
        if (settings.CurrentValue.BlogUniqueId == Guid.Empty && db.Blogs.Count() == 1 && settings.CurrentValue.UseSingleBlog)
        {
            // use blog if not set
            var blog = db.Blogs.Single();
            Console.WriteLine($@"Existing single blog used with id '{blog.UniqueId}'");

            settings.CurrentValue.BlogUniqueId = blog.UniqueId;
            return app;
        }

        // A blog id is set (or not), cannot be found and the blog is not created (and should be created)
        if (settings.CurrentValue.CreateBlogIfNotExist)
        {
            var id = settings.CurrentValue.BlogUniqueId == Guid.Empty ? Guid.NewGuid() : settings.CurrentValue.BlogUniqueId;
            // create blog if not exists and use it
            var blog = new Blog()
            {
                UniqueId = id,
                Title = $"My Blog #{id}",
                Description = "My Blog",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            db.Blogs.Add(blog);

            db.Posts.AddFooterLink(blog, Guid.Parse("F5C50612-DAAA-44A3-AA2B-49090DE41F90"), "Onkel Mato Blog Engine", "https://github.com/OnkelMato/BlogEngine");
            db.Posts.AddFooterLink(blog, Guid.Parse("1A12B373-7E48-4E01-B040-DA896CE67F75"), "Admin", "/Admin", 20000);
            db.Posts.AddFooter(blog, Guid.Parse("1B5FBEA4-10FE-46DD-AC59-7573F54DFDBB"), "Legal Notice", "## Legal Notice", 15100);
            db.Posts.AddFooter(blog, Guid.Parse("D20F7182-1CB6-4A22-AD20-3B49B47D391F"), "Data Protection", "## Data protection", 15000);
            
            db.SaveChanges();

            Console.WriteLine($@"New blog created with id '{blog.UniqueId}'");

            settings.CurrentValue.BlogUniqueId = blog.UniqueId;
            return app;
        }

        // validate of blog is set correctly
        if (settings.CurrentValue.BlogUniqueId == Guid.Empty)
            throw new InvalidOperationException("BlogUniqueId is not set in configuration and no blog exists. Please create a blog or set BlogUniqueId in configuration.");
        if (!db.Blogs.Any(x => x.UniqueId == settings.CurrentValue.BlogUniqueId))
            throw new InvalidOperationException($"Blog with id '{settings.CurrentValue.BlogUniqueId}' does not exist. Please create a blog or set BlogUniqueId in configuration.");

        throw new ArgumentException("Cannot determine or create blog. Please set the blog Uid in App Settings.");
    }

    private static void AddFooter(this DbSet<Post> source, Blog blog, Guid id, string title, string content, int order = 10000)
    {
        var post = new Post()
        {
            Title = title,
            MdContent = content,
            MdPreview = ".",
            ShowState = ShowState.Footer,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            PublishedAt = DateTime.Now,
            UniqueId = id,
            Order = order,
            Blog = blog,
        };

        source.Add(post);
    }
    private static void AddFooterLink(this DbSet<Post> source, Blog blog, Guid id, string title, string link, int order = 10000)
    {
        var post = new Post()
        {
            Title = title,
            MdPreview = link,
            ShowState = ShowState.LinkAndFooter,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            PublishedAt = DateTime.Now,
            UniqueId = id,
            Order = order,
            Blog = blog,
        };

        source.Add(post);
    }
}