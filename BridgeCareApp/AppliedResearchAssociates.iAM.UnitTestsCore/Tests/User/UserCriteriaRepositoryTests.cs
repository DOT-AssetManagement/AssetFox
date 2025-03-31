using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User
{
    public class UserCriteriaRepositoryTests
    {
        [Fact]
        public async Task GetOwnUserCriteria_AdminUserExistsWithNoCriteria_GeneratesAndReturnsAdminCriteria()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, true);
            var userInfo = new UserInfoDTO
            {
                Sub = user.Username,
                HasAdminAccess = true,
            };

            var userCriteria = TestHelper.UnitOfWork.UserCriteriaRepo.GetOwnUserCriteria(userInfo);
            var expected = new UserCriteriaDTO
            {
                HasAccess = true,
                HasCriteria = false,
                UserId = user.Id,
                UserName = user.Username,
            };
            ObjectAssertions.EquivalentExcluding(expected, userCriteria, x => x.CriteriaId);
        }

        [Fact]
        public async Task GetAllUserCriteria_UserExistsWithoutCriteria_UserIsNotInList()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, true);
            var userInfo = new UserInfoDTO
            {
                Sub = user.Username,
                HasAdminAccess = true,
            };

            var allUserCriteria = TestHelper.UnitOfWork.UserCriteriaRepo.GetAllUserCriteria();

            var criteriaForUser = allUserCriteria.SingleOrDefault(uc => uc.UserId == user.Id);
            Assert.Null(criteriaForUser);
        }

        [Fact]
        public async Task GetAllUserCriteria_UserExistsWithCriteria_Expected()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, true);
            var userCriteria = UserCriteriaTestSetup.ModelForEntityInDb(user.Id, user.Username, true);
            var userInfo = new UserInfoDTO
            {
                Sub = user.Username,
                HasAdminAccess = true,
            };

            var allUserCriteria = TestHelper.UnitOfWork.UserCriteriaRepo.GetAllUserCriteria();

            var criteriaForUser = allUserCriteria.SingleOrDefault(uc => uc.UserId == user.Id);
            var expected = new UserCriteriaDTO
            {
                UserId = user.Id,
                UserName = user.Username,
                HasAccess = true,
            };
            ObjectAssertions.EquivalentExcluding(expected, criteriaForUser, c => c.CriteriaId);
        }

        [Fact]
        public async Task GetOwnUserCriteria_UserDoesNotExist_Expected()
        {
            var username = RandomStrings.WithPrefix("User");
            var userInfo = new UserInfoDTO
            {
                Sub = username,
                HasAdminAccess = true,
            };

            var userCriteria = TestHelper.UnitOfWork.UserCriteriaRepo.GetOwnUserCriteria(userInfo);

            var expected = new UserCriteriaDTO
            {
                HasAccess = true,
                HasCriteria = false,
                UserName = username,
            };
            ObjectAssertions.EquivalentExcluding(expected, userCriteria, x => x.CriteriaId, x => x.UserId);
            var userInDb = await TestHelper.UnitOfWork.UserRepo.GetUserById(userCriteria.UserId);
            Assert.NotNull(userInDb);
        }

        [Fact]
        public void UpsertUserCriteria_UserDoesNotExist_Throws()
        {
            var userId = Guid.NewGuid();
            var dto = new UserCriteriaDTO
            {
                UserId = userId,
            };

            var exception = Assert.Throws<RowNotInTableException>(() => TestHelper.UnitOfWork.UserCriteriaRepo
                .UpsertUserCriteria(dto));

            Assert.Equal(UserCriteriaRepository.TheUserWasNotFound, exception.Message);
        }


        [Fact]
        public async void UpsertUserCriteria_ExistsButCriteriaDont_Adds()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, false);
            var userId = user.Id;
            var dto = UserCriteriaDtos.Dto(userId, user.Username);
            var filterBefore = TestHelper.UnitOfWork.Context.UserCriteria.SingleOrDefault(uc => uc.UserId == userId);
            Assert.Null(filterBefore);

            TestHelper.UnitOfWork.UserCriteriaRepo
                .UpsertUserCriteria(dto);

            var filterAfter = TestHelper.UnitOfWork.UserCriteriaRepo.GetUserCriteria(userId);
            Assert.Equal("Criteria", filterAfter);
        }

        [Fact]
        public void RevokeUserAccess_UserCriteriaDoNotExist_DoesNothing()
        {
            var nonexistentCriteriaId = Guid.NewGuid();

            TestHelper.UnitOfWork.UserCriteriaRepo
                .RevokeUserAccess(nonexistentCriteriaId);
        }

        [Fact]
        public async Task RevokeUserAccess_UserCriteriaExist_Deletes()
        {
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork, true);
            var userId = user.Id;
            var dto = new UserCriteriaDTO
            {
                UserId = userId,
                HasAccess = true,
            };
            TestHelper.UnitOfWork.UserCriteriaRepo
                .UpsertUserCriteria(dto);
            var userCriteriaEntityBefore = TestHelper.UnitOfWork.Context
                .UserCriteria.SingleOrDefault(uc => uc.UserId == userId);
            Assert.NotNull(userCriteriaEntityBefore);
            var userBefore = TestHelper.UnitOfWork.Context.User
                .Single(u => u.Id == user.Id);
            Assert.True(userBefore.HasInventoryAccess);

            TestHelper.UnitOfWork.UserCriteriaRepo.RevokeUserAccess(userCriteriaEntityBefore.UserCriteriaId);

            var userCriteriaEntityAfter = TestHelper.UnitOfWork.Context
            .UserCriteria.SingleOrDefault(uc => uc.UserId == userId);
            Assert.Null(userCriteriaEntityAfter);
            var userAfter = TestHelper.UnitOfWork.Context.User
                .Single(u => u.Id == user.Id);
            Assert.False(userAfter.HasInventoryAccess);
        }
    }
}
