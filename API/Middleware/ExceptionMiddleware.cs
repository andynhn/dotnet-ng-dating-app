using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        // request delegate is what's coming up next in the middleware pipeline
        // ILogger logs the exception to the terminal
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _logger = logger;
            _next = next;
        }

        // happening in the context of an http request
        public async Task InvokeAsync(HttpContext context)
        {
            // use try catch here to catch the exception.
            try
            {
                await _next(context); // we get our context and pass the context on to the next piece of middleware. This lives at the top of our middleware. Anything below this will invoke next at some point. Any exception further down in the chain will get thrown up the chain. The exception middleware is at the top of the tree and it catches it at the end.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                
                // if development mode, return actual message and stack trace details.
                // if not development, return status code and "internal server error"
                var response = _env.IsDevelopment() 
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiException(context.Response.StatusCode, "Internal Server Error");
                    // use StackTrace? to prevent getting exceptions from that. Don't want exceptions in our our exception handling middleware.
                
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}