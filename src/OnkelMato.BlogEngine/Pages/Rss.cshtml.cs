using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Pages;

public class RssModel(
    IBlogEngineRssProvider rssProvider,
    IOptionsMonitor<BlogConfiguration> blogConfiguration) : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        // create feed xml
        var feed = blogConfiguration.CurrentValue.EnableRssFeed
            ? await rssProvider.RetrieveSyndicationFeed()
            : await rssProvider.RetrieveEmptyFeed();

        var settings = new XmlWriterSettings
        {
            Async = true,
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };

        // return feed stream/file
        await using var stream = new MemoryStream();
        await using (var xmlWriter = XmlWriter.Create(stream, settings))
        {
            var rssFormatter = new Rss20FeedFormatter(feed, false);
            rssFormatter.WriteTo(xmlWriter);
            await xmlWriter.FlushAsync();
        }

        return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
    }
}