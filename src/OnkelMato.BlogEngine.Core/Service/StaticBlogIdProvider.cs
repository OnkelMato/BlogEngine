namespace OnkelMato.BlogEngine.Core.Service;

public class StaticBlogIdProvider(Guid blogId) : IBlogIdProvider
{
    public Guid Id => blogId;
}