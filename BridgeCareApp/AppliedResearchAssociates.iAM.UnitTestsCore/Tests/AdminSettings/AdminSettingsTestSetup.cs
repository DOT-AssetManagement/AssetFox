using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppliedResearchAssociates.iAM.DataPersistenceCore.UnitOfWork;
using AppliedResearchAssociates.iAM.UnitTestsCore.TestUtils;

namespace AppliedResearchAssociates.iAM.UnitTestsCore.Tests
{
    public static class AdminSettingsTestSetup
    {
        public static void SetupBamsAdminSettings(IUnitOfWork unitOfWork, string networkName, string keyFields, string rawKeyFields)
        {
            unitOfWork.AdminSettingsRepo.SetPrimaryNetwork(networkName);
            unitOfWork.AdminSettingsRepo.SetRawDataNetwork(networkName);
            unitOfWork.AdminSettingsRepo.SetInventoryReports("BAMSInventoryLookup(P)");
            unitOfWork.AdminSettingsRepo.SetKeyFields(keyFields);
            unitOfWork.AdminSettingsRepo.SetRawDataKeyFields(rawKeyFields);
        }

        public static void SetupBamsAdminSettingsForTestNetwork(IUnitOfWork unitOfWork, bool alsoUseBmsId)
        {
            var networkName = NetworkTestSetup.TestNetworkName;
            var keyAttributeName1 = TestAttributeNames.BrKey;
            var keyAttributeName2 = TestAttributeNames.BmsId;
            var keyAttributeNames = alsoUseBmsId ?
                $"{keyAttributeName1},{keyAttributeName2}"
                : keyAttributeName1;
            SetupBamsAdminSettings(unitOfWork, networkName, keyAttributeNames, keyAttributeNames);
        }
    }
}

