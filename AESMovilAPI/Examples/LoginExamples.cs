using AESMovilAPI.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace AESMovilAPI.Examples
{
    public class LoginExample : IExamplesProvider<LoginDto>
    {
        public LoginDto GetExamples()
        {
            return new LoginDto
            {
                Auth = "Basic "
            };
        }
    }
}
