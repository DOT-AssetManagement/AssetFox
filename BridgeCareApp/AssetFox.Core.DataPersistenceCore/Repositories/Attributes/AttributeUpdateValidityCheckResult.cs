using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetFox.Core.DataPersistenceCore.Repositories.Attributes
{
    public class AttributeUpdateValidityCheckResult
    {
        public bool Ok { get; set; }
        public string Message { get; set; }
    }
}
