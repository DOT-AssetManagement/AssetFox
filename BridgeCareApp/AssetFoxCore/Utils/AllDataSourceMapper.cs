using System;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Abstract;
using AssetFoxCore.Models;

namespace AssetFoxCore.Utils
{
    public static class AllDataSourceMapper
    {
        private static ExcelDataSourceDTO ToExcel(AllDataSource allDataSource)
        {
            var returnValue = new ExcelDataSourceDTO
            {
                DateColumn = allDataSource.DateColumn,
                Name = allDataSource.Name,
                Id = allDataSource.Id,
                LocationColumn = allDataSource.LocationColumn,
            };
            return returnValue;
        }

        private static SQLDataSourceDTO ToSql(AllDataSource allDataSource)
        {
            var returnValue = new SQLDataSourceDTO
            {
                ConnectionString = allDataSource.ConnectionString,
                Id = allDataSource.Id,
                Name = allDataSource.Name,
            };
            return returnValue;
        }

        public static BaseDataSourceDTO ToSpecificDto(AllDataSource allDataSource)
        {
            BaseDataSourceDTO specificDto;
            switch (allDataSource.Type.ToLower())
            {
            case "excel":
                specificDto = ToExcel(allDataSource);
                break;
            case "sql":
                specificDto = ToSql(allDataSource);
                break;
            default:
                throw new NotImplementedException();
            }
            return specificDto;
        }
    }
}
