using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnkelMato.BlogEngine.Database;

namespace OnkelMato.BlogEngine.Pages;

public class ExportModel(BlogEngineContext context) : PageModel
{
    private readonly BlogEngineContext _context = context;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    [BindProperty(SupportsGet = true)]
    public string? Entity { get; set; }

    public ActionResult OnGet()
    {
        if (Entity is null)
            throw new ArgumentException(nameof(Entity));

        switch (Entity.ToLower())
        {
            case "posts":
            {
                Func<Post, bool> filter = Id == Guid.Empty
                    ? ((x) => true)
                    : ((x) => x.UniqueId == Id);

                var pst = _context.Posts.Where(filter).ToArray();
                var filename = !pst.Any()
                    ? $"Posts.{DateTime.Now.ToShortDateString()}.json"
                    : $"{pst.First().Title}.json";
                var json = JsonSerializer.Serialize(pst);
                return File(Encoding.UTF8.GetBytes(json), "application/json", filename);
            }
            case "postimages":
            {
                Func<PostImage, bool> filter = Id == Guid.Empty
                    ? ((x) => true)
                    : ((x) => x.UniqueId == Id);

                var img = _context.PostImages.Where(filter);
                var filename = !img.Any()
                    ? $"PostImages.{DateTime.Now.ToShortDateString()}.json"
                    : $"{img.First().Filename}.json";
                var json = JsonSerializer.Serialize(img);
                return File(Encoding.UTF8.GetBytes(json), "application/json", filename);
            }
            default:
                return BadRequest();
        }
    }
}