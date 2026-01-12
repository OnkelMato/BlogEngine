using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.BlogAdmin
{
    public class ListModel(BlogEngineMgmtRepository mgmtRepository, IOptionsMonitor<ImportExportConfiguration> imexConfiguration) : PageModel
    {
        public IActionResult OnGet()
        {
            if (!imexConfiguration.CurrentValue.EnableBlogImportSync)
                return Unauthorized();

            var blogs = mgmtRepository.GetAllBlogs().ToDictionary(x => x.UniqueId, y => y.Title);
            return new JsonResult(blogs);
        }
    }
}
