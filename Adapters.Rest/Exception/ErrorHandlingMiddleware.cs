using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace Adapters.Rest.Exception
{
    public class ErrorHandlingMiddleware : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, System.Exception exception, CancellationToken cancellationToken)
        {
            var result = JsonSerializer.Serialize(new {error = exception?.Message});

            httpContext.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                ProductNotFoundException => HttpStatusCode.NotFound,
                InvalidProductURLException => HttpStatusCode.BadRequest,
                InvalidProductValueException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError
            };

            httpContext.Response.StatusCode = Convert.ToInt32(statusCode);
            await httpContext.Response.WriteAsync(result, cancellationToken: cancellationToken);

            return true;
        }
    }
}
