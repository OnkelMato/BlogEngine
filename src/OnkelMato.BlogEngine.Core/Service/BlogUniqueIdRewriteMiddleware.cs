using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Core.Service;

public static class BlogUniqueIdRewriteMiddleware
{
    public static WebApplication OverwriteEmptyBlogIdInSettings(this WebApplication app)
    {
        var s = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var mgmtRepository = s.ServiceProvider.GetService<BlogEngineMgmtRepository>() ?? throw new ArgumentException("Cannot get repository for blog"); ;
        var blogSettings = s.ServiceProvider.GetService<IOptionsMonitor<BlogConfiguration>>() ?? throw new ArgumentException("Cannot get settings for posts");

        if (blogSettings.CurrentValue.BlogUniqueId.HasValue &&
            blogSettings.CurrentValue.BlogUniqueId != Guid.Empty) return app;

        // use first blog from database
        var firstBlog = mgmtRepository.GetFirstBlogOrDefault().Result;
        blogSettings.CurrentValue.BlogUniqueId = firstBlog?.UniqueId ?? Guid.NewGuid();

        return app;
    }
}