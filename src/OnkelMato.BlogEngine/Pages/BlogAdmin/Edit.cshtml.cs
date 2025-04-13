using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.BlogAdmin;

public class EditModel : PageModel
{
    private readonly BlogEngineContext _context;
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration;

    public EditModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));
    }

    [BindProperty]
    public BlogAdminModel Blog { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_postsConfiguration.CurrentValue.AllowBlogAdministration)
            return RedirectToPage("/Index");

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

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more information, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
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
}