using Adapters.Rest.Exception;
using Microsoft.Extensions.DependencyInjection;

namespace Adapters.Rest
{
    public static class ErrorHandlingDependency
    {
        public static void AddExceptionHandling(this IServiceCollection services)
        {
            services.AddProblemDetails();
            services.AddExceptionHandler<ErrorHandlingMiddleware>();
        }
    }
}
