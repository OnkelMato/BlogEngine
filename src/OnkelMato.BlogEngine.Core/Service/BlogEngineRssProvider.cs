using Markdig;
using OnkelMato.BlogEngine.Core.Repository;
using System.ServiceModel.Syndication;

namespace OnkelMato.BlogEngine.Core.Service;

public class BlogEngineRssProvider(
    BlogEngineReadRepository repository,
    ILinkFactory linkFactory) : IBlogEngineRssProvider
{
    public async Task<SyndicationFeed> RetrieveSyndicationFeed()
    {
        var blog = repository.Blog()!;
        var posts = repository.PostsOnBlog(1, 25).ToArray();
        var lastUpdateTime = posts.Max(x => x.UpdatedAt);

        var feed = new SyndicationFeed(
            blog.Title,
            blog.Description,
            null,
            posts.Select(x =>
            {
                var result = new SyndicationItem()
                {
                    Title = new TextSyndicationContent(x.Title),
                    Content = new TextSyndicationContent(Markdown.ToHtml(x.MdContent ?? string.Empty),
                        TextSyndicationContentKind.Html),
                    Id = x.UniqueId.ToString(),
                    Summary = new TextSyndicationContent(Markdown.ToHtml(x.MdPreview), TextSyndicationContentKind.Html),
                    BaseUri = new Uri(linkFactory.CreatePostLink(x)),
                    Links =
                    {
                        new SyndicationLink(new Uri(linkFactory.CreatePostLink(x))),
                    },
                    PublishDate = x.PublishedAt,
                    LastUpdatedTime = x.UpdatedAt,
                };
                x.PostTags.Select(y => new SyndicationCategory(y)).ToList().ForEach(x => result.Categories.Add(x));
                return result;
            }))
        {
            LastUpdatedTime = lastUpdateTime,
            Description = new TextSyndicationContent(blog.Description),
            Title = new TextSyndicationContent(blog.Title)
        };

        return feed;
    }

    public async Task<SyndicationFeed> RetrieveEmptyFeed()
    {
        var blog = repository.Blog()!;

        var feed = new SyndicationFeed(
            blog.Title,
            blog.Description,
            null,
            [ 
                new SyndicationItem()
                {
                    Title = new TextSyndicationContent("RSS feed are not enabled"),
                    Content = new TextSyndicationContent("contact your administrator to enable RSS feeds at this blog")
                }
            ])
        {
            LastUpdatedTime = DateTime.Now.AddYears(-10), // to ensure when activated it is newer
            Description = new TextSyndicationContent(blog.Description),
            Title = new TextSyndicationContent(blog.Title)
        };

        return feed;
    }
}
