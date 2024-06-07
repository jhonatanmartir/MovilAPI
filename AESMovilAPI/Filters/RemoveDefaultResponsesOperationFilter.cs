using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AESMovilAPI.Filters
{
    public class RemoveDefaultResponsesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Remueve la respuesta 200 por defecto si existe
            if (operation.Responses.ContainsKey("200"))
            {
                operation.Responses.Remove("200");
            }

            // Para parametros opcionales
            if (operation.Parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.Parameters)
            {
                if (context.ApiDescription.ParameterDescriptions.Any(p => p.Name == parameter.Name && p.RouteInfo != null && p.RouteInfo.IsOptional))
                {
                    parameter.Description += " (Opcional)";
                    parameter.Required = false;
                }
            }
        }
    }
}
