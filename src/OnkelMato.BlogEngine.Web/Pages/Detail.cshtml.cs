using System.ComponentModel.DataAnnotations;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace OnkelMato.BlogEngine.Web.Pages;

public class DetailModel(BlogEngineRepository repository, IOptionsMonitor<PostsConfiguration> postsConfiguration) : PageModel
{
    public class PostModel
    {
        public Guid UniqueId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string HtmlPreview { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
        public DateTime UpdatedAt { get; set; }

        public Guid? HeaderImage { get; set; }
    }

    private readonly BlogEngineRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public bool AllowBlogAdministration => _postsConfiguration.CurrentValue.AllowBlogAdministration;
    public PostModel Post { get; set; } = new PostModel();

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    public IActionResult OnGet()
    {
        var x = _repository.Post(Id);
        if (x == null)
            return NotFound($"Post {Id} not found");

        Post = new PostModel()
        {
            Title = x.Title,
            UniqueId = x.UniqueId,
            UpdatedAt = x.UpdatedAt,
            HeaderImage = x.HeaderImage == null ? null : x.HeaderImage.UniqueId,
            HtmlPreview = Markdown.ToHtml(x.MdPreview, null, null),
            HtmlContent = Markdown.ToHtml(x.MdContent ?? string.Empty, null, null)
        };

        return Page();
    }
}
