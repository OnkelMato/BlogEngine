using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class EditModel : PageModel
{
    private readonly PostsConfiguration _postsConfiguration;
    private readonly BlogEngineContext _context;

    public EditModel(BlogEngineContext context, IOptionsSnapshot<PostsConfiguration> postsConfiguration)
    {
        _postsConfiguration = postsConfiguration.Value;
        _context = context;
    }

    [BindProperty]
    public PostImageModel PostImage { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.AllowNewPosts)
            RedirectToPage("/Index");

        var postimage =  await _context.PostImages.SingleAsync(m => m.UniqueId == id);
        if (postimage == null)
        {
            return NotFound();
        }

        PostImage = new PostImageModel() {
            UniqueId = postimage.UniqueId,
            Name = postimage.Name,
            FileName = postimage.Filename,
            ContentType = postimage.ContentType,
            AltText = postimage.AltText,
            IsPublished = postimage.IsPublished,
            CreatedAt = postimage.CreatedAt,
            UpdatedAt = postimage.UpdatedAt
        };
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more information, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.AllowNewPosts)
            RedirectToPage("/Index");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var entity = await _context.PostImages.SingleAsync(x=> x.UniqueId == PostImage.UniqueId);
        entity.AltText = PostImage.AltText;
        entity.IsPublished = PostImage.IsPublished;
        entity.Name = PostImage.Name;
        entity.UpdatedAt = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PostImageExists(entity.Id))
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

    private bool PostImageExists(int id)
    {
        return _context.PostImages.Any(e => e.Id == id);
    }
}