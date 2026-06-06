using KubeOps.Operator;

using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddKubernetesOperator()
#if DEBUG
    .AddCrdInstaller(c =>
    {
        // Careful, these can be very destructive.
        // c.WithOverwriteExisting()
        //     .WithDeleteOnShutdown();
    })
#endif
    .RegisterComponents();

using var host = builder.Build();
await host.RunAsync();
