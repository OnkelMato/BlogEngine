using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Core.Service;

public static class BlogSelectorMiddleware
{
    public static WebApplication UseBlogSelector(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var s = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var mgmtRepository = s.ServiceProvider.GetService<BlogEngineMgmtRepository>() ?? throw new ArgumentException("Cannot get repository for blog"); ;
            var blogSettings = s.ServiceProvider.GetService<IOptionsMonitor<BlogConfiguration>>() ?? throw new ArgumentException("Cannot get settings for posts");
       
            var blogUniqueId = Guid.Parse(context.Session.GetString("blogid") ?? Guid.Empty.ToString());

            // this is shot. maybe a session cache or sth
            if (context.Request.Query.TryGetValue("blogid", out var blogId) &&
                Guid.TryParse(blogId, out var blogIdParsed) &&
                blogIdParsed != Guid.Empty)

                blogUniqueId = blogIdParsed;

            if (blogUniqueId == Guid.Empty)
                blogUniqueId = blogSettings.CurrentValue.BlogUniqueId!.Value;

            context.Session.SetString("blogid", blogUniqueId.ToString());

            // create blog if not exist
            if (blogSettings.CurrentValue.AllowBlogCreation && !mgmtRepository.HasBlog(blogUniqueId))
                _ = mgmtRepository.CreateBlog(blogUniqueId).Result;

            await next.Invoke();
        });

        return app;
    }
}