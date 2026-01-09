using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;
using OnkelMato.BlogEngine.Core.Service;

namespace OnkelMato.BlogEngine.Pages.BlogAdmin
{
    public class DeleteModel(
        IBlogIdProvider blogId,
        IOptionsMonitor<BlogConfiguration> blogConfiguration,
        BlogEngineMgmtRepository repository) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        [Display(Name = "Blog Id")]
        public Guid BlogId { get; set; } = Guid.Empty;

        public async Task<RedirectResult> OnGet()
        {
            if (blogConfiguration.CurrentValue.AllowBlogDeletion == false)
            {
                TempData["StatusMessage"] = "Blog Deletion is disabled in the configuration.";
                return Redirect("/BlogAdmin/Edit?blogId=" + blogId.Id);
            }

            var redirectId = (BlogId == Guid.Empty || BlogId == blogId.Id) ? blogConfiguration.CurrentValue.BlogUniqueId : blogId.Id;

            await repository.ClearPostsAndImages(BlogId);
            await repository.DeleteBlog(BlogId);

            return Redirect("/BlogAdmin/Edit?blogId=" + redirectId);
        }
    }
}
