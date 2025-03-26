using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Softwarekueche.Web.Infrastructure.Data;
using Softwarekueche.Web.Pages.PostAdmin;
using System.ComponentModel.DataAnnotations;

namespace Softwarekueche.Web.Pages;

public class IndexModel(ILogger<IndexModel> logger, SoftwarekuecheHomeContext context)
    : PageModel
{
    public class PostModel
    {
        public Guid UniqueId { get; set; }
        [DataType(DataType.Date)]
        public DateTime UpdatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;

    }

    private readonly ILogger<IndexModel> _logger = logger;
    private readonly SoftwarekuecheHomeContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public IList<PostModel> Posts { get; set; } = [];


    public void OnGet()
    {
        Posts = _context.Posts.OrderByDescending(x => x.UpdatedAt).Select(x => new PostModel()
        {
            Title = x.Title,
            UniqueId = x.UniqueId,
            UpdatedAt = x.UpdatedAt,
            HtmlContent = Markdown.ToHtml(x.MdContent, null, null)
        }).ToList();
    }
}