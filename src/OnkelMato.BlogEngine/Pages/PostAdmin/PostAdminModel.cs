using System.ComponentModel.DataAnnotations;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class PostAdminModel
{
    public Guid UniqueId { get; set; }

    [MaxLength(256)]
    public string Title { get; set; } = null!;

    [MaxLength(int.MaxValue)]
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

    [Display(Name = "Posts Image Guid")] public Guid? HeaderImage { get; set; }

    [Display(Name = "Posts Image")]
    public bool HasHeaderImage => HeaderImage is not null;
}

public enum ShowStateModel
{
    [Display(Name = "Unpublished")] None = 0,
    [Display(Name = "Show in Blog")] Blog = 1,
    [Display(Name = "Show in Menu")] Menu = 2,
    [Display(Name = "Show in Footer")] Footer = 8,

    [Display(Name = "Show in Blog an Menu")] BlogAndMenu = 3,
    [Display(Name = "Show in Blog and Footer")] BlogAndFooter = 9,
    [Display(Name = "Show in Link and Menu")] LinkAndMenu = 6,
    [Display(Name = "Show in Link and Footer")] LinkAndFooter = 12,
}