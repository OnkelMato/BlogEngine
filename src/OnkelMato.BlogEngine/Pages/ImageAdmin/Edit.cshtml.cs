using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class EditModel(BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    : PageModel
{
    [BindProperty]
    public PostImageModel PostImage { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var blog = await context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        // make sure it cannot be accessed if new posts are not allowed
        if (!postsConfiguration.CurrentValue.AllowBlogAdministration)
            RedirectToPage("/Index");

        var postImage = await context.PostImages.SingleOrDefaultAsync(m => m.UniqueId == id && m.Blog == blog);
        if (postImage == null) { return NotFound($"Cannot find image with id {id}"); }

        PostImage = new PostImageModel()
        {
            UniqueId = postImage.UniqueId,
            Name = postImage.Name,
            ContentType = postImage.ContentType,
            AltText = postImage.AltText,
            IsPublished = postImage.IsPublished,
            CreatedAt = postImage.CreatedAt,
            UpdatedAt = postImage.UpdatedAt
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var blog = await context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        // make sure it cannot be accessed if new posts are not allowed
        if (!postsConfiguration.CurrentValue.AllowBlogAdministration)
            RedirectToPage("/Index");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var entity = await context.PostImages.SingleOrDefaultAsync(x => x.UniqueId == PostImage.UniqueId && x.Blog == blog);
        if (entity == null) { return NotFound($"Cannot find image with id {PostImage.UniqueId}"); }

        entity.AltText = PostImage.AltText;
        entity.IsPublished = PostImage.IsPublished;
        entity.Name = PostImage.Name;
        entity.UpdatedAt = DateTime.Now;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!context.PostImages.Any(e => e.Id == entity.Id))
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

}