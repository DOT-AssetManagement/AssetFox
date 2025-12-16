using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Controllers.BaseController;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AssetFoxCoreTests.Tests
{
    public class AssetFoxCoreBaseControllerTests
    {
        private AssetFoxCoreBaseController CreateController(Mock<IUnitOfWork> unitOfWork, IHttpContextAccessor accessor)
        {
            var security = EsecSecurityMocks.Dbe;
            var hubService = HubServiceMocks.Default();
            var controller = new AssetFoxCoreBaseController(
                security,
                unitOfWork.Object,
                hubService,
                accessor
                );
            return controller;
        }

        [Fact]
        public void RequestHasBearer_DefaultController_True()
        {
            var unitOfWork = UnitOfWorkMocks.EveryoneExists();
            var accessor = HttpContextAccessorMocks.Default();
            var controller = CreateController(unitOfWork, accessor);
            var hasBearer = controller.RequestHasBearer();
            Assert.True(hasBearer);
        }

        [Fact]
        public void RequestHasBearer_PathIsInPathsToIgnore_False()
        {
            var unitOfWork = UnitOfWorkMocks.EveryoneExists();
            var accessor = HttpContextAccessorMocks.Default();
            var context = accessor.HttpContext;
            var request = context.Request;
            var path = new PathString("/UserTokens");
            request.Path = path;
            var controller = CreateController(unitOfWork, accessor);
            var hasBearer = controller.RequestHasBearer();
            Assert.False(hasBearer);
        }

        [Fact]
        public void RequestHasBearer_NoContextAccessor_False()
        {
            var unitOfWork = UnitOfWorkMocks.EveryoneExists();
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(null as HttpContext);
            var controller = CreateController(unitOfWork, accessor.Object);
            var hasBearer = controller.RequestHasBearer();
            Assert.False(hasBearer);
        }

        [Fact]
        public void RequestHasBearer_NoRequestInHttpContext_False()
        {
            var unitOfWork = UnitOfWorkMocks.EveryoneExists();
            var context = new Mock<HttpContext>();
            context.Setup(c => c.Request).Returns(null as HttpRequest);
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context.Object);
            var controller = CreateController(unitOfWork, accessor.Object);
            var hasBearer = controller.RequestHasBearer();
            Assert.False(hasBearer);
        }
    }
}
