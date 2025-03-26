using System.ComponentModel.DataAnnotations;

namespace Softwarekueche.Web.Pages.PostAdmin;

public class PostAdminModel
{
    public Guid UniqueId { get; set; }

    [MaxLength(256)]
    public string Title { get; set; } = null!;

    [MaxLength(4096)]
    public string MdContent { get; set; } = null!;

    public DateTime UpdatedAt { get; set; }
}