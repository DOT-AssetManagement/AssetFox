using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Common;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Hubs;
using NLog;
using AssetFox.Core.Hubs.Services;
using AssetFox.Core.DTOs;
using AssetFox.Core.Common.Logging;
using AssetFox.Core.DTOs.Enums;

namespace AssetFox.Core.Reporting.Logging
{
    public class GeneralWorkQueueLogger : IWorkQueueLog
    {
        private readonly IHubService _hubService;
        private readonly string _username;
        private Action<string> _updateAction;
        private string _workId;

        public GeneralWorkQueueLogger(IHubService hubService, string userName, Action<string> updateAction, string workId)
        {
            _hubService = hubService;
            _username = userName;
            _updateAction = updateAction;
            _workId = workId;
        }

        public void UpdateWorkQueueStatus( string statusMessage)
        {
            var queueStatusUpdate = new QueuedWorkStatusUpdateModel() { Id = _workId, Status = statusMessage};
            _updateAction.Invoke(statusMessage);
            _hubService.SendRealTimeMessage(_username, HubConstant.BroadcastWorkQueueStatusUpdate, queueStatusUpdate);
        }
    }
}
