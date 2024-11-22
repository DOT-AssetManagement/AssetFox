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
        public static List<string> GetThreeArgumentErrorMessages(this Mock<IHubService> hubServiceMock)
        {
            var invocations = hubServiceMock.Invocations
                .Where(i => i.Method.Name == nameof(IHubService.SendRealTimeErrorMessage)
                && i.Arguments.Count == 3)
                .ToList();
            var messages = new List<string>();
            foreach (var errorMessage in invocations)
            {
                var message = errorMessage.Arguments[1];
                messages.Add(message.ToString());
            }
            return messages;
        }


        public static List<string> ThreeArgumentUserMessages(this Mock<IHubService> mock)
        {
            var invocations = mock.Invocations.ToList();
            var realTimeMessageInvocations = invocations.Where(i => i.Method.Name == nameof(IHubService.SendRealTimeMessage)).ToList();
            var threeArgumentInvocations = realTimeMessageInvocations.Where(i => i.Arguments.Count == 3).ToList();
            var threeArgumentInvocationFinalArguments = threeArgumentInvocations.Select(i => i.Arguments[2].ToString()).ToList();
            return threeArgumentInvocationFinalArguments;
        }

        public static string SingleThreeArgumentUserMessage(this Mock<IHubService> mock)
        {
            var messages = mock.ThreeArgumentUserMessages();
            var message = messages.Single();
            return message;
        }
    }
}
