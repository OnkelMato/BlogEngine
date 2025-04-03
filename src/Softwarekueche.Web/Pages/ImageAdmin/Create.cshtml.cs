using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Softwarekueche.Web.Infrastructure.Data;

namespace Softwarekueche.Web.Pages.ImageAdmin
{
    public class CreateModel(
        SoftwarekuecheHomeContext context, 
        IOptionsSnapshot<PostsConfiguration> postsConfiguration)
        : PageModel
    {
        private readonly SoftwarekuecheHomeContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IOptionsSnapshot<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

        public IActionResult OnGet()
        {
            // make sure it cannot be accessed if new posts are not allowed
            if (!_postsConfiguration.Value.AllowNewPosts)
                return RedirectToPage("/Index");

            return Page();
        }

        [BindProperty]
        public PostImageModel PostImage { get; set; } = new PostImageModel();

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            // make sure it cannot be accessed if new posts are not allowed
            if (!_postsConfiguration.Value.AllowNewPosts)
                return RedirectToPage("/Index");

            Console.WriteLine("Post Async");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model is Invalid");
                return Page();
            }

            using var stream = new MemoryStream();
            await PostImage.File.CopyToAsync(stream);
            var imageRaw = stream.ToArray();

            var entity = new PostImage()
            {
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UniqueId = Guid.NewGuid(),
                Name = PostImage.Name,
                Filename = PostImage.File.FileName,
                ContentType = PostImage.File.ContentType,
                AltText = PostImage.AltText,
                IsPublished = PostImage.IsPublished,
                Image = imageRaw
            };

            _context.PostImages.Add(entity);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
