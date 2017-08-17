using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace tracktor.app
{
    public class ExceptionHandling
    {
        private readonly RequestDelegate _next;
        private ILogger _logger;

        public ExceptionHandling(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger("Exception");
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            if(exception is UnauthorizedAccessException)
            {
                code = HttpStatusCode.Unauthorized;
            }
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(exception.Message);
        }
    }
}
