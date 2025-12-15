namespace OnkelMato.BlogEngine.Core.Model
{
    public class Post
    {
        public Guid UniqueId { get; set; }

        public PostImage? HeaderImage { get; set; }
        public string Title { get; set; } = null!;
        public string MdPreview { get; set; } = null!;
        public string? MdContent { get; set; }
        public ShowState ShowState { get; set; } = ShowState.None;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public int Order { get; set; } = 1000;
        public List<string> PostTags { get; set; } = [];
    }
}
