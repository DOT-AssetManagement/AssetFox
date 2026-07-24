using System;
using System.Threading;
using System.Threading.Tasks;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests.Hub
{
    public class HubServiceTests
    {
        [Fact]
        public async Task SendRealTimeMessage_NormalizesUsernameAndWaitsForSend()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var sendCompletion = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            var client = new Mock<IClientProxy>();
            client
                .Setup(c => c.SendCoreAsync(
                    "TestMethod",
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
                .Returns(sendCompletion.Task);
            var service = CreateService(client, out var clients);

            var send = Task.Run(
                () => service.SendRealTimeMessage(
                    "  Tyler.User  ",
                    "TestMethod",
                    "payload"),
                cancellationToken);

            var firstCompleted = await Task.WhenAny(
                send,
                Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken));
            Assert.NotSame(send, firstCompleted);

            sendCompletion.SetResult(null);
            await send.WaitAsync(TimeSpan.FromSeconds(1), cancellationToken);

            clients.Verify(c => c.Group("TYLER.USER"), Times.Once);
            client.Verify(c => c.SendCoreAsync(
                "TestMethod",
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    (string)args[0] == "payload"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public void SendRealTimeMessage_WithoutUsername_DoesNotBroadcast()
        {
            var client = new Mock<IClientProxy>();
            var service = CreateService(client, out var clients);

            service.SendRealTimeMessage(
                " ",
                "TestMethod",
                "payload");

            clients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
            clients.VerifyGet(c => c.All, Times.Never);
        }

        [Fact]
        public void SendRealTimeMessage_WhenSignalRFails_DoesNotFailDomainWork()
        {
            var client = new Mock<IClientProxy>();
            client
                .Setup(c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromException(
                    new InvalidOperationException("SignalR unavailable")));
            var logger = new Mock<ILogger<HubService>>();
            var service = CreateService(client, out _, logger);

            var exception = Record.Exception(
                () => service.SendRealTimeMessage(
                    "Tyler.User",
                    "TestMethod",
                    "payload"));

            Assert.Null(exception);
            logger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString().Contains("Failed to send SignalR method")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void SendRealTimeErrorMessage_WhenSignalRFails_RethrowsOriginalError()
        {
            var client = new Mock<IClientProxy>();
            client
                .Setup(c => c.SendCoreAsync(
                    It.IsAny<string>(),
                    It.IsAny<object[]>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromException(
                    new InvalidOperationException("SignalR unavailable")));
            var service = CreateService(client, out _);
            var original = new ArgumentException("Domain work failed");

            var thrown = Assert.Throws<ArgumentException>(
                () => service.SendRealTimeErrorMessage(
                    "Tyler.User",
                    "Domain work failed",
                    original));

            Assert.Same(original, thrown);
        }

        [Theory]
        [InlineData("Tyler.User", "TYLER.USER")]
        [InlineData("  TYLER.user ", "TYLER.USER")]
        public void GetGroupName_NormalizesCaseAndWhitespace(
            string username,
            string expected)
        {
            Assert.Equal(expected, HubUserGroup.GetGroupName(username));
        }

        private static HubService CreateService(
            Mock<IClientProxy> client,
            out Mock<IHubClients> clients,
            Mock<ILogger<HubService>> logger = null)
        {
            clients = new Mock<IHubClients>();
            clients
                .Setup(c => c.Group(It.IsAny<string>()))
                .Returns(client.Object);

            var hubContext = new Mock<IHubContext<BridgeCareHub>>();
            hubContext.SetupGet(c => c.Clients).Returns(clients.Object);

            return new HubService(hubContext.Object, logger?.Object);
        }
    }
}
