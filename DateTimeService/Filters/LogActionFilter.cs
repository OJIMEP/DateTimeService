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
                _logger.LogError("Error in LogActionFilter", ex);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _watch.Stop();

            var items = context.HttpContext.Items;
            var requestBody = items["LogRequestBody"] != null ? items["LogRequestBody"].ToString() : "";
            
            var responseBody = "";
            if (context.Result is ObjectResult objectResult)
            {
                responseBody = JsonSerializer.Serialize(objectResult.Value);
            }

            var logElement = new ElasticLogElement(context.HttpContext)
            {
                RequestContent = requestBody,
                ResponseContent = responseBody,
                DatabaseConnection = items["DatabaseConnection"] != null ? (string)items["DatabaseConnection"] : "",
                TimeFullExecution = _watch.ElapsedMilliseconds,
                TimeSQLExecutionFact = items["TimeSqlExecutionFact"] != null ? (long)items["TimeSqlExecutionFact"] : 0,
                LoadBalancingExecution = items["LoadBalancingExecution"] != null ? (long)items["LoadBalancingExecution"] : 0,
                TimeLocationExecution = items["TimeLocationExecution"] != null ? (long)items["TimeLocationExecution"] : 0
            };

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
            
            if (logElement.Status == LogStatus.Error) 
            { 
                _logger.LogError(JsonSerializer.Serialize(logElement));
            }
            else
            {
                _logger.LogInformation(JsonSerializer.Serialize(logElement));
            }
        }

        public string FormatRequestBody(IDictionary<string, object> actionArguments)
        {
            try
            {
                if (actionArguments != null)
                    return $"{JsonSerializer.Serialize(actionArguments)}";
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in LogActionFilter", ex);
            }
            return "";
        }
    }
}
