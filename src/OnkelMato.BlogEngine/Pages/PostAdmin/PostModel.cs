using System.ComponentModel.DataAnnotations;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class PostModel
{
    public Guid UniqueId { get; set; }

    [MaxLength(256)]
    public string Title { get; set; } = null!;

    //[MaxLength(int.MaxValue)]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Contents (Markdown)")]
    public string? MdContent { get; set; } = null!;

    [Display(Name = "Preview (Markdown)")]
    [DataType(DataType.MultilineText)]
    [MaxLength(4096)]
    public string MdPreview { get; set; } = null!;

    public ShowStateModel ShowState { get; set; } = ShowStateModel.None;

    [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
    [Display(Name = "Last Update")]
    public DateTime UpdatedAt { get; set; }

    [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
    [Display(Name = "Published At")]
    public DateTime PublishedAt { get; set; } = DateTime.Now;

    public int Order { get; set; } = 1000;

    [Display(Name = "Posts Image Guid")]
    public Guid? HeaderImage { get; set; }

    [MaxLength(256)]
    [Display(Name = "Tags (comma separated)")]
    public string Tags { get; set; } = null!;

    [Display(Name = "Posts Image")]
    public bool HasHeaderImage => HeaderImage is not null;
}