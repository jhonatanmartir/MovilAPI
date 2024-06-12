using AESMovilAPI.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace AESMovilAPI.Examples
{
    public class LoginExample : IExamplesProvider<Login>
    {
        public Login GetExamples()
        {
            return new Login
            {
                Auth = "Basic "
            };
        }
    }
}
