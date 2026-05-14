import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { clone, reject, propEq } from 'ramda';
import DataSourceService from '@/services/data-source.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import {
    Datasource,
    DataSourceMappingData,
    ExcelDataSource,
    RawDataColumns,
    SqlCommandResponse,
    SqlDataSource,
    emptySqlCommandResponse,
    noneDatasource,
} from '@/shared/models/iAM/data-source';
import { TestStringData } from '@/shared/models/iAM/test-string';

const emptyRawDataColumns: RawDataColumns = {
    columnHeaders: [],
    warningMessage: '',
};

export interface DataSourceStoreState {
    dataSources: Datasource[];
    dataSourceTypes: string[];
    excelColumns: RawDataColumns;
    isSuccessfulImport: boolean;
    sqlCommandResponse: SqlCommandResponse;
    dataSourceMappings: DataSourceMappingData[];
}

function withNoneDatasource(dataSources: Datasource[]): Datasource[] {
    const clonedDataSources = clone(dataSources);
    clonedDataSources.push(noneDatasource);
    return clonedDataSources;
}

export const useDataSourceStore = defineStore('dataSource', {
    state: (): DataSourceStoreState => ({
        dataSources: [],
        dataSourceTypes: [],
        excelColumns: emptyRawDataColumns,
        isSuccessfulImport: false,
        sqlCommandResponse: emptySqlCommandResponse,
        dataSourceMappings: [],
    }),
    actions: {
        async getDataSources() {
            await DataSourceService.getDataSources().then(
                (response: AxiosResponse<Datasource[]>) => {
                    if (hasValue(response, 'data')) {
                        this.dataSources = withNoneDatasource(response.data);
                    }
                },
            );
        },
        async getDataSourceTypes() {
            await DataSourceService.getDataSourceTypes().then(
                (response: AxiosResponse<string[]>) => {
                    if (hasValue(response, 'data')) {
                        this.dataSourceTypes = clone(response.data);
                    }
                },
            );
        },
        async upsertSqlDataSource(payload: SqlDataSource) {
            const notificationStore = useNotificationStore();
            await DataSourceService.upsertSqlDatasource(payload).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        notificationStore.addSuccessNotification({
                            message: 'Modified data sources',
                        });
                    }
                },
            );
        },
        async upsertExcelDataSource(payload: ExcelDataSource) {
            const notificationStore = useNotificationStore();
            await DataSourceService.upsertExcelDatasource(payload).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        notificationStore.addSuccessNotification({
                            message: 'Modified data sources',
                        });
                    }
                },
            );
        },
        async deleteDataSource(payload: string) {
            const notificationStore = useNotificationStore();
            await DataSourceService.DeleteDataSource(payload).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        notificationStore.addSuccessNotification({
                            message: 'Deleted data sources',
                        });
                        this.dataSources = withNoneDatasource(
                            reject(propEq('id', payload), this.dataSources) as Datasource[],
                        );
                    }
                },
            );
        },
        async checkSqlCommand(payload: TestStringData) {
            await DataSourceService.checkSqlConnection(payload).then(
                (response: AxiosResponse<SqlCommandResponse>) => {
                    if (hasValue(response, 'status')) {
                        this.sqlCommandResponse = response.data;
                    }
                },
            );
        },
        async getExcelSpreadsheetColumnHeaders(payload: string) {
            await DataSourceService.getExcelSpreadsheetColumnHeaders(payload).then(
                (response: AxiosResponse<RawDataColumns>) => {
                    if (hasValue(response, 'status')) {
                        this.excelColumns = clone(response.data);
                    }
                },
            );
        },
        async importExcelSpreadsheetFile(file: File, id: string) {
            const notificationStore = useNotificationStore();
            await DataSourceService.importExcelSpreadsheet(
                file,
                id,
            ).then((response: AxiosResponse) => {
                if (hasValue(response, 'data')) {
                    this.isSuccessfulImport = true;
                    notificationStore.addSuccessNotification({
                        message: 'Excel file imported',
                    });
                }
            });
        },
        async getDataSourceMappings(payload: string) {
            await DataSourceService.getDataSourceMappings(payload).then(
                (response: AxiosResponse<DataSourceMappingData[]>) => {
                    if (hasValue(response, 'status')) {
                        this.dataSourceMappings = clone(response.data);
                    }
                },
            );
        },
    },
    getters: {
        dataSourcesGetter: (state) => state.dataSources,
        dataSourceTypesGetter: (state) => state.dataSourceTypes,
        getExcelColumns: (state) => state.excelColumns,
        getIsSuccessfulImport: (state) => state.isSuccessfulImport,
        getSqlCommandResponse: (state) => state.sqlCommandResponse,
        dataSourceMappingsGetter: (state) => state.dataSourceMappings,
    },
});
