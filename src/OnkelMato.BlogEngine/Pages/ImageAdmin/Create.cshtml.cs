using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Model;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Pages.ImageAdmin;

public class CreateModel(
    BlogEngineEditRepository editRepository,
    IOptionsMonitor<BlogConfiguration> blogConfiguration)
    : PageModel
{
    private readonly BlogEngineEditRepository _context = editRepository ?? throw new ArgumentNullException(nameof(editRepository));
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = blogConfiguration ?? throw new ArgumentNullException(nameof(blogConfiguration));

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

        var blog = editRepository.Blog;

        using var stream = new MemoryStream();
        if (PostImage.File is not null)
            await PostImage.File.CopyToAsync(stream);
        var imageRaw = stream.ToArray();

        var entity = new PostImage()
        {
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            UniqueId = Guid.NewGuid(),
            Name = PostImage.Name,
            ContentType = PostImage.File?.ContentType!,
            AltText = PostImage.AltText,
            IsPublished = PostImage.IsPublished,
            Image = imageRaw
        };

        var result = editRepository.AddImage(entity);



        return RedirectToPage("./Index");
    }
}