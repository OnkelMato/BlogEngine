namespace OnkelMato.BlogEngine.Core.Repository.Model
{
    public class BlogExportModel
    {
        public class PostImageExportModel
        {
            public Guid UniqueId { get; set; }
            public byte[] Image { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string ContentType { get; set; } = null!;
            public string? AltText { get; set; }
            public bool IsPublished { get; set; } = false;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class PostExportModel
        {
            public Guid UniqueId { get; set; }
            public string Title { get; set; } = null!;
            public string MdPreview { get; set; } = null!;
            public string? MdContent { get; set; }
            public int ShowState { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public int Order { get; set; } = 1000;
            public Guid? HeaderImage { get; set; }

            public DateTime PublishedAt { get; set; }

            public List<string> PostTags { get; set; } = [];
        }

        public class PostTagExportModel
        {
            public Guid UniqueId { get; set; }
            public string Title { get; set; } = null!;
        }

        public string? Title { get; set; }
        public string? Description { get; set; }

        public List<PostExportModel> Posts { get; set; } = [];
        public List<PostImageExportModel> PostImages { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool IsFullExport { get; set; }
    }
}
