using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.WorkQueue
{
    public static class WorkQueueWorkIdFactory
    {
        public static string CreateId(Guid domainId, WorkType workType)
        {
            return domainId.ToString() + workType.ToString();
        }
    }
}
