using Application.Ports.Driving;
using Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ApplicationModuleDependency
    {
        public static void AddApplicationModule(this IServiceCollection services)
        {
            services.AddTransient<IManageProductsService, ManageProductService>();
            services.AddTransient<INotificatorService, NotificatorService>();
        }
    }
}
