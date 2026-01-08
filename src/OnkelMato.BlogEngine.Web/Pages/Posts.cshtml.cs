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
    public PostModel Post { get; set; } = new PostModel();

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    // todo change to async
    public IActionResult OnGet()
    {
        // get blog data for SEO
        var blog = _readRepository.Blog()!;
                  
        this.BlogTitle = blog.Title;
        this.BlogDescription = blog.Description;

        // get post data
        var x = _readRepository.PostAsync(Id).Result;
        if (x == null)
        {
            TempData["StatusMessage"] = "Post does not exist.";
            return RedirectToPage("Index");
        }

        // check if post is published. Only show unpublished posts when administration is allowed
        if (x.ShowState == Core.Model.ShowState.None && !AllowBlogAdministration)
            return NotFound($"Post {Id} is in draft mode.");

        Post = new PostModel()
        {
            Title = x.Title,
            UniqueId = x.UniqueId,
            UpdatedAt = x.UpdatedAt,
            PublishedAt = x.PublishedAt,
            HeaderImage = x.HeaderImage?.UniqueId,
            HtmlPreview = Markdown.ToHtml(x.MdPreview, null, null),
            HtmlContent = Markdown.ToHtml(x.MdContent ?? string.Empty, null, null)
        };

        return Page();
    }

    public string? BlogDescription { get; set; }

    public string? BlogTitle { get; set; }
}
