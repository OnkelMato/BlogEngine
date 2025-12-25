using OnkelMato.BlogEngine.Core.Database.Entity;
using OnkelMato.BlogEngine.Core.Model;

namespace OnkelMato.BlogEngine.Core.Repository;

public static class DbToModelExtensions
{
    public static ShowState ToModel(this ShowStateDb showStateDb)
    {
        return showStateDb switch
        {
            ShowStateDb.None => ShowState.None,
            ShowStateDb.Blog => ShowState.Blog,
            ShowStateDb.Menu => ShowState.Menu,
            ShowStateDb.Footer => ShowState.Footer,
            ShowStateDb.BlogAndMenu => ShowState.BlogAndMenu,
            ShowStateDb.BlogAndFooter => ShowState.BlogAndFooter,
            ShowStateDb.LinkAndMenu => ShowState.LinkAndMenu,
            ShowStateDb.LinkAndFooter => ShowState.LinkAndFooter,
            _ => ShowState.None,
        };
    }
    public static ShowStateDb FromModel(this ShowState showState)
    {
        return showState switch
        {
            ShowState.None => ShowStateDb.None,
            ShowState.Blog => ShowStateDb.Blog,
            ShowState.Menu => ShowStateDb.Menu,
            ShowState.Footer => ShowStateDb.Footer,
            ShowState.BlogAndMenu => ShowStateDb.BlogAndMenu,
            ShowState.BlogAndFooter => ShowStateDb.BlogAndFooter,
            ShowState.LinkAndMenu => ShowStateDb.LinkAndMenu,
            ShowState.LinkAndFooter => ShowStateDb.LinkAndFooter,
            _ => ShowStateDb.None,
        };
    }
    public static PostImage ToModel(this PostImageDb postImageDb)
    {
        return new PostImage()
        {
            UniqueId = postImageDb.UniqueId,
            AltText = postImageDb.AltText,
            ContentType = postImageDb.ContentType,
            Name = postImageDb.Name,
            Image = postImageDb.Image,
            IsPublished = postImageDb.IsPublished,
            CreatedAt = postImageDb.CreatedAt,
            UpdatedAt = postImageDb.UpdatedAt
        };
    }

    public static Post ToModel(this PostDb postDb)
    {
        return new Post()
        {
            UniqueId = postDb.UniqueId,
            Title = postDb.Title,
            MdPreview = postDb.MdPreview,
            MdContent = postDb.MdContent,
            ShowState = postDb.ShowState.ToModel(),
            CreatedAt = postDb.CreatedAt,
            UpdatedAt = postDb.UpdatedAt,
            PublishedAt = postDb.PublishedAt,
            Order = postDb.Order,
            HeaderImage = postDb.HeaderImage?.ToModel(),
            PostTags = postDb.PostTags?.Select(x => x.Title).ToList() ?? []
        };
    }

    public static PostTag ToModel(this PostTagDb postTagDb)
    {
        return new PostTag()
        {

            Title = postTagDb.Title,
            Blog = postTagDb.Blog.ToModel(),
            Post = postTagDb.Post.ToModel()
        };
    }

    public static Blog ToModel(this BlogDb blogDb)
    {
        return new Blog()
        {
            UniqueId = blogDb.UniqueId,
            Title = blogDb.Title,
            Description = blogDb.Description,
            CreatedAt = blogDb.CreatedAt,
            UpdatedAt = blogDb.UpdatedAt,
            CSS = blogDb.CSS,
            Posts = blogDb.Posts?.Select(x => x.ToModel()).ToList() ?? [],
            PostImages = blogDb.PostImages?.Select(x => x.ToModel()).ToList() ?? [],
            PostTags = blogDb.PostTags?.Select(x => x.ToModel()).ToList() ?? []
        };
    }
}