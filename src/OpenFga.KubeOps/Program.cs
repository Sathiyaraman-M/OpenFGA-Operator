using KubeOps.Operator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenFga.KubeOps.Extensions;
using OpenFga.KubeOps.Services;
using OpenFga.KubeOps.Services.Resolvers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(builder.Configuration);

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

builder.Services.AddScoped<ConnectionConfigResolver>();
builder.Services.AddScoped<AuthorizationStoreResolver>();
builder.Services.AddScoped<OpenFgaClientFactory>();
builder.Services.AddScoped<OpenFgaService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<ModelService>();
builder.Services.AddScoped<TupleSetService>();

using var host = builder.Build();
await host.RunAsync();
