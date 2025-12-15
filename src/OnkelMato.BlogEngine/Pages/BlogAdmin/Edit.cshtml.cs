using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.BlogAdmin;

public class EditModel(BlogEngineEditRepository editRepository, BlogEngineMgmtRepository managementRepository, IOptionsMonitor<BlogConfiguration> blogConfiguration)
    : PageModel
{
    private readonly BlogEngineEditRepository _editRepository = editRepository ?? throw new ArgumentNullException(nameof(editRepository));
    private readonly BlogEngineMgmtRepository _managementRepository = managementRepository ?? throw new ArgumentNullException(nameof(managementRepository));
    private readonly IOptionsMonitor<BlogConfiguration> _blogConfiguration = blogConfiguration ?? throw new ArgumentNullException(nameof(blogConfiguration));

    [BindProperty]
    public BlogAdminModel Blog { get; set; } = null!;

    public bool AllowBlogAdministration => _blogConfiguration.CurrentValue.AllowAdministration;
    public bool AllowBlogDeletion => _blogConfiguration.CurrentValue.AllowBlogDeletion;
    public bool AllowBlogCreation => _blogConfiguration.CurrentValue.AllowBlogCreation;
    public bool AllowBlogSelection => _blogConfiguration.CurrentValue.EnableBlogSelection;


    public async Task<IActionResult> OnGetAsync()
    {
        var blog = _editRepository.Blog;

        Blog = new BlogAdminModel()
        {
            UniqueId = blog.UniqueId,
            Title = blog.Title,
            Description = blog.Description,
            CSS = blog.CSS,
            Blogs = _managementRepository.GetBlogs().Select(x => new BlogAdminModel.BlogItemModel() { BlogId = x.UniqueId, Title = x.Title }).ToArray()
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_blogConfiguration.CurrentValue.AllowAdministration)
            return Page();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (await _editRepository.UpdateBlog(Blog.Title, Blog.Description, Blog.CSS))
            return RedirectToPage("/Admin");
        else
            return NotFound();
    }
}
