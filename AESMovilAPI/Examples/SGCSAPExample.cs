using AESMovilAPI.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace AESMovilAPI.Examples
{
    public class SGCSAPExample : IExamplesProvider<Query>
    {
        public Query GetExamples()
        {
            return new Query
            {
                Cuenta = "5301766"
            };
        }
    }
}
