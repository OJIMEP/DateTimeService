using DateTimeService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using DateTimeService.Logging;

namespace DateTimeService.Filters
{
    public class LogActionFilter : IActionFilter
    {
        private readonly ILogger<DateTimeController> _logger;
        Stopwatch _watch;

        public LogActionFilter(ILogger<DateTimeController> logger)
        {
            _logger = logger;
        }
        
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _watch = Stopwatch.StartNew();

            try
            {
                if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    var requestBody = FormatRequestBody(context.ActionArguments);
                    context.HttpContext.Items["LogRequestBody"] = requestBody;
                }
            }
            catch (Exception ex)
            {
                _logger.LogErrorMessage("Error in LogActionFilter", ex);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _watch.Stop();

            if (context.Result is null)
            {
                return;
            }

            var responseBody = "";
            if (context.Result is ObjectResult objectResult)
            {
                responseBody = JsonSerializer.Serialize(objectResult.Value);
            }

            var logElement = new ElasticLogElement(context.HttpContext)
            {
                ResponseContent = responseBody,
                TimeFullExecution = _watch.ElapsedMilliseconds           
            };

            logElement.FillFromHttpContextItems(context.HttpContext.Items);

            if (context.Result is BadRequestObjectResult badRequest)
            {
                logElement.Status = LogStatus.Error;
                logElement.ErrorDescription = "Некорректные входные данные";
                logElement.AdditionalData.Add("InputErrors", JsonSerializer.Serialize(badRequest.Value));
            }
            if (context.Result is ObjectResult internalError && internalError.StatusCode == StatusCodes.Status500InternalServerError)
            {
                logElement.Status = LogStatus.Error;
                logElement.ErrorDescription = internalError.Value.ToString();
            }

            _logger.LogMessageGen(JsonSerializer.Serialize(logElement));
        }

        public string FormatRequestBody(IDictionary<string, object> actionArguments)
        {
            try
            {
                if (actionArguments != null)
                    return $"{JsonSerializer.Serialize(actionArguments["inputData"])}";
            }
            catch (Exception ex)
            {
                _logger.LogErrorMessage("Error in LogActionFilter", ex);
            }
            return "";
        }
    }
}
