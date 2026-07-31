using Adapters.Rest.Exception;
using Application.Ports.Driven;
using Application.UseCases;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Rest
{
    public static class ErrorHandlingDependency
    {
        public static void AddExceptionHandling(this IServiceCollection services)
        {
            services.AddExceptionHandler<ErrorHandlingMiddleware>();
        }
    }
}
