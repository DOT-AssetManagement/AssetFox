import { DataSourceMappingData } from '@/shared/models/iAM/data-source';
import { getBlankGuid } from "@/shared/utils/uuid-utils";

export interface EditDataSourceMappingsDialogData {
    showDialog: boolean;
    dataSourceMappings: DataSourceMappingData[];
    columnSelectItems: string[];
    dataSourceId: string;
}

export const emptyEditDataSourceMappingsDialogData: EditDataSourceMappingsDialogData = {
    showDialog: false,
    dataSourceMappings: [],
    columnSelectItems: [],
    dataSourceId: getBlankGuid(),
};