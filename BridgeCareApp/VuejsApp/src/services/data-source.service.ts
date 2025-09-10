import {AxiosPromise} from 'axios';
import {API, coreAxiosInstance} from '@/shared/utils/axios-instance';
import {DataSourceMappingData, ExcelDataSource, SqlDataSource} from '@/shared/models/iAM/data-source';
import { TestStringData } from '@/shared/models/iAM/test-string';
import { Gunzip } from 'zlib';


export default class DataSourceService {
    static upsertDataSourceMappings(dataSourceMappings: DataSourceMappingData[], dataSourceId: string): AxiosPromise {
        return coreAxiosInstance.post(
            `${API.DataSourceMapping}/UpsertDataSourceMappings/${dataSourceId}`,
            dataSourceMappings,
        );
    }
    static getDataSourceTypes(): AxiosPromise {
        return coreAxiosInstance.get(`${API.DataSource}/GetDataSourceTypes`);
    }
    static getDataSources(): AxiosPromise {
        return coreAxiosInstance.get(`${API.DataSource}/GetDataSources`);
    }
    static upsertSqlDatasource(
        sqlDataSourceDTO: SqlDataSource,
    ): AxiosPromise {
        return coreAxiosInstance.post(
            `${API.DataSource}/UpsertSqlDataSource`,
            sqlDataSourceDTO,
        );
    }
    static upsertExcelDatasource(
        data: ExcelDataSource,
    ): AxiosPromise {
        return coreAxiosInstance.post(
            `${API.DataSource}/UpsertExcelDataSource/`,
            data
        );
    }
    static DeleteDataSource(
        id: string,
    ): AxiosPromise {
        return coreAxiosInstance.delete(
            `${API.DataSource}/DeleteDataSource/${id}`
        );
    }
    static getExcelSpreadsheetColumnHeaders(
        datasourceId: string
    ): AxiosPromise {
        return coreAxiosInstance.get(
            `${API.RawData}/GetExcelSpreadsheetColumnHeaders/${datasourceId}`
        );
    }
    static checkSqlConnection(
        data: TestStringData
    ): AxiosPromise {
        return coreAxiosInstance.post(`${API.DataSource}/CheckSqlConnection`, data);
    }
    static importExcelSpreadsheet(
        file: File,
        id: string,
    ) {
        let formData = new FormData();
        formData.append('file', file);
        return coreAxiosInstance.post(
            `${API.RawData}/ImportExcelSpreadsheet/${id}`,
            formData,
            {headers: {'Content-Type': 'multipart/form-data'}},
        );
    }
    static getDataSourceMappings(datasourceId: string
    ): AxiosPromise {
        return coreAxiosInstance.get(
            `${API.DataSourceMapping}/GetDataSourceMappings/${datasourceId}`
        );
    }
    static downloadDataSourceMappings(dataSourceId: string): AxiosPromise {
        return coreAxiosInstance.get(
            `${API.DataSourceMapping}/DownloadDataSourceMappings/${dataSourceId}`,
        );
    }
}