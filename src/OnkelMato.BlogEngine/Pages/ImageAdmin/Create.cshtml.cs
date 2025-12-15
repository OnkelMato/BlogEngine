using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class CreateModel(
    BlogEngineContext context,
    IOptionsMonitor<BlogConfiguration> postsConfiguration)
    : PageModel
{
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public IActionResult OnGet()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            return RedirectToPage("/Index");

        return Page();
    }

    [BindProperty]
    public PostImageModel PostImage { get; set; } = new PostImageModel();

    // For more information, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        // make sure it cannot be accessed if new posts are not allowed
        if (!_postsConfiguration.CurrentValue.AllowAdministration)
            return RedirectToPage("/Index");

        if (!ModelState.IsValid || PostImage.File is null)
        {
            Console.WriteLine("Model is Invalid");
            return Page();
        }

        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        using var stream = new MemoryStream();
        if (PostImage.File is not null)
            await PostImage.File.CopyToAsync(stream);
        var imageRaw = stream.ToArray();

        var entity = new PostImage()
        {
            Blog = blog,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            UniqueId = Guid.NewGuid(),
            Name = PostImage.Name,
            ContentType = PostImage.File?.ContentType!,
            AltText = PostImage.AltText,
            IsPublished = PostImage.IsPublished,
            Image = imageRaw
        };

        _context.PostImages.Add(entity);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}