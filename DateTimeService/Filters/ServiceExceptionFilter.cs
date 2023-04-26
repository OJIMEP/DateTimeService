using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DateTimeService.Filters
{
    public class ServiceExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException)
            {
                context.HttpContext.Response.StatusCode = 400;
            }
            else
            {
                context.HttpContext.Response.StatusCode = 500;
            }
            context.Result = new ObjectResult( context.Exception.Message );
            
            context.ExceptionHandled = true;
        }
    }
}
