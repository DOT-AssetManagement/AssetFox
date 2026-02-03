using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.UnitTestsCore.Tests.Repositories;
using AssetFox.Core.UnitTestsCore.TestUtils;

namespace AssetFox.Core.UnitTestsCore.Tests.Attributes
{
    public static class UnitTestsCoreAttributeTestSetup
    {
        public static void EnsureAttributeExists(AttributeDTO dto)
        {
            var existingAttribute = TestHelper.UnitOfWork.AttributeRepo.GetSingleByName(dto.Name);
            if (existingAttribute == null)
            {
                TestHelper.UnitOfWork.AttributeRepo.UpsertAttributes(dto);
            }
        }

        public static AttributeDTO ExcelAttributeForEntityInDb(BaseDataSourceDTO dataSourceDTO)
        {
            var attribute = AttributeTestSetup.NumericDto(dataSourceDTO, connectionType: Data.ConnectionType.EXCEL);
            EnsureAttributeExists(attribute);
            return attribute;
        }
    }
}
