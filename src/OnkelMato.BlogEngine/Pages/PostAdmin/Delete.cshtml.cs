using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class DeleteModel(
    BlogEngineReadRepository readRepository,
    BlogEngineEditRepository editRepository,
    IOptionsMonitor<BlogConfiguration> blogConfiguration)
    : PageModel
{
    private readonly BlogEngineReadRepository _readRepository = readRepository;
    private readonly BlogEngineEditRepository _editRepository = editRepository ?? throw new ArgumentException(nameof(editRepository));
    private readonly IOptionsMonitor<BlogConfiguration> _blogConfiguration = blogConfiguration ?? throw new ArgumentNullException(nameof(blogConfiguration));

    [BindProperty]
    public PostModel Post { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (!_blogConfiguration.CurrentValue.AllowAdministration)
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

    public async Task<IActionResult> OnPostAsync(Guid? id)
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_blogConfiguration.CurrentValue.AllowAdministration)
            RedirectToPage("/Index");

        if (!id.HasValue) { return NotFound($"Cannot find image with id {id}"); }
     
        await editRepository.Delete(new Post(){UniqueId = id.Value});
        return RedirectToPage("./Index");
    }
}
