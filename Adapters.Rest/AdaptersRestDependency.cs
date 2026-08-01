using Application.Ports.Driving;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Rest
{
    public static class AdaptersRestDependency
    {
        public static void AddAdaptersRest(this IServiceCollection services)
        {
            services.AddControllers().AddApplicationPart(typeof(AdaptersRestDependency).Assembly);
        }
    }
}
