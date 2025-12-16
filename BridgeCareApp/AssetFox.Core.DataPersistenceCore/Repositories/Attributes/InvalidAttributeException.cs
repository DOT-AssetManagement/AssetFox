using System;
using System.Runtime.Serialization;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    [Serializable]
    public class InvalidAttributeException : Exception
    {
        public InvalidAttributeException(string message) : base(message)
        {
        }
    }
}
