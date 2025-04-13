using Markdig;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine;
using OnkelMato.BlogEngine.Database;

namespace Softwarekueche.Web.Pages;

public class IndexModel(ILogger<IndexModel> logger, BlogEngineContext context, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    : PageModel
{
    public class PostModel
    {
        public Guid UniqueId { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime UpdatedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public string HtmlPreview { get; set; } = string.Empty;
        public bool HasContent { get; set; }
        public Guid? HeaderImage { get; set; } = null!;
    }

    private readonly ILogger<IndexModel> _logger = logger;
    private readonly BlogEngineContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public IList<PostModel> Posts { get; set; } = [];

    [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
    public int NumOfPages { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var blog = await _context.Blogs.FirstOrDefaultAsync(m => m.UniqueId == _postsConfiguration.CurrentValue.BlogUniqueId);
        if (blog == null) { return NotFound($"Blog {_postsConfiguration.CurrentValue.BlogUniqueId} not Found"); }

        NumOfPages = (_context.Posts.Count() / _postsConfiguration.CurrentValue.PageSize) + 1;

        Posts = _context.Posts
            .Include(x=> x.HeaderImage)
            .Where(x => x.Blog == blog && x.ShowState == ShowState.Blog || x.ShowState == ShowState.BlogAndMenu)
            .OrderBy(x => x.Order)
            .Skip((CurrentPage - 1) * _postsConfiguration.CurrentValue.PageSize)
            .Take(_postsConfiguration.CurrentValue.PageSize)
            .Select(x => new PostModel()
            {
                Title = x.Title,
                UniqueId = x.UniqueId,
                UpdatedAt = x.UpdatedAt,
                HasContent = x.MdContent != null,
                HeaderImage = (x.HeaderImage != null) ? x.HeaderImage.UniqueId : null,
                HtmlPreview = Markdown.ToHtml(x.MdPreview, null, null)
            }).ToList();

        return Page();
    }
}