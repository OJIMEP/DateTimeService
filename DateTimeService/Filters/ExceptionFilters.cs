using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DateTimeService.Filters
{
    public class ConnectionResetExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            
            if (context.Exception is Microsoft.AspNetCore.Connections.ConnectionResetException)
            {
                string actionName = context.ActionDescriptor.DisplayName;
                string exceptionStack = context.Exception.StackTrace;
                string exceptionMessage = context.Exception.Message;
                context.Result = new ContentResult
                {
                    Content = $"В методе {actionName} возникло исключение: \n {exceptionMessage} \n {exceptionStack}"
                };
                context.ExceptionHandled = true;
            }
            return Task.CompletedTask;
        }
    }
}
