using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class DetailsModel(BlogEngineReadRepository readRepository, IOptionsMonitor<BlogConfiguration> blogConfiguration)
    : PageModel
{
    public PostImageModel PostImage { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (!blogConfiguration.CurrentValue.AllowAdministration)
            return RedirectToPage("/Index");

        if (id == null)
        {
            return NotFound();
        }

        var db = await readRepository.PostImageAsync(id.Value);
        if (db == null) return NotFound();
        PostImage = db.ToModel();

        return Page();
    }
}