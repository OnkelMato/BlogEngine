using OnkelMato.BlogEngine.Core.Model;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public static class PostImageModelExtensions
{
    public static PostImage FromModel(this PostImageModel model)
    {
        return new PostImage
        {
            UniqueId = model.UniqueId,
            Name = model.Name,
            AltText = model.AltText,
            IsPublished = model.IsPublished,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            ContentType = model.ContentType ?? "application/octet-stream"
        };
    }

    public static PostImageModel ToModel(this PostImage postImage)
    {
        return new PostImageModel
        {
            UniqueId = postImage.UniqueId,
            Name = postImage.Name,
            AltText = postImage.AltText,
            IsPublished = postImage.IsPublished,
            CreatedAt = postImage.CreatedAt,
            UpdatedAt = postImage.UpdatedAt,
            ContentType = postImage.ContentType
        };
    }
}