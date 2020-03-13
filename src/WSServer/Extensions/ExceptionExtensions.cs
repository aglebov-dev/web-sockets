using System;
using System.Text;

namespace WSServer.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception exception)
        {
            var sb = new StringBuilder(exception.Message);
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
                sb.AppendLine(exception.Message);
            }

            return sb.ToString();
        }
    }
}
