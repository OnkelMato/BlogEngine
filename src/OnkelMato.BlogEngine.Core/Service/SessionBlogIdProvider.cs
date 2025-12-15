namespace OnkelMato.BlogEngine.Core.Service;

public class SessionBlogIdProvider(IHttpContextAccessor contextAccessor) : IBlogIdProvider
{
    public Guid Id => Guid.Parse(contextAccessor?.HttpContext?.Session.GetString("blogid") ?? Guid.Empty.ToString());
}