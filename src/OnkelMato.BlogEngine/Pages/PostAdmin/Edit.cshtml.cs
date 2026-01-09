using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class EditModel(
    BlogEngineReadRepository readRepository,
    BlogEngineEditRepository editRepository,
    IOptionsMonitor<BlogConfiguration> postsConfiguration)
    : PageModel
{
    private readonly BlogEngineReadRepository _readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
    private readonly BlogEngineEditRepository _editRepository = editRepository ?? throw new ArgumentNullException(nameof(editRepository));
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    [BindProperty]
    public PostModel Post { get; set; } = null!;

    [BindProperty(Name = "redirect_uri", SupportsGet = true)]
    public string? RedirectUri { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            return RedirectToPage(RedirectUri ?? "/");

        if (id == null)
        {
            return NotFound();
        }

        var post = await _readRepository.PostAsync(id.Value);
        if (post == null)
        {
            return NotFound();
        }

        Post = post.ToModel();
        Post.UpdatedAt = DateTime.Now;
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more information, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var postHeaderImage = Post.HasHeaderImage ? (await _readRepository.PostImageAsync(Post.HeaderImage!.Value)) : null;

        if (Post.HeaderImage != null && postHeaderImage is null)
        {
            ModelState.AddModelError("Post.HeaderImage", $"Cannot find header image {Post.HeaderImage} in images");
            TempData["ErrorMessage"] = $"Cannot find header image {Post.HeaderImage} in images";
            return Page();
        }

        var tags = Post.Tags?.Split(',').Select(x => x.Trim()).ToArray() ?? [];
        await _editRepository.UpdatePost(Post.FromModel(postHeaderImage, tags));

        return RedirectToPage("./Index");
    }
}