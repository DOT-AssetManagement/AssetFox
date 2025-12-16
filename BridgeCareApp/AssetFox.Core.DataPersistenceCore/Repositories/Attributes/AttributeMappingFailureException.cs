using System;
using System.Runtime.Serialization;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    [Serializable]
    internal class AttributeMappingFailureException : Exception
    {
        public AttributeMappingFailureException(string message) : base(message)
        {
        }
    }
}
