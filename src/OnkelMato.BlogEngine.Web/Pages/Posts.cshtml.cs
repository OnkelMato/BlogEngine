using System;
using System.ComponentModel.DataAnnotations;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Web.Pages;

public class PostsModel(BlogEngineReadRepository readRepository, IOptionsMonitor<BlogConfiguration> postsConfiguration) : PageModel
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

        public DateTime PublishedAt { get; set; }
    }

    private readonly BlogEngineReadRepository _readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
    private readonly IOptionsMonitor<BlogConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public bool AllowBlogAdministration => _postsConfiguration.CurrentValue.AllowAdministration;
    public PostModel Post { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    // todo change to async
    public IActionResult OnGet()
    {
        // get post data
        var dbPost = _readRepository.PostAsync(Id).Result;
        if (dbPost == null)
        {
            TempData["StatusMessage"] = "Post does not exist. Sending you back to index page.";
            return RedirectToPage(nameof(Index));
        }

        // check if post is published. Only show unpublished posts when administration is allowed
        if (dbPost.ShowState == Core.Model.ShowState.None && !AllowBlogAdministration)
            return NotFound($"Post {Id} is in draft mode.");

        Post = new PostModel()
        {
            Title = dbPost.Title,
            UniqueId = dbPost.UniqueId,
            UpdatedAt = dbPost.UpdatedAt,
            PublishedAt = dbPost.PublishedAt,
            HeaderImage = dbPost.HeaderImage?.UniqueId,
            HtmlPreview = Markdown.ToHtml(dbPost.MdPreview),
            HtmlContent = Markdown.ToHtml(dbPost.MdContent ?? string.Empty)
        };

        return Page();
    }
}
