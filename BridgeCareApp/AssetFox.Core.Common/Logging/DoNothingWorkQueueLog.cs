using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetFox.Core.Common.Logging
{
    public class DoNothingWorkQueueLog : IWorkQueueLog
    {
        public void UpdateWorkQueueStatus(string statusMessage) { }
    }
}
