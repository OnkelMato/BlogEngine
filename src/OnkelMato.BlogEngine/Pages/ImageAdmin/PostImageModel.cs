using System.ComponentModel.DataAnnotations;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class PostImageModel {
    public Guid UniqueId { get; set; }
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;
    public IFormFile? File { get; set; } = null!;
    [Display(Name = "Image Alt Text")]
    public string? AltText { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [Display(Name = "Last Update")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public string? ContentType { get; internal set; } = null!;
}