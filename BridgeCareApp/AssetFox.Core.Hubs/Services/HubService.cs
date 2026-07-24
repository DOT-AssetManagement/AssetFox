using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AssetFox.Core.Hubs.Services
{
    public class HubService : IHubService
    {
        private readonly IHubContext<BridgeCareHub> _hubContext;
        private readonly ILogger<HubService> _logger;

        public Dictionary<string, string> _errorList;

        public HubService(
            IHubContext<BridgeCareHub> hubContext,
            ILogger<HubService> logger = null)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? NullLogger<HubService>.Instance;
            _errorList = new Dictionary<string, string>()
            {
                {"Exception", "An exception has occurred." },
                {"SimulationException", "An error has occurred during the simulation." },
                {"InvalidAttributeException", "An invalid attribute upsert occurred." },
                {"InvalidAttributeUpsertException", "An invalid attribute with aggregation rule." },
                {"InvalidOperationException", "An invalid operation occurred." },
                {"CalculateEvaluateException", "Expression could not be compiled." },
                {"Unauthorized", "Unauthorized to access this component." },
                {"AnalysisDefaultData", "Configuration read error." },
                {"InvestmentDefaultData", "Configuration read error." },
                {"RowNotInTable", "Invalid entry into the database." }
            };
        }

        Dictionary<string, string> IHubService.errorList => _errorList;//throw new System.NotImplementedException();

        public void SendRealTimeErrorMessage(string username, string arg, Exception e, bool shouldRethrowError = true)
        {
            SendToUser(username, HubConstant.BroadcastError, arg, e?.StackTrace);

            if (shouldRethrowError)
            {
                var edi = ExceptionDispatchInfo.Capture(e);
                edi.Throw();
            }       
        }
        public void SendRealTimeErrorMessage(string username, string arg1, string arg2, Exception e, bool shouldRethrowError = true)
        {
            SendToUser(username, HubConstant.BroadcastError, arg1, e?.StackTrace, arg2);

            if (shouldRethrowError)
            {
                var edi = ExceptionDispatchInfo.Capture(e);
                edi.Throw();
            }           
        }

        public void SendRealTimeMessage(string username, string method, object arg)
        {
            SendToUser(username, method, arg);
        }

        public void SendRealTimeMessage(string username, string method, object arg1, object arg2)
        {
            SendToUser(username, method, arg1, arg2);
        }

        private void SendToUser(string username, string method, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning(
                    "Skipped SignalR method {Method} because no target username was provided.",
                    method);
                return;
            }

            try
            {
                var groupName = HubUserGroup.GetGroupName(username);
                _hubContext.Clients
                    .Group(groupName)
                    .SendCoreAsync(method, args)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception sendException)
            {
                _logger.LogError(
                    sendException,
                    "Failed to send SignalR method {Method} to user {Username}.",
                    method,
                    username);
            }
        }
    }
}
