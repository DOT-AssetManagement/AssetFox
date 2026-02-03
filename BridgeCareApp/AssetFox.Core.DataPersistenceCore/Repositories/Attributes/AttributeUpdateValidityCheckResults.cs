using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetFox.Core.DataPersistenceCore.Repositories.Attributes
{
    public static class AttributeUpdateValidityCheckResults
    {
        public static AttributeUpdateValidityCheckResult Ok()
            => new()
            {
                Ok = true,
            };

        public static AttributeUpdateValidityCheckResult NotOk(string message)
            => new()
            {
                Ok = false,
                Message = message
            };
    }
}
