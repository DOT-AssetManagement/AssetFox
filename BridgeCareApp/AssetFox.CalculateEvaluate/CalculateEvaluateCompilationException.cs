using System;
using System.Runtime.Serialization;

namespace AssetFox.CalculateEvaluate
{
    public class CalculateEvaluateCompilationException : CalculateEvaluateException
    {
        public CalculateEvaluateCompilationException()
        {
        }

        public CalculateEvaluateCompilationException(string message) : base(message)
        {
        }

        public CalculateEvaluateCompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
