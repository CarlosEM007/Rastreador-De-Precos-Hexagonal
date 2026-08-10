using Adapters.Messaging;
using Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var config = new ConfigurationBuilder().Build();

ApplicationModuleDependency.AddApplicationModule(services);
AdaptersMessagingDependency.AddAdaptersMessaging(services, config);