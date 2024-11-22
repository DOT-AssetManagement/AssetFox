using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.Hubs;
using AppliedResearchAssociates.iAM.Hubs.Interfaces;
using AppliedResearchAssociates.iAM.Hubs.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils
{
    public static class HubServiceMockExtensions
    {
        public static List<string> GetErrorMessages(this Mock<IHubService> hubServiceMock)
        {
            var invocations = hubServiceMock.Invocations
                .Where(i => i.Method.Name == nameof(IHubService.SendRealTimeErrorMessage)).ToList();
            var messages = new List<string>();
            foreach (var errorMessage in invocations)
            {
                var message = errorMessage.Arguments[1];
                messages.Add((string)message);
            }
            return messages;
        }
    }
}
