using System;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Web;

internal static class SeoExtensions
{
    public class SeoTagsSeoProvider(BlogEngineReadRepository readRepository) : ISeoProvider
    {
        public void SetSiteInfo(IHtmlHelper html, string siteTitle, string siteDescription, string robots = "index, follow")
        {
            html.SetSeoInfo(seo =>
            {
                seo.SetCommonInfo(siteTitle, siteDescription, html.ViewContext.HttpContext.Request.GetDisplayUrl(), []);
                seo.SetSiteInfo(readRepository.Blog()?.Title ?? "Onkel Mato Blog Engine", robots: robots);
            });
        }
    }
    public static IServiceCollection AddBlogSeoTags(this IServiceCollection services)
    {
        services.AddTransient<ISeoProvider, SeoTagsSeoProvider>();

        //Register your services
        services.AddSeoTags(seoInfo =>
        {
            var serviceProvider = services.BuildServiceProvider();
            var blogSettings = serviceProvider.GetService<IOptionsMonitor<BlogConfiguration>>() ?? throw new Exception("Cannot get blog settings");
            var blogTitle = "Onkel Mato Blog Engine";

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
            seoInfo.SetLocales(blogSettings.CurrentValue.Language);
        });
        return services;
    }
}