using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.BlogAdmin;

public class EditModel(BlogEngineRepository repository, IOptionsMonitor<BlogConfiguration> postsConfiguration)
    : PageModel
{
    private readonly BlogEngineRepository _context = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public class BlogEngineInfo
    {
        public string Version { get; set; } = "N/A";
        public string VersionSourceSha { get; set; } = "N/A";
        public string UncommittedChanges { get; set; } = "N/A";
        public string BranchName { get; set; } = "N/A";
    }

    [BindProperty]
    public BlogAdminModel Blog { get; set; } = null!;

    public BlogEngineInfo EngineInfo { get; set; } = new();

    public bool AllowBlogAdministration { get; set; } = false;


    public async Task<IActionResult> OnGetAsync()
    {
        AllowBlogAdministration = _postsConfiguration.CurrentValue.AllowAdministration;
        EngineInfo = GetBlogEngineInfo();

        var blog = _context.Blog();
        if (blog == null) { return NotFound("Blog not found"); }

        Blog = new BlogAdminModel()
        {
            UniqueId = blog.UniqueId,
            Title = blog.Title,
            Description = blog.Description,
            // todo CSS = blog.CSS,
            Blogs = _context.GetBlogs().Select(x => new BlogAdminModel.BlogItemModel() {BlogId = x.UniqueId, Title = x.Title}).ToArray()
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            return Page();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (await _context.UpdateBlog(Blog.Title, Blog.Description, Blog.CSS))
            return RedirectToPage("/Admin");
        else
            return NotFound();
    }

    private static BlogEngineInfo GetBlogEngineInfo()
    {
        return new BlogEngineInfo()
        {
            Version = typeof(BlogEngineContext).Assembly.GetName().Version?.ToString() ?? "N/A",
            //Version = (typeof(BlogEngineContext).Assembly.GetName().Version?.ToString() ?? "N/A") + $" ({GitVersionInformation.AssemblySemVer})" ,
            //VersionSourceSha = GitVersionInformation.VersionSourceSha,
            //UncommittedChanges = GitVersionInformation.UncommittedChanges,
            //BranchName = GitVersionInformation.BranchName,
        };
    }
}
