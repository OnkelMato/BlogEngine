namespace OnkelMato.BlogEngine.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddBlogEngine();
        builder.Services.AddRazorPages(options =>
        {
            options.Conventions.AddPageRoute("/Posts", "/Post/{titleStub}/{id}");
        });
        builder.Services.AddBlogSeoTags();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.EnsureDatabase();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();
        app.Run();
    }
}

// static class for seo extensions
public static class SeoExtensions
{
    public static IServiceCollection AddBlogSeoTags(this IServiceCollection services)
    {
        var db = services.BuildServiceProvider().GetService<BlogEngineRepository>() ?? throw new Exception("Cannot init Database");
        var blogTitle = db.Blog()?.Title ?? "Onkel Mato Blog Engine";

        //Register your services
        services.AddSeoTags(seoInfo =>
        {
            seoInfo.SetSiteInfo(
                siteTitle: blogTitle,
                //openSearchUrl: "https://site.com/open-search.xml",  //Optional
                robots: "index, follow"                             //Optional
            );

            //Optional
            //seoInfo.AddFeed(
            //    title: "Post Feeds",
            //    url: "https://site.com/rss/",
            //    feedType: FeedType.Rss);

            //Optional
            seoInfo.SetLocales("de_DE");
        }); return services;
    }
}