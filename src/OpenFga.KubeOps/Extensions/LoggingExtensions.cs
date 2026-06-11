using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Formatting.Compact;

namespace OpenFga.KubeOps.Extensions;

public static class LoggingExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSerilog(IConfiguration configuration)
        {
            services.AddSerilog((provider, logger) =>
            {
                logger
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(provider)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(new RenderedCompactJsonFormatter());
            });
            return services;
        }
    }
}
