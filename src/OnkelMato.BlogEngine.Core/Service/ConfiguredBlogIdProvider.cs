using Microsoft.Extensions.Options;
using OnkelMato.BlogEngine.Core.Configuration;

namespace OnkelMato.BlogEngine.Core.Service;

public class ConfiguredBlogIdProvider(IOptionsMonitor<BlogConfiguration> settings) : IBlogIdProvider
{
    public Guid Id => settings.CurrentValue.BlogUniqueId!.Value;
}