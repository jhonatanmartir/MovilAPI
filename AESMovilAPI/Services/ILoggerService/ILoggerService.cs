using System.Runtime.CompilerServices;

namespace AESMovilAPI.Services.Interfaces
{
    public interface ILoggerService
    {
        void Info(string message, object data = null,
                        [CallerMemberName] string methodName = "",
                        [CallerFilePath] string filePath = "",
                        [CallerLineNumber] int lineNumber = 0);
        void Err(Exception ex, string message = "", object data = null,
                         [CallerMemberName] string methodName = "",
                         [CallerFilePath] string filePath = "",
                         [CallerLineNumber] int lineNumber = 0);
    }
}
