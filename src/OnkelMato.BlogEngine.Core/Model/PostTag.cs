namespace OnkelMato.BlogEngine.Core.Model;

public class PostTag
{
    public string Title { get; set; } = null!;
    public Blog Blog { get; set; } = null!;
    public Post Post { get; set; } = null!;
}