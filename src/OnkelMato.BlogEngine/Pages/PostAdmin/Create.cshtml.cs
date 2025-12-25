using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class CreateModel : PageModel
{
    private readonly BlogEngineReadRepository _readRepository;
    private readonly BlogEngineEditRepository _editRepository;
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration;

    public CreateModel(
        BlogEngineReadRepository readRepository,
        BlogEngineEditRepository editRepository,
        IOptionsMonitor<BlogConfiguration> blogConfiguration)
    {
        _readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
        _editRepository = editRepository ?? throw new ArgumentNullException(nameof(editRepository));
        _postsConfiguration = blogConfiguration ?? throw new ArgumentNullException(nameof(blogConfiguration));

        Post = new PostModel();
    }

    public IActionResult OnGet()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            return RedirectToPage("/Index");

        return Page();
    }

    [BindProperty]
    public PostModel Post { get; set; } = null!;

    // For more information, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            return RedirectToPage("/Index");

        if (!ModelState.IsValid)
            return Page();

        var blog = _editRepository.Blog;

        var postHeaderImage = Post.HasHeaderImage ? await _readRepository.PostImageAsync(Post.HeaderImage!.Value) : null;
        var post = Post.FromModel(postHeaderImage, Post.Tags.Split(','));

        var result = await _editRepository.AddPost(post);

        if (result.IsFailure)
        {
            ModelState.AddModelError(nameof(Post.HeaderImage), result.ErrorMessage!);
            return Page();
        }

        return RedirectToPage("./Index");
    }
}