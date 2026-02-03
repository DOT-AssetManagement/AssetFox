using System;
using System.Runtime.Serialization;

namespace AssetFox.AFCore
{
    public class MalformedInputException : Exception
    {
        public MalformedInputException()
        {
        }

        public MalformedInputException(string message) : base(message)
        {
        }

        public MalformedInputException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MalformedInputException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
