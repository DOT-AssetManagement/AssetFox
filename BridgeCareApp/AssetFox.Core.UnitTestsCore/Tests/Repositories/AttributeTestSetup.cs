using System;
using System.Linq;
using AssetFox.Core.Data;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.Data.Mappers;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataUnitTests;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.TestHelpers;
using AssetFox.Core.UnitTestsCore.Tests.Attributes;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using AttributesTextAttribute = AssetFox.Core.Data.Attributes.TextAttribute;
using System.Collections.Generic;
using AssetFox.Core.UnitTestsCore.Tests.DataSources;
using AssetFox.Core.Analysis;

namespace AssetFox.Core.UnitTestsCore.Tests.Repositories
{
    public static class AttributeTestSetup
    {
        public static string ValidAttributeName() => "A" + RandomStrings.Length11();

        public static NumericAttribute Numeric(Guid? id = null, string name = null, Guid? dataSourceId = null, ConnectionType connectionType = ConnectionType.MSSQL)
        {
            var resolvedId = id ?? Guid.NewGuid();
            var randomName = name ?? ValidAttributeName();
            var attribute = new NumericAttribute(2, 3, 1, resolvedId, randomName, "AVERAGE", "Command", connectionType, "connectionString", false, false, dataSourceId);
            return attribute;
        }

        public static AttributeDTO NumericDto(BaseDataSourceDTO dataSourceDTO, Guid? id = null, string name = null, ConnectionType connectionType = ConnectionType.MSSQL)
        {
            var attribute = Numeric(id, name, dataSourceDTO.Id, connectionType);
            var dto = AttributeDtoDomainMapper.ToDto(attribute, dataSourceDTO);
            return dto;
        }

        public static AttributesTextAttribute Text(Guid? id = null, string name = null, ConnectionType connectionType = ConnectionType.MSSQL, bool calculated = false)
        {
            var resolvedId = id ?? Guid.NewGuid();
            var randomName = name ?? ValidAttributeName();
            var attribute = new AttributesTextAttribute("defaultValue", resolvedId, randomName, "PREDOMINANT", "command", connectionType, "connectionString", calculated, true, Guid.Empty);
            return attribute;
        }


        private static bool AttributesHaveBeenCreated = false;
        private static readonly object AttributeLock = new object();
        private static ExcelDataSourceDTO CacheDataSourceForAttributes { get; set; }

        public static ExcelDataSourceDTO CreateAttributes(IUnitOfWork unitOfWork)
        {
            if (!AttributesHaveBeenCreated)
            {
                lock (AttributeLock)  // Necessary as long as there is a chance that some tests may run in paralell. Can we eliminate that possiblity?
                {
                    if (!AttributesHaveBeenCreated)
                    {
                        var config = TestConfiguration.Get();
                        ExcelDataSourceDTO dataSourceToApply = null;
                        if (!unitOfWork.DataSourceRepo.GetDataSources().Any(_ => _.Type == "EXCEL"))
                        {
                            dataSourceToApply = new ExcelDataSourceDTO
                            {
                                Id = Guid.NewGuid(),
                                Name = "Test Excel DataSource",
                                LocationColumn = TestAttributeNames.BrKey,
                                DateColumn = "Date",
                            };
                            unitOfWork.DataSourceRepo.UpsertDatasource(dataSourceToApply);
                        }
                        else
                        {
                            dataSourceToApply = (ExcelDataSourceDTO)unitOfWork.DataSourceRepo.GetDataSources().First(_ => _.Type == "Excel");
                        }
                        CacheDataSourceForAttributes = dataSourceToApply;
                        var attributesToInsert = AttributeDtoLists.AttributeSetupDtos();
                        foreach (var attribute in attributesToInsert)
                        {
                            attribute.DataSource = dataSourceToApply;
                        }
                        unitOfWork.AttributeRepo.UpsertAttributes(attributesToInsert);
                        AttributesHaveBeenCreated = true;
                    }
                }
            }
            return CacheDataSourceForAttributes;
        }

        private static BaseDataSourceDTO GetDataSource(ConnectionType connectionType, string locationColumn = null)
        {
            switch (connectionType)
            {
            case ConnectionType.MSSQL:
                return DataSourceDtos.TestConfigurationSql();
            case ConnectionType.EXCEL:
                return DataSourceDtos.TestConfigurationExcel(locationColumn);
            default:
                throw new NotImplementedException($"Not implemented for {connectionType}");
            }
        }

        public static AttributeDTO CreateSingleTextAttribute(
            IUnitOfWork unitOfWork,
            Guid? id = null,
            string name = null,
            ConnectionType connectionType = ConnectionType.MSSQL,
            string locationColumnIfExcel = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var resolveName = name ?? RandomStrings.WithPrefix("attribute");

            var dataSource = GetDataSource(connectionType, locationColumnIfExcel);
            unitOfWork.DataSourceRepo.UpsertDatasource(dataSource);
            var attribute = AttributeDtos.Text(resolveName, resolveId);
            attribute.DataSource = dataSource;
            var attributes = new List<AttributeDTO> { attribute };
            unitOfWork.AttributeRepo.UpsertAttributes(attributes);
            return attribute;
        }
        public static AttributeDTO CreateSingleNumericAttribute(
            IUnitOfWork unitOfWork,
            Guid? id = null,
            string name = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var resolveName = name ?? RandomStrings.WithPrefix("attribute");
            var dataSource = DataSourceDtos.TestConfigurationSql();
            unitOfWork.DataSourceRepo.UpsertDatasource(dataSource);
            var attribute = AttributeDtos.Numeric(resolveName, resolveId);
            attribute.DataSource = dataSource;
            var attributes = new List<AttributeDTO> { attribute };
            unitOfWork.AttributeRepo.UpsertAttributes(attributes);
            return attribute;
        }
    }
}
