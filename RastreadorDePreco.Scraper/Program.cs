using Adapters.Coletor;
using Adapters.Messaging;
using Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var hostbuilder = Host.CreateApplicationBuilder(args);

ApplicationModuleDependency.AddApplicationModule(hostbuilder.Services);
AdaptersMessagingDependency.AddAdaptersMessaging(hostbuilder.Services, hostbuilder.Configuration);
ColetorModuleDependency.AddColetorModule(hostbuilder.Services, hostbuilder.Configuration);

await hostbuilder.Build().RunAsync();
