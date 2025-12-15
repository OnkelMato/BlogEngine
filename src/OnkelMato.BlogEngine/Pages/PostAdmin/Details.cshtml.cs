using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class DetailsModel : PageModel
{
    private readonly BlogEngineReadRepository _readRepository;
    private readonly IOptionsSnapshot<BlogConfiguration> _blogConfiguration;

    public DetailsModel(BlogEngineReadRepository readRepository, IOptionsSnapshot<BlogConfiguration> blogConfiguration)
    {
        _readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
        _blogConfiguration = blogConfiguration ?? throw new ArgumentNullException(nameof(blogConfiguration));
    }

    public PostModel Post { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (!_blogConfiguration.Value.AllowAdministration)
            return RedirectToPage("/Index");

        if (id == null)
        {
            return NotFound();
        }

        var dbPost = await _readRepository.PostAsync(id.Value);
        if (dbPost == null) return NotFound();
        Post = dbPost.ToModel();

        return Page();
    }
}