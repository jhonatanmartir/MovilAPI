using AESMovilAPI.Responses;
using Swashbuckle.AspNetCore.Filters;

namespace AESMovilAPI.Examples
{
    public class LoginResponseExample : IExamplesProvider<Response<string>>
    {
        public Response<string> GetExamples()
        {
            return new Response<string>
            {
                Success = true,
                Message = "Successfully",
                Data = "https://test.payway.sv/pwo/lnk/bFDIlg1672"
            };
        }
    }
}
