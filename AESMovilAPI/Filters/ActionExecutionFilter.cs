using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AESMovilAPI.Filters
{
    public class ActionExecutionFilter : IActionFilter
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        // Se ejecuta antes de que la acción sea llamada
        public void OnActionExecuting(ActionExecutingContext context)
        {
            string actionName = context.ActionDescriptor.DisplayName;
            Info(data: context.ActionArguments, caller: actionName);
        }

        // Se ejecuta después de que la acción ha sido ejecutada
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            var result = context.Result;

            if (context.Exception == null)
            {
                // Si no hay excepción, se loguea el resultado
                Info(data: result, caller: actionName);
            }
            else
            {
                // Si hay una excepción, se loguea el error
                Info(message: "Error", caller: actionName);
            }
        }

        protected static void Info(object? data = null, string message = "", bool isAsync = true, [CallerMemberName] string caller = "")
        {
            string info;
            if (isAsync)
            {
                info = caller;
            }
            else
            {
                var stackFrame = new StackFrame(1, true);
                info = "line " + stackFrame.GetFileLineNumber() + "::" + stackFrame.GetMethod().Name;
            }

            if (data != null)
            {
                _logger.Info("{method} result: {result} | {data}", info, message, JsonConvert.SerializeObject(data));
            }
            else
            {
                _logger.Info("{method} result: {result}", info, message);
            }
        }
    }
}
