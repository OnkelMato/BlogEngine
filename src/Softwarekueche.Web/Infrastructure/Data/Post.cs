using System.ComponentModel.DataAnnotations;

namespace Softwarekueche.Web.Infrastructure.Data;

public class Post
{
    [Key]
    public int Id { get; set; }

    public Guid UniqueId { get; set; }

    [MaxLength(256)]
    public string Title { get; set; } = null!;

    [MaxLength(4096)]
    public string MdContent { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}