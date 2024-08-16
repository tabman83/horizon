using Horizon;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.AddSingleton<IKubernetes>(new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigFile("C:\\Users\\aparisi\\.kube\\config", "lz-nonprod-we-aks")));
builder.Services.AddHostedService<HostedService>();

using var host = builder.Build();
await host.RunAsync();