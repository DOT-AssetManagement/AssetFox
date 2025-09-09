import { DataSourceMappingData } from '@/shared/models/iAM/data-source';

export interface EditDataSourceMappingsDialogData {
    showDialog: boolean;
    dataSourceMappings: DataSourceMappingData[];
    columnSelectItems: string[];
}

export const emptyEditDataSourceMappingsDialogData: EditDataSourceMappingsDialogData = {
    showDialog: false,
    dataSourceMappings: [],
    columnSelectItems: []
};