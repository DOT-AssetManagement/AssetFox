using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.TestUtils;
using AssetFoxCore.Models;
using AssetFoxCore.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;

namespace AssetFoxCoreTests
{
    public static class EsecSecurityMocks
    {
        public const string AdminRole = "PD-BAMS-Administrator";
        public const string AdminEmail = "pdstseseca5@pa.gov";

        public const string DbeUsername = "b-bamsadmin";
        public const string DbeRole = "PD-BAMS-DBEngineer";
        public const string DbeEmail = "jmalmberg@ara.com";

        public static Mock<IEsecSecurity> AdminMock
        {
            get
            {
                var mock = new Mock<IEsecSecurity>();
                mock.Setup(_ => _.GetUserInformation(It.IsAny<HttpRequest>()))
                .Returns(new UserInfo
                {
                    Name = TestUsernames.Admin,
                    HasAdminAccess = true,
                    HasSimulationAccess = true,
                    Email = AdminEmail,
                });
                return mock;
            }
        }

        public static IEsecSecurity Admin
        {
            get
            {
                var mock = AdminMock;
                return mock.Object;
            }
        }

        public static Mock<IEsecSecurity> DbeMock
        {
            get
            {
                var mock = new Mock<IEsecSecurity>();
                mock.Setup(_ => _.GetUserInformation(It.IsAny<HttpRequest>()))
                    .Returns(new UserInfo
                    {
                        Name = DbeUsername,
                        HasAdminAccess = false,
                        HasSimulationAccess = false,
                        Email = DbeEmail,
                    });
                return mock;
            }
        }

        public static IEsecSecurity Dbe
        {
            get
            {
                return DbeMock.Object;
            }
        }


        public static IEsecSecurity DbeForUser(UserDTO user)
        {
            var userInfo = new UserInfo
            {
                Name = user.Username,
                HasAdminAccess = false,
                HasSimulationAccess = false,
                Email = "random@gmail.com",
            };
            var mock = ForUserInfo(userInfo);
            return mock.Object;
        }

        public static Mock<IEsecSecurity> ForUserInfo(UserInfo userInfo)
        {
            var mock = new Mock<IEsecSecurity>();
            mock.Setup(_ => _.GetUserInformation(It.IsAny<HttpRequest>()))
            .Returns(userInfo);
            return mock;
        }
    }
}
