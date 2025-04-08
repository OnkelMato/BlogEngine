using System.ComponentModel.DataAnnotations;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class PostAdminModel
{
    public Guid UniqueId { get; set; }

    [MaxLength(256)]
    public string Title { get; set; } = null!;

    [MaxLength(4096)]
    [DataType(DataType.MultilineText)]
    [UIHint("MarkdownEditor")]
    public string? MdContent { get; set; } = null!;
    [MaxLength(4096)]
    public string MdPreview { get; set; } = null!;

    public bool IsPublished { get; set; }
    public DateTime UpdatedAt { get; set; }
}