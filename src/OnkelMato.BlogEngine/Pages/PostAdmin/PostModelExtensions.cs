using OnkelMato.BlogEngine.Core.Model;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public static class PostModelExtensions
{
    public static PostModel ToModel(this Post post)
    {
        return new PostModel()
        {
            UniqueId = post.UniqueId,
            Title = post.Title,
            MdContent = post.MdContent,
            UpdatedAt = post.UpdatedAt,
            PublishedAt = post.PublishedAt,
            Order = post.Order,
            ShowState = post.ShowState.FromShowState(),
            HeaderImage = post.HeaderImage?.UniqueId,
            MdPreview = post.MdPreview
        };
    }

    public static Post FromModel(this PostModel postModel, PostImage? headerImage, IEnumerable<string> tags)
    {
        return new Post()
        {
            UniqueId = postModel.UniqueId,
            Title = postModel.Title,
            MdContent = postModel.MdContent,
            UpdatedAt = postModel.UpdatedAt,
            PublishedAt = postModel.PublishedAt,
            Order = postModel.Order,
            HeaderImage = headerImage,
            PostTags = tags.ToList(),
            ShowState = postModel.ShowState.ToShowState(),
            MdPreview = postModel.MdPreview
        };
    }
}