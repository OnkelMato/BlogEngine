using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.BlogAdmin;

public class EditModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    : PageModel
{
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

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
        AllowBlogAdministration = _postsConfiguration.CurrentValue.AllowBlogAdministration;
        EngineInfo = GetBlogEngineInfo();

        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound("Blog not found"); }

        Blog = new BlogAdminModel()
        {
            UniqueId = blog.UniqueId,
            Title = blog.Title,
            Description = blog.Description,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_postsConfiguration.CurrentValue.AllowBlogAdministration)
            return Page();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var blogUid = _postsConfiguration.CurrentValue.BlogUniqueId;
        var dbBlog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == blogUid);
        if (dbBlog == null) { return NotFound(); }

        dbBlog.Title = Blog.Title;
        dbBlog.Description = Blog.Description;
        dbBlog.UpdatedAt = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Blogs.Any(e => e.UniqueId == blogUid))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("/Admin");
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
