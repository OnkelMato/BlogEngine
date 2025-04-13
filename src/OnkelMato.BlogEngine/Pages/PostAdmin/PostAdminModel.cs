using System.ComponentModel.DataAnnotations;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class PostAdminModel
{
    public Guid UniqueId { get; set; }

    [MaxLength(256)]
    public string Title { get; set; } = null!;

    [MaxLength(4096)]
    [DataType(DataType.MultilineText)]
    public string? MdContent { get; set; } = null!;
    [MaxLength(4096)]
    public string MdPreview { get; set; } = null!;

    public ShowStateModel ShowState { get; set; } = ShowStateModel.None;

    [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
    public DateTime UpdatedAt { get; set; }

    public int Order { get; set; } = 1000;

    public Guid?  HeaderImage { get; set; }
    public bool HasHeaderImage => HeaderImage is not null;
}

public enum ShowStateModel
{
    [Display(Name = "Unpublished")] None = 0,
    [Display(Name = "Show in Blog")] Blog = 1,
    [Display(Name = "Show in Menu")] Menu = 2,
    [Display(Name = "Show in Blog an Menu")] BlogAndMenu = 3

}