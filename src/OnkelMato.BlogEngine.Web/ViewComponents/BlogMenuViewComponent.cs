using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Database;
using System.Text.RegularExpressions;

namespace OnkelMato.BlogEngine.Web.ViewComponents;

public class BlogMenuViewComponent(BlogEngineRepository repository, IOptionsMonitor<PostsConfiguration> postsConfiguration)
    : ViewComponent
{
    private readonly BlogEngineRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly IOptionsMonitor<PostsConfiguration> _postsConfiguration = postsConfiguration ?? throw new ArgumentNullException(nameof(postsConfiguration));

    public List<DynamicMenuModel> MenuPosts { get; set; } = [];

    public IViewComponentResult Invoke(int maxPriority, bool isDone)
    {
        MenuPosts = _repository.PostsInMenu()
            .Select(x => new DynamicMenuModel()
            {
                PostId = x.UniqueId,
                Url = x.ShowState == ShowState.LinkAndMenu ? x.MdPreview : null, // because MdPreview is required
                TitleSEO = Regex.Replace(x.Title, "[^a-zA-Z0-9 ]", "").Replace(" ", "_"),
                Title = x.Title
            }).ToList();

        return View(this);
    }

    public class DynamicMenuModel
    {
        public string Title { get; set; } = null!;
        public string? Url { get; set; } = null!;
        public Guid? PostId { get; set; }
        public string? TitleSEO { get; set; }
    }
}