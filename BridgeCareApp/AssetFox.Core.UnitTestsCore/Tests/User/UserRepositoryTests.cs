using System;
using System.Linq;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests.User
{
    public class UserRepositoryTests
    {
        [Fact]
        public async Task AddUser_Does()
        {
            var username = RandomStrings.WithPrefix("user");

            TestHelper.UnitOfWork.UserRepo.AddUser(username, true);

            var userAfter = await TestHelper.UnitOfWork.UserRepo.GetUserByUserName(username);
            var expected = new UserDTO
            {
                Username = username,
                ActiveStatus = true,
                HasInventoryAccess = true,
                CriterionLibrary = new CriterionLibraryDTO(),
            };
            ObjectAssertions.EquivalentExcluding(expected, userAfter, u => u.Id);
            Assert.NotEqual(Guid.Empty, userAfter.Id);
        }

        [Fact]
        public void UpdateLastNewsAccessDate_UserDoesNotExist_DoesNotThrow()
        {
            var nonexistentUserId = Guid.NewGuid();

            TestHelper.UnitOfWork.UserRepo.UpdateLastNewsAccessDate(
                nonexistentUserId, new DateTime(2024, 1, 10));
        }

        [Fact]
        public async Task UpdateLastNewsAccessDate_UserDoesExist_Updates()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var userId = user.Id;

            TestHelper.UnitOfWork.UserRepo.UpdateLastNewsAccessDate(
                userId, new DateTime(2024, 1, 10));

            var userAfter = await TestHelper.UnitOfWork.UserRepo.GetUserById(userId);
            Assert.Equal(new DateTime(2024, 1, 10), userAfter.LastNewsAccessDate);
        }

        [Fact]
        public async Task GetAllUsers_UserExists_UserIsInList()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);

            var allUsers = await TestHelper.UnitOfWork.UserRepo.GetAllUsers();

            var foundUser = allUsers.Single(u => u.Id == user.Id);
            ObjectAssertions.Equivalent(user, foundUser);
        }

        [Fact]
        public async Task GetUserByUsername_OrById_UserExists_GetsSame()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);

            var foundUser1 = await TestHelper.UnitOfWork.UserRepo.GetUserByUserName(user.Username);
            var foundUser2 = await TestHelper.UnitOfWork.UserRepo.GetUserById(user.Id);

            Assert.NotNull(foundUser1);
            ObjectAssertions.Equivalent(foundUser1, foundUser2);
        }

        [Fact]
        public void UserExists_DoesNotExist_False()
        {
            var nonexistentUsername = "noSuchUser";

            var exists = TestHelper.UnitOfWork.UserRepo.UserExists(nonexistentUsername);

            Assert.False(exists);
        }

        [Fact]
        public async Task UserExists_DoesExist_True()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);

            var exists = TestHelper.UnitOfWork.UserRepo.UserExists(user.Username);

            Assert.True(exists);
        }
    }
}
