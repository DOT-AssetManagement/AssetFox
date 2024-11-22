using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.Hubs;
using AppliedResearchAssociates.iAM.Hubs.Interfaces;
using AppliedResearchAssociates.iAM.Hubs.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils
{
    public static class HubServiceMocks
    {
        private static Dictionary<string, string> _createErrorList()
        {
            var dummyHubContext = new Mock<IHubContext<BridgeCareHub>>();
            var dummy = new HubService(dummyHubContext.Object);
            var castDummy = dummy as IHubService;
            var dictionary = castDummy.errorList;
            return dictionary;
        }

        public static Mock<IHubService> DefaultMock()
        {
            var mock = new Mock<IHubService>();
            var errorList = _createErrorList();
            mock.Setup(m => m.errorList).Returns(errorList);
            return mock;
        }

        public static IHubService Default()
        {
            var mock = DefaultMock();
            return mock.Object;
        }
    }
}
