using System.Collections.Generic;
using System.Linq;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Hubs.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace AssetFox.Core.UnitTestsCore.TestUtils
{
    public static class HubServiceMockExtensions
    {
        public static List<string> GetFourArgumentErrorMessages(this Mock<IHubService> hubServiceMock)
        {
            var invocations = hubServiceMock.Invocations
                .Where(i => i.Method.Name == nameof(IHubService.SendRealTimeErrorMessage)
                && i.Arguments.Count == 4)
                .ToList();
            var messages = new List<string>();
            foreach (var errorMessage in invocations)
            {
                var message = errorMessage.Arguments[1];
                messages.Add(message.ToString());
            }
            return messages;
        }

        public static string GetSingleFourArgumentErrorMessage(this Mock<IHubService> mock)
        {
            var messages = mock.GetFourArgumentErrorMessages();
            var message = messages.Single();
            return message;
        }

        public static List<string> FourArgumentUserMessages(this Mock<IHubService> mock)
        {
            var invocations = mock.Invocations.ToList();
            var realTimeMessageInvocations = invocations.Where(i => i.Method.Name == nameof(IHubService.SendRealTimeMessage)).ToList();
            var threeArgumentInvocations = realTimeMessageInvocations.Where(i => i.Arguments.Count == 3).ToList();
            var threeArgumentInvocationFinalArguments = threeArgumentInvocations.Select(i => i.Arguments[2].ToString()).ToList();
            return threeArgumentInvocationFinalArguments;
        }

        public static string SingleFourArgumentUserMessage(this Mock<IHubService> mock)
        {
            var messages = mock.FourArgumentUserMessages();
            var message = messages.Single();
            return message;
        }
    }
}
