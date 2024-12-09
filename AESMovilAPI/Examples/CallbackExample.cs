using AESMovilAPI.DTOs;
using Swashbuckle.AspNetCore.Filters;

namespace AESMovilAPI.Examples
{
    public class CallbackExample : IExamplesProvider<Callback>
    {
        public Callback GetExamples()
        {
            return new Callback
            {
                Code = "675248c7f776b416fa7ab714",
                Option = 0
            };
        }
    }
}
