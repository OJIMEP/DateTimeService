using DateTimeService.Logging;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace DateTimeService.Middlewares
{
    public class GlobalExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var logElement = new ElasticLogElement(context)
                {
                    Status = LogStatus.Error,
                    ErrorDescription = ex.Message
                };

                logElement.FillFromHttpContextItems(context.Items);

                _logger.LogMessageGen(JsonSerializer.Serialize(logElement));

                if (ex is ValidationException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }

                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(ex.Message);
            }
        }
    }
}
