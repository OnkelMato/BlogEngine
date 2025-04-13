using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class DetailsModel : PageModel
{
    private readonly PostsConfiguration _postsConfiguration;
    private readonly BlogEngineContext _context;

    public DetailsModel(BlogEngineContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
    {
        _postsConfiguration = postsConfiguration.Value;
        _context = context;
    }

    public PostImageModel PostImage { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.AllowBlogAdministration)
            RedirectToPage("/Index");

        var postimage =  await _context.PostImages.SingleAsync(m => m.UniqueId == id);
        if (postimage == null)
        {
            return NotFound();
        }

        PostImage = new PostImageModel() {
            UniqueId = postimage.UniqueId,
            Name = postimage.Name,
            ContentType = postimage.ContentType,
            AltText = postimage.AltText,
            IsPublished = postimage.IsPublished,
            CreatedAt = postimage.CreatedAt,
            UpdatedAt = postimage.UpdatedAt
        };

        return Page();
    }
}