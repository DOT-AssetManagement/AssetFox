using System;
using System.Runtime.Serialization;

namespace AssetFox.Core.Common
{
    [Serializable]
    public class InvalidAttributeUpsertException : Exception
    {
        public InvalidAttributeUpsertException(string message) : base(message)
        {
        }
    }
}
