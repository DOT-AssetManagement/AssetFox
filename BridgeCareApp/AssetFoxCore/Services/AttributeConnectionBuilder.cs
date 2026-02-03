using System;
using System.Data;
using AssetFox.Core.Data;
using AssetFox.Core.Data.Attributes;
using AssetFox.Core.DTOs.Abstract;
using Attribute = AssetFox.Core.Data.Attributes.Attribute;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Data.ExcelDatabaseStorage.Serializers;
using AssetFoxCore.Models;

namespace AssetFoxCore.Services
{
    public static class AttributeConnectionBuilder
    {
        public static AttributeConnection Build(Attribute attribute, DataSourceMappingDTO dataSourceMapping, BaseDataSourceDTO dataSource, IUnitOfWork unitOfWork, ExcelRawDataDTO excelSpreadsheet = null)
        {
            if (dataSource is AllDataSource)
            {
                throw new InvalidOperationException("DataSource passed into AttributeConnection should not be an AllDataSource");
            }
            switch (dataSource.Type)
            {
            case "MSSQL":
                return new SqlAttributeConnection(attribute, dataSource);

            case "Excel":
                if (excelSpreadsheet == null)
                {
                    var warningMessage = $@"Found DataSource {dataSource.Name}. The DataSource was of type ""EXCEL"". However, we did not find an ExcelRawData for that data source.";
                    throw new RowNotInTableException(warningMessage);
                }
                var worksheet = ExcelRawDataSpreadsheetSerializer.Deserialize(excelSpreadsheet.SerializedWorksheetContent).Worksheet;
                return new ExcelAttributeConnection(attribute, dataSourceMapping, dataSource, worksheet);
            default:
                throw new InvalidOperationException($"Invalid Connection type \"{attribute.ConnectionType}\".");
            }
        }
    }
}
