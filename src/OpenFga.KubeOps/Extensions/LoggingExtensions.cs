using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Formatting.Compact;

namespace OpenFga.KubeOps.Extensions;

public static class LoggingExtensions
{
    private const string DevelopmentOutputTemplate =
        "[{Timestamp:HH:mm:ss.fff} {Level:u3}] <{SourceContext}> {Message:lj}{NewLine}{Exception}";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddSerilog(IConfiguration configuration, bool isDevelopment = false)
        {
            services.AddSerilog((provider, logger) =>
            {
                logger
                    .ReadFrom.Configuration(configuration)
                    .ReadFrom.Services(provider)
                    .Enrich.FromLogContext();

                if (isDevelopment)
                {
                    logger.WriteTo.Console(outputTemplate: DevelopmentOutputTemplate);
                }
                else
                {
                    logger.WriteTo.Console(new RenderedCompactJsonFormatter());
                }
            });
            return services;
        }
    }
}
