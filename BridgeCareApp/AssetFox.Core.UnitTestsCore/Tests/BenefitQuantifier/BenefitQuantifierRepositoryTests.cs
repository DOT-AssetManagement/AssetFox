using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Data.Networking;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;
using Xunit;

namespace AssetFox.Core.UnitTestsCore.Tests.BenefitQuantifier
{
    public class BenefitQuantifierRepositoryTests
    {
        [Fact]
        public void DeleteBenefitQuantifier_BenefitQuantifierInDbWithEquation_Deletes()
        {
            var networkId = Guid.NewGuid();
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var network = NetworkTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, new List<MaintainableAsset>(), networkId);
            var dto = BenefitQuantifierDtos.Dto(networkId);
            TestHelper.UnitOfWork.BenefitQuantifierRepo.UpsertBenefitQuantifier(dto);

            var benefitQuantifierInDb = TestHelper.UnitOfWork.Context.BenefitQuantifier
                .SingleOrDefault(bc => bc.NetworkId == networkId);
            Assert.NotNull(benefitQuantifierInDb);

            TestHelper.UnitOfWork.BenefitQuantifierRepo.DeleteBenefitQuantifier(networkId);

            var benefitQuantifierInDbAfter = TestHelper.UnitOfWork.Context.BenefitQuantifier
                .SingleOrDefault(bc => bc.NetworkId == networkId);
            Assert.Null(benefitQuantifierInDbAfter);
        }


        [Fact]
        public void DeleteBenefitQuantifier_BenefitQuantifierInDbWithoutEquation_Deletes()
        {
            var networkId = Guid.NewGuid();
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var network = NetworkTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, new List<MaintainableAsset>(), networkId);
            var dto = BenefitQuantifierDtos.Dto(networkId);
            TestHelper.UnitOfWork.BenefitQuantifierRepo.UpsertBenefitQuantifierNonAtomic(dto);

            var benefitQuantifierInDb = TestHelper.UnitOfWork.Context.BenefitQuantifier
                .SingleOrDefault(bc => bc.NetworkId == networkId);
            Assert.NotNull(benefitQuantifierInDb);
            var equationInDb = TestHelper.UnitOfWork.Context.Equation
                .SingleOrDefault(e => e.BenefitQuantifier.NetworkId == networkId);
            Assert.NotNull(equationInDb);
            TestHelper.UnitOfWork.Context.Equation.Remove(equationInDb);
            TestHelper.UnitOfWork.Context.SaveChanges();
            var equationInDbMid = TestHelper.UnitOfWork.Context.Equation
                .SingleOrDefault(e => e.BenefitQuantifier.NetworkId == networkId);
            Assert.Null(equationInDbMid);

            TestHelper.UnitOfWork.BenefitQuantifierRepo.DeleteBenefitQuantifier(networkId);

            var benefitQuantifierInDbAfter = TestHelper.UnitOfWork.Context.BenefitQuantifier
                .SingleOrDefault(bc => bc.NetworkId == networkId);
            Assert.Null(benefitQuantifierInDbAfter);
            var equationInDbAfter = TestHelper.UnitOfWork.Context.Equation
                 .SingleOrDefault(e => e.BenefitQuantifier.NetworkId == networkId);
            Assert.Null(equationInDbAfter);
        }

        [Fact]
        public async Task GetBenefitQuantifier_BenefitQuantifierInDb_Gets()
        {
            var networkId = Guid.NewGuid();
            AttributeTestSetup.CreateAttributes(TestHelper.UnitOfWork);
            var network = NetworkTestSetup.ModelForEntityInDb(
                TestHelper.UnitOfWork, new List<MaintainableAsset>(), networkId);
            var dto = BenefitQuantifierDtos.Dto(networkId);
            TestHelper.UnitOfWork.BenefitQuantifierRepo.UpsertBenefitQuantifierNonAtomic(dto);

            var benefitQuantifier = TestHelper.UnitOfWork.BenefitQuantifierRepo.GetBenefitQuantifier(networkId);

            Assert.Equivalent(dto, benefitQuantifier);
        }
    }
}
