using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IDataSourceRepository
    {
        List<BaseDataSourceDTO> GetDataSources();

        BaseDataSourceDTO GetDataSource(Guid id);

        void UpsertDatasource(BaseDataSourceDTO dataSource);

        void DeleteDataSource(Guid id);

        Dictionary<string, string> GetRawData(Dictionary<AttributeDTO, string> dictionary);
    }
}
