using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.PostAdmin;

public class EditModel : PageModel
{
    private readonly BlogEngineContext _context;
    private readonly IOptionsSnapshot<PostsConfiguration> _postsConfiguration;

    public EditModel(BlogEngineContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));
    }

    [BindProperty]
    public PostAdminModel Post { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (!_postsConfiguration.Value.AllowNewPosts)
            return RedirectToPage("/Index");

        if (id == null)
        {
            return NotFound();
        }

        var post =  await _context.Posts.FirstOrDefaultAsync(m => m.UniqueId == id);
        if (post == null)
        {
            return NotFound();
        }
        Post = new PostAdminModel() { 
            UniqueId = post.UniqueId, Title = post.Title, MdContent = post.MdContent,
            UpdatedAt = post.UpdatedAt,
            IsPublished = post.IsPublished, MdPreview = post.MdPreview };
        Post.UpdatedAt = DateTime.Now;
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

        var dbPost = await _context.Posts.SingleAsync(m => m.UniqueId == Post.UniqueId);
        dbPost.MdContent = Post.MdContent;
        dbPost.Title = Post.Title;
        dbPost.UpdatedAt = DateTime.Now;
        dbPost.IsPublished = Post.IsPublished;
        dbPost.MdPreview = Post.MdPreview;
            
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostExists(Post.UniqueId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private bool PostExists(Guid id)
    {
        return _context.Posts.Any(e => e.UniqueId == id);
    }
}