using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class EditModel(
    BlogEngineEditRepository editRepository,
    BlogEngineReadRepository readRepository,
    IOptionsMonitor<BlogConfiguration> blogConfiguration)
    : PageModel
{
    [BindProperty]
    public PostImageModel PostImage { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!blogConfiguration.CurrentValue.AllowAdministration)
            RedirectToPage("/Index");

        var postImage = await readRepository.PostImageAsync(id);
        if (postImage == null) { return NotFound($"Cannot find image with id {id}"); }

        PostImage = new PostImageModel()
        {
            UniqueId = postImage.UniqueId,
            Name = postImage.Name,
            ContentType = postImage.ContentType,
            AltText = postImage.AltText,
            IsPublished = postImage.IsPublished,
            CreatedAt = postImage.CreatedAt,
            UpdatedAt = postImage.UpdatedAt
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!blogConfiguration.CurrentValue.AllowAdministration)
            RedirectToPage("/Index");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await editRepository.UpdateImage(PostImage.FromModel());
        if (result.IsFailure) return NotFound();

        return RedirectToPage("./Index");
    }

}