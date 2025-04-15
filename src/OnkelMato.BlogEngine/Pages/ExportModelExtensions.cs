using System.Text.Json;
using System.Text.Json.Serialization;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages;

public static class ExportModelExtensions
{
    public static BlogExportModel.PostImageExportModel ToPostImageExportModel(this PostImage postImage)
    {
        return new BlogExportModel.PostImageExportModel()
        {
            UniqueId = postImage.UniqueId,
            Image = postImage.Image,
            Name = postImage.Name,
            ContentType = postImage.ContentType,
            AltText = postImage.AltText,
            IsPublished = postImage.IsPublished,
            CreatedAt = postImage.CreatedAt,
            UpdatedAt = postImage.UpdatedAt
        };
    }

    public static BlogExportModel.PostExportModel ToPostExportModel(this Post post)
    {
        return new BlogExportModel.PostExportModel()
        {
            UniqueId = post.UniqueId,
            Title = post.Title,
            MdPreview = post.MdPreview,
            MdContent = post.MdContent,
            HeaderImage = post.HeaderImage?.UniqueId,
            ShowState = (int)post.ShowState,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            Order = post.Order
        };
    }

    public static string AsJson(this BlogExportModel export)
    {
        return JsonSerializer.Serialize(export, new JsonSerializerOptions() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });
    }
}