using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL;
using AppliedResearchAssociates.iAM.DTOs;
using AppliedResearchAssociates.iAM.DTOs.Enums;
using AppliedResearchAssociates.iAM.TestHelpers;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Repositories;
using AppliedResearchAssociates.iAM.UnitTestsCore.Tests.User;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;
using Xunit;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests.Attributes.CalculatedAttributes
{
    public class CalculatedAttributeRepositoryRealDatabaseTests
    {
        [Fact]
        public async Task UpdateCalculatedAttributeLibraryWithUserAccessChange_Does()
        {
            TestHelper.UnitOfWork.ClearAttributeIdNameCache();
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var library = CalculatedAttributeLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            CalculatedAttributeLibraryUserTestSetup.SetUsersOfCalculatedAttributeLibrary(TestHelper.UnitOfWork, library.Id, LibraryAccessLevel.Modify, user.Id);
            var libraryUsersBefore = TestHelper.UnitOfWork.CalculatedAttributeRepo.GetLibraryUsers(library.Id);
            var libraryUserBefore = libraryUsersBefore.Single();
            Assert.Equal(LibraryAccessLevel.Modify, libraryUserBefore.AccessLevel);
            libraryUserBefore.AccessLevel = LibraryAccessLevel.Read;

            TestHelper.UnitOfWork.CalculatedAttributeRepo.UpsertOrDeleteUsers(library.Id, libraryUsersBefore);

            var libraryUsersAfter = TestHelper.UnitOfWork.CalculatedAttributeRepo.GetLibraryUsers(library.Id);
            var libraryUserAfter = libraryUsersAfter.Single();
            Assert.Equal(LibraryAccessLevel.Read, libraryUserAfter.AccessLevel);
        }

        [Fact]
        public async Task UpdateCalculatedAttributeLibraryUsers_RequestAccessRemoval_Does()
        {
            TestHelper.UnitOfWork.ClearAttributeIdNameCache();
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var library = CalculatedAttributeLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            CalculatedAttributeLibraryUserTestSetup.SetUsersOfCalculatedAttributeLibrary(TestHelper.UnitOfWork, library.Id, LibraryAccessLevel.Modify, user.Id);
            var libraryUsersBefore = TestHelper.UnitOfWork.CalculatedAttributeRepo.GetLibraryUsers(library.Id);
            var libraryUserBefore = libraryUsersBefore.Single();
            libraryUsersBefore.Remove(libraryUserBefore);

            TestHelper.UnitOfWork.CalculatedAttributeRepo.UpsertOrDeleteUsers(library.Id, libraryUsersBefore);
            TestHelper.UnitOfWork.Context.SaveChanges();

            var libraryUsersAfter = TestHelper.UnitOfWork.CalculatedAttributeRepo.GetLibraryUsers(library.Id);
            Assert.Empty(libraryUsersAfter);
        }

        [Fact]
        public async Task UpdateLibraryUsers_AddAccessForUser_Does()
        {
            TestHelper.UnitOfWork.ClearAttributeIdNameCache();
            var user1 = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var user2 = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var library = CalculatedAttributeLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            CalculatedAttributeLibraryUserTestSetup.SetUsersOfCalculatedAttributeLibrary(TestHelper.UnitOfWork, library.Id, LibraryAccessLevel.Modify, user1.Id);
            var usersBefore = TestHelper.UnitOfWork.CalculatedAttributeRepo.GetLibraryUsers(library.Id);
            var newUser = new LibraryUserDTO
            {
                AccessLevel = LibraryAccessLevel.Read,
                UserId = user2.Id,
            };
            usersBefore.Add(newUser);

            TestHelper.UnitOfWork.CalculatedAttributeRepo.UpsertOrDeleteUsers(library.Id, usersBefore);

            var libraryUsersAfter = TestHelper.UnitOfWork.CalculatedAttributeRepo.GetLibraryUsers(library.Id);
            var user1After = libraryUsersAfter.Single(u => u.UserId == user1.Id);
            var user2After = libraryUsersAfter.Single(u => u.UserId == user2.Id);
            Assert.Equal(LibraryAccessLevel.Modify, user1After.AccessLevel);
            Assert.Equal(LibraryAccessLevel.Read, user2After.AccessLevel);
        }

        [Fact]
        public void GetCalculatedAttributesByLibraryIdNoChildren_LibraryInDbWithCalculatedAttribute_Gets()
        {
            TestHelper.UnitOfWork.ClearAttributeIdNameCache();
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var library = CalculatedAttributeLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var calculatedAttributeId = Guid.NewGuid();
            var calculatedAttribute = CalculatedAttributeTestSetup.TestCalculatedAttributeInLibraryInDb(TestHelper.UnitOfWork,
                library, calculatedAttributeId, TestAttributeNames.CulvSeeded);

            var actual = TestHelper.UnitOfWork.CalculatedAttributeRepo.GetCalcuatedAttributesByLibraryIdNoChildren(library.Id);

            var foundAttribute = actual.Single();
            ObjectAssertions.EquivalentExcluding(calculatedAttribute, foundAttribute, ca => ca.Equations);
        }

        [Fact]
        public void GetCalculatedAttributesByScenarioIdNoChildren_SimulationInDbWithCalculatedAttribute_Gets()
        {
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            NetworkTestSetup.CreateNetwork(TestHelper.UnitOfWork);
            var simulation = SimulationTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var calculatedAttributeId = Guid.NewGuid();
            var calculatedAttribute = CalculatedAttributeTestSetup.TestCalculatedAttributeInScenarioInDb(
                TestHelper.UnitOfWork, simulation.Id, calculatedAttributeId, TestAttributeNames.CulvSeeded
                );

            var calculatedAttributes = TestHelper.UnitOfWork.CalculatedAttributeRepo.GetCalcuatedAttributesByScenarioIdNoChildren(simulation.Id);

            var actual = calculatedAttributes.Single();
            ObjectAssertions.EquivalentExcluding(calculatedAttribute, actual, ca => ca.Equations);
        }

        [Fact]
        public async Task GetLibraryAccess_LibraryInDbWithUserAccess_Gets()
        {
            TestHelper.UnitOfWork.ClearAttributeIdNameCache();
            var user = await UserTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            var library = CalculatedAttributeLibraryTestSetup.ModelForEntityInDb(TestHelper.UnitOfWork);
            TestHelper.UnitOfWork.CalculatedAttributeRepo.UpsertCalculatedAttributeLibrary(library);
            var libraryUserDto = new LibraryUserDTO
            {
                AccessLevel = LibraryAccessLevel.Modify,
                UserId = user.Id,
                UserName = user.Username,
            };
            var libraryUserDtos = new List<LibraryUserDTO> { libraryUserDto };
            TestHelper.UnitOfWork.CalculatedAttributeRepo.UpsertOrDeleteUsers(library.Id, libraryUserDtos);

            var libraries = TestHelper.UnitOfWork.CalculatedAttributeRepo.GetCalculatedAttributeLibrariesNoChildrenAccessibleToUser(user.Id);

            var foundLibrary = libraries.Single(l => l.Id == library.Id);
            ObjectAssertions.EquivalentExcluding(library, foundLibrary, l => l.Owner);

        }
    }
}
