using AESMovilAPI.Services.Interfaces;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace AESMovilAPI.Services
{
    public class LoggerService<T> : ILoggerService
    {
        private readonly ILogger<T> _logger;
        public LoggerService(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void Err(Exception ex, string message = "", object data = null,
                         [CallerMemberName] string methodName = "",
                         [CallerFilePath] string filePath = "",
                         [CallerLineNumber] int lineNumber = 0)
        {
            string info = $"linea {lineNumber} * {typeof(T).Name}::{methodName}()";
            _logger.LogError(ex, "{method} result: {result} | {data}", info, message, data == null ? "" : JsonConvert.SerializeObject(data));
        }

        public void Info(string message, object data = null, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            string info = $"linea {lineNumber} * {typeof(T).Name}::{methodName}";
            _logger.LogInformation("{method} result: {result} | {data}", info, message, data == null ? "" : JsonConvert.SerializeObject(data));
        }
    }
}
