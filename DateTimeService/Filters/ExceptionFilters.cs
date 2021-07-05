using DateTimeService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DateTimeService.Filters
{
    public class ConnectionResetExceptionFilter : IAsyncExceptionFilter
    {

        private readonly ILogger<DateTimeController> _logger;

        public ConnectionResetExceptionFilter(ILogger<DateTimeController> logger)
        {
            _logger = logger;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {

            if (context.Exception is Microsoft.AspNetCore.Connections.ConnectionResetException)
            {
                //string actionName = context.ActionDescriptor.DisplayName;
                //string exceptionStack = context.Exception.StackTrace;
                //string exceptionMessage = context.Exception.Message;
                //context.Result = new ContentResult
                //{
                //    Content = $"В методе {actionName} возникло исключение: \n {exceptionMessage} \n {exceptionStack}"
                //};
                context.ExceptionHandled = true;


                _logger.LogInformation("Соединение сброшено");
            }
            return Task.CompletedTask;
        }
    }
}
