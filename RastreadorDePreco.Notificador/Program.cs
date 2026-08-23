using Adapters.Email;
using Adapters.Messaging;
using Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var hostbuilder = Host.CreateApplicationBuilder(args);

ApplicationModuleDependency.AddApplicationModule(hostbuilder.Services);
AdaptersMessagingDependency.AddAdaptersMessaging(hostbuilder.Services, hostbuilder.Configuration);
AdaptersEmailDependency.AddAdaptersEmail(hostbuilder.Services);

await hostbuilder.Build().RunAsync();
