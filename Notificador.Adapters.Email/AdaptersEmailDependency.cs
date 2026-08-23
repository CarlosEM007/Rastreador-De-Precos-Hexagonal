using Adapters.Email.Enviador;
using Application.Ports.Driven;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Email
{
    public static class AdaptersEmailDependency
    {
        public static void AddAdaptersEmail(this IServiceCollection services)
        {
            services.AddSingleton<IEnviadorDeAlerta, FakeEmailEnviador>();
        }
    }
}
