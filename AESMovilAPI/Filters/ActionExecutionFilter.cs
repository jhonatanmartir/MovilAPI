using AESMovilAPI.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AESMovilAPI.Filters
{
    public class ActionExecutionFilter : IActionFilter
    {
        protected readonly LoggerService<ActionExecutionFilter> _log;

        public ActionExecutionFilter(LoggerService<ActionExecutionFilter> log)
        {
            _log = log;
        }

        // Se ejecuta antes de que la acción sea llamada
        public void OnActionExecuting(ActionExecutingContext context)
        {
            string actionName = context.ActionDescriptor.DisplayName;

            _log.Info(message: "IN", data: context.ActionArguments, methodName: actionName);
        }

        // Se ejecuta después de que la acción ha sido ejecutada
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var actionName = context.ActionDescriptor.DisplayName;
            var result = context.Result;

            _log.Info(message: "LEFT", data: result, methodName: actionName);
        }
    }
}
