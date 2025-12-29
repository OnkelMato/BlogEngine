namespace OnkelMato.BlogEngine.Core.Service;

public class StaticFallbackBlogIdProviderDecorator(IBlogIdProvider inner, Guid staticBlogId) 
    : IBlogIdProvider
{
    public Guid Id => inner.Id != Guid.Empty ? inner.Id: staticBlogId;
}