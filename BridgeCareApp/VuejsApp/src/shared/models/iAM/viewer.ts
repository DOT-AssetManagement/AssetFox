export interface ViewerDataRequest {
    simulationId: string;
    page: number;
    pageSize: number;
    sortBy: string;
    sortDirection: string;
    columnFilters: Record<string, string>;
}

export interface PagedViewerResult {
    columns: ViewerColumn[];
    rows: any[][];
    totalRowCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
}

export interface ViewerColumn {
    key: string;
    header: string;
    format: string;
    sortable: boolean;
    filterable: boolean;
}

export interface ViewerMetadata {
    simulationId: string;
    simulationName: string;
    runId: number;
    years: number[];
    assetCount: number;
    availableAttributes: string[];
    hasOutput: boolean;
}

export const emptyViewerDataRequest: ViewerDataRequest = {
    simulationId: '',
    page: 1,
    pageSize: 50,
    sortBy: '',
    sortDirection: 'asc',
    columnFilters: {},
};

export const emptyPagedViewerResult: PagedViewerResult = {
    columns: [],
    rows: [],
    totalRowCount: 0,
    page: 1,
    pageSize: 50,
    totalPages: 0,
};

export const emptyViewerMetadata: ViewerMetadata = {
    simulationId: '',
    simulationName: '',
    runId: 0,
    years: [],
    assetCount: 0,
    availableAttributes: [],
    hasOutput: false,
};
