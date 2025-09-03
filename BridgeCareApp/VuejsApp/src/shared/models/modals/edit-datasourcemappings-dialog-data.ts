import { DataSourceMapping } from '@/shared/models/iAM/data-source';

export interface EditDataSourceMappingsDialogData {
    showDialog: boolean;
    dataSourceMappings: DataSourceMapping[];
}

export const emptyEditDataSourceMappingsDialogData: EditDataSourceMappingsDialogData = {
    showDialog: false,
    dataSourceMappings: [],
};