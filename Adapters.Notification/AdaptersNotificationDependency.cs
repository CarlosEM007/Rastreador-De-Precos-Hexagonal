using Adapters.Notification.Notificator;
using Application.Ports.Driven;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Notification
{
    public static class AdaptersNotificationDependency
    {
        public static void AddAdaptersNotification(this IServiceCollection services)
        {
            services.AddTransient<INotificationGateway, LogNotificator>();
        }
    }
}
