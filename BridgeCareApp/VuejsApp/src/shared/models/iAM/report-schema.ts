import { getBlankGuid, getNewGuid } from '@/shared/utils/uuid-utils';
import { LibraryUser } from '@/shared/models/iAM/user';

// ============================================================
// Schema DTO (persisted to database)
// ============================================================

export interface ReportSchemaDTO {
    id: string;
    name: string;
    description: string;
    schemaJson: string;
    reportCategory: string;
    isSystemDefault: boolean;
    isSharedWithAll: boolean;
    createdByName: string;
    createdDate: string;
    modifiedDate: string;
    ownerName: string;
    currentUserAccessLevel: number | null;
    canModify: boolean;
    canShare: boolean;
    isShared: boolean;
    sharedUserCount: number;
    pageCount: number;
    componentCount: number;
    users: LibraryUser[];
}

export const emptyReportSchemaDTO: ReportSchemaDTO = {
    id: getBlankGuid(),
    name: '',
    description: '',
    schemaJson: '',
    reportCategory: '',
    isSystemDefault: false,
    isSharedWithAll: false,
    createdByName: '',
    createdDate: new Date().toISOString(),
    modifiedDate: new Date().toISOString(),
    ownerName: '',
    currentUserAccessLevel: null,
    canModify: false,
    canShare: false,
    isShared: false,
    sharedUserCount: 0,
    pageCount: 0,
    componentCount: 0,
    users: [],
};

// ============================================================
// Schema Definition (the JSON inside schemaJson)
// ============================================================

export interface ReportSchemaDefinition {
    schemaVersion: string;
    name: string;
    description: string;
    calculatedFields: ReportCalculatedField[];
    pages: ReportPage[];
}

export type ReportCalculatedFieldResultType = 'text' | 'number';
export type ReportCalculatedFieldMode = 'formula' | 'rules';

export interface ReportAttributeReference {
    attributeId: string;
    token: string;
    name: string;
}

export interface ReportExpressionDefinition {
    expression: string;
    attributeReferences: ReportAttributeReference[];
}

export interface ReportCalculationResult {
    value: string | null;
}

export type ReportRuleLogicalOperator = 'all' | 'any';
export type ReportRuleComparisonOperator =
    'equals' | 'notEquals' |
    'lessThan' | 'lessThanOrEqual' | 'greaterThan' | 'greaterThanOrEqual' |
    'contains' | 'notContains' | 'startsWith' | 'endsWith';

export interface ReportRuleCondition {
    id: string;
    attribute: ReportAttributeReference;
    operator: ReportRuleComparisonOperator | '';
    value: string | null;
}

export interface ReportCalculationRule {
    id: string;
    logicalOperator: ReportRuleLogicalOperator;
    conditions: ReportRuleCondition[];
    result: ReportCalculationResult;
}

export interface ReportCalculatedField {
    id: string;
    name: string;
    description: string;
    resultType: ReportCalculatedFieldResultType;
    mode: ReportCalculatedFieldMode;
    formula?: ReportExpressionDefinition;
    rules: ReportCalculationRule[];
    defaultResult?: ReportCalculationResult;
    missingResult?: ReportCalculationResult;
}

export interface ReportPage {
    id: string;
    name: string;
    order: number;
    components: ReportComponent[];
}

export interface ReportComponent {
    id: string;
    type: ComponentType;
    title: string;
    order: number;
    visible: boolean;
    criteriaExpression: string;
    pageSize: number;
    stripedRows: boolean;
    // Grid layout fields (12-column grid)
    gridX: number;      // column start (0-11)
    gridY: number;      // row position
    gridW: number;      // width in grid columns (1-12)
    gridH: number;      // height in grid rows
    config: ComponentConfig;
}

// ============================================================
// Component Types
// ============================================================

export type ComponentType =
    | 'assetDetail'
    | 'treatmentTimeline'
    | 'workSummary'
    | 'budgetSummary'
    | 'scenarioParameters'
    | 'conditionTracking'
    | 'fundedTreatmentList'
    | 'barChart'
    | 'pieChart'
    | 'lineChart'
    | 'textBlock';

// ============================================================
// Component Config Types
// ============================================================

export interface AssetDetailConfig {
    timeScope: 'initial' | 'year' | 'allYears';
    selectedYear?: number;
    attributeColumns: ColumnDef[];
    sorting: SortingConfig;
    showTotals: boolean;
}

export interface TreatmentTimelineConfig {
    attributeColumns: ColumnDef[];
    yearDataFields: YearField[];
    sorting: SortingConfig;
    showNoTreatment: boolean;
}

export interface WorkSummaryConfig {
    groupByAttribute: string;
    aggregateField: string;
    aggregateFunction: AggregateFunction;
    includePercentages: boolean;
}

export interface BudgetSummaryConfig {
    showSpentAmount: boolean;
    showRemainingAmount: boolean;
}

export type ScenarioParametersSectionKey =
    | 'scenario'
    | 'investment'
    | 'analysis'
    | 'scenarioCriteria'
    | 'priorities'
    | 'cashFlow'
    | 'funding'
    | 'budgetCriteria'
    | 'treatments';

export interface ScenarioParametersConfig {
    sections: ScenarioParametersSectionKey[];
}

export interface ConditionTrackingConfig {
    conditionAttributes: string[];
    conditionFields?: ColumnDef[];
    attributeColumns: ColumnDef[];
    showInitialState: boolean;
}

export interface FundedTreatmentListConfig {
    columns: ColumnDef[];
    simulationColumns: SimulationColumnDef[];
    sorting: SortingConfig;
}

export interface TextBlockConfig {
    content: string;
    style: 'heading' | 'subheading' | 'body' | 'note';
}

export type ChartKind = 'bar' | 'pie' | 'line';
export type ChartRole = 'dimension' | 'measure' | 'series' | 'tooltip';
export type ChartAggregation = 'sum' | 'avg' | 'count' | 'min' | 'max';

export interface ChartFieldBinding {
    source: string;
    field: string;
    label: string;
    role: ChartRole;
    aggregate?: ChartAggregation;
    format: ColumnFormat | 'year';
}

export interface ChartYearScope {
    mode: 'allYears' | 'selectedYear' | 'range';
    selectedYear?: number;
    startYear?: number;
    endYear?: number;
}

export interface ChartLimit {
    topN?: number;
    includeOther: boolean;
}

export interface ChartFilterOptions {
    excludeNoTreatment: boolean;
}

export interface ChartDisplayOptions {
    showLegend: boolean;
    legendPosition: 'right' | 'bottom';
    showLabels: boolean;
    showGridLines: boolean;
    stacked: boolean;
    horizontal: boolean;
    showMarkers: boolean;
    donut: boolean;
}

export interface ChartConfig {
    chartKind: ChartKind;
    mode: string;
    bindings: {
        category?: ChartFieldBinding;
        value?: ChartFieldBinding;
        series?: ChartFieldBinding;
        tooltip?: ChartFieldBinding[];
    };
    yearScope: ChartYearScope;
    limit: ChartLimit;
    filters: ChartFilterOptions;
    display: ChartDisplayOptions;
}

export type ComponentConfig =
    | AssetDetailConfig
    | TreatmentTimelineConfig
    | WorkSummaryConfig
    | BudgetSummaryConfig
    | ScenarioParametersConfig
    | ConditionTrackingConfig
    | FundedTreatmentListConfig
    | ChartConfig
    | TextBlockConfig;

// ============================================================
// Shared Types
// ============================================================

export interface ColumnDef {
    source?: 'attribute' | 'calculatedField';
    fieldId?: string;
    attributeName: string;
    displayHeader: string;
    format: ColumnFormat;
    width?: number;
}

export interface ReportAttributeOption {
    source: 'attribute' | 'calculatedField';
    fieldId: string;
    name: string;
    type: string;
}

export interface SimulationColumnDef {
    field: SimulationField;
    displayHeader: string;
    format: ColumnFormat;
}

export interface YearField {
    type: 'treatment' | 'cost' | 'attribute' | 'calculatedField';
    attributeName?: string;
    fieldId?: string;
    displayHeader: string;
    format: ColumnFormat;
}

export interface SortingConfig {
    field: string;
    direction: 'asc' | 'desc';
}

export type ColumnFormat = 'text' | 'number' | 'currency' | 'percent';
export type AggregateFunction = 'sum' | 'avg' | 'count' | 'min' | 'max';
export type SimulationField =
    | 'year'
    | 'appliedTreatment'
    | 'treatmentCause'
    | 'treatmentStatus'
    | 'cost'
    | 'budget';

// ============================================================
// Defaults
// ============================================================

export const emptySortingConfig: SortingConfig = {
    field: '',
    direction: 'asc',
};

export const emptyColumnDef: ColumnDef = {
    attributeName: '',
    displayHeader: '',
    format: 'text',
    width: 12,
};

export const emptySimulationColumnDef: SimulationColumnDef = {
    field: 'year',
    displayHeader: '',
    format: 'text',
};

export const emptyYearField: YearField = {
    type: 'treatment',
    displayHeader: 'Treatment',
    format: 'text',
};

export const emptyReportSchemaDefinition: ReportSchemaDefinition = {
    schemaVersion: '1.1',
    name: '',
    description: '',
    calculatedFields: [],
    pages: [createDefaultPage()],
};

export function createEmptyReportCalculatedField(): ReportCalculatedField {
    return {
        id: getNewGuid(),
        name: '',
        description: '',
        resultType: 'text',
        mode: 'rules',
        formula: {
            expression: '',
            attributeReferences: [],
        },
        rules: [{
            id: getNewGuid(),
            logicalOperator: 'all',
            conditions: [createEmptyReportRuleCondition()],
            result: { value: null },
        }],
        defaultResult: { value: null },
        missingResult: { value: null },
    };
}

export function createEmptyReportRuleCondition(): ReportRuleCondition {
    return {
        id: getNewGuid(),
        attribute: {
            attributeId: '',
            token: '',
            name: '',
        },
        operator: '',
        value: null,
    };
}

export function reportColumnKey(column: Pick<ColumnDef, 'source' | 'fieldId' | 'attributeName'>): string {
    return column.source === 'calculatedField'
        ? `calculatedField:${column.fieldId || ''}`
        : `attribute:${column.fieldId || column.attributeName}`;
}

export function reportFieldOptionKey(field: ReportAttributeOption): string {
    return `${field.source}:${field.fieldId}`;
}

export function columnDefFromReportField(field: ReportAttributeOption): ColumnDef {
    return {
        source: field.source,
        fieldId: field.fieldId,
        attributeName: field.name,
        displayHeader: field.name,
        format: field.type === 'NUMBER' ? 'number' : 'text',
        width: 12,
    };
}

// ============================================================
// Default configs per component type
// ============================================================

export function createDefaultConfig(type: ComponentType): ComponentConfig {
    switch (type) {
        case 'assetDetail':
            return {
                timeScope: 'initial',
                attributeColumns: [],
                sorting: { ...emptySortingConfig },
                showTotals: false,
            } as AssetDetailConfig;
        case 'treatmentTimeline':
            return {
                attributeColumns: [],
                yearDataFields: [
                    { type: 'treatment', displayHeader: 'Treatment', format: 'text' },
                    { type: 'cost', displayHeader: 'Cost', format: 'currency' },
                ],
                sorting: { ...emptySortingConfig },
                showNoTreatment: false,
            } as TreatmentTimelineConfig;
        case 'workSummary':
            return {
                groupByAttribute: '',
                aggregateField: 'cost',
                aggregateFunction: 'sum',
                includePercentages: false,
            } as WorkSummaryConfig;
        case 'budgetSummary':
            return {
                showSpentAmount: true,
                showRemainingAmount: true,
            } as BudgetSummaryConfig;
        case 'scenarioParameters':
            return {
                sections: scenarioParametersSectionOptions.map(section => section.value),
            } as ScenarioParametersConfig;
        case 'conditionTracking':
            return {
                conditionAttributes: [],
                conditionFields: [],
                attributeColumns: [],
                showInitialState: true,
            } as ConditionTrackingConfig;
        case 'fundedTreatmentList':
            return {
                columns: [],
                simulationColumns: [
                    { field: 'year', displayHeader: 'Year', format: 'number' },
                    { field: 'appliedTreatment', displayHeader: 'Treatment', format: 'text' },
                    { field: 'cost', displayHeader: 'Cost', format: 'currency' },
                ],
                sorting: { ...emptySortingConfig },
            } as FundedTreatmentListConfig;
        case 'barChart':
            return createDefaultChartConfig('bar');
        case 'pieChart':
            return createDefaultChartConfig('pie');
        case 'lineChart':
            return createDefaultChartConfig('line');
        case 'textBlock':
            return {
                content: '',
                style: 'body',
            } as TextBlockConfig;
    }
}

export const standardChartColors = ['#2A578D', '#2F6F4E', '#C77700', '#7A4E9D', '#2F6F7E', '#B23A48', '#5B667A', '#8A6A00'];

export function createDefaultChartConfig(kind: ChartKind): ChartConfig {
    return {
        chartKind: kind,
        mode: kind === 'bar' ? 'single' : kind === 'pie' ? 'pie' : 'line',
        bindings: {},
        yearScope: {
            mode: 'allYears',
        },
        limit: {
            topN: kind === 'pie' ? 8 : undefined,
            includeOther: kind === 'pie',
        },
        filters: {
            excludeNoTreatment: false,
        },
        display: {
            showLegend: true,
            legendPosition: 'right',
            showLabels: kind === 'pie',
            showGridLines: kind !== 'pie',
            stacked: false,
            horizontal: false,
            showMarkers: kind === 'line',
            donut: false,
        },
    };
}

export function pageSizeToGridH(pageSize: number): number {
    if (pageSize <= 10) {
        return 2;
    }
    if (pageSize <= 25) {
        return 3;
    }
    if (pageSize <= 50) {
        return 4;
    }
    return 5;
}

export function createDefaultComponent(type: ComponentType): ReportComponent {
    const pageSize = 10;
    const isChart = isChartComponentType(type);
    return {
        id: getNewGuid(),
        type,
        title: componentTypeInfo[type].label,
        order: 0,
        visible: true,
        criteriaExpression: '',
        pageSize,
        stripedRows: false,
        gridX: 0,
        gridY: 0,
        gridW: 12,
        gridH: isChart ? 4 : pageSizeToGridH(pageSize),
        config: createDefaultConfig(type),
    };
}

export function createDefaultPage(name?: string): ReportPage {
    return {
        id: getNewGuid(),
        name: name || 'Page 1',
        order: 0,
        components: [],
    };
}

export const pageSizeOptions = [
    { title: '10', value: 10 },
    { title: '25', value: 25 },
    { title: '50', value: 50 },
    { title: '100', value: 100 },
];

// ============================================================
// Component Type Metadata (for palette, chips, colors)
// ============================================================

export interface ComponentTypeInfo {
    label: string;
    description: string;
    icon: string;
    color: string;
    chipColor: string;
}

export const componentTypeInfo: Record<ComponentType, ComponentTypeInfo> = {
    assetDetail: {
        label: 'Asset Detail Table',
        description: 'Flat table of assets with selected attribute columns',
        icon: 'fas fa-table',
        color: '#1565C0',
        chipColor: '#E3F2FD',
    },
    treatmentTimeline: {
        label: 'Treatment Timeline',
        description: 'Per-asset treatment and condition history across years',
        icon: 'fas fa-stream',
        color: '#2E7D32',
        chipColor: '#E8F5E9',
    },
    workSummary: {
        label: 'Work Summary',
        description: 'Aggregated work counts and costs by category and year',
        icon: 'fas fa-chart-bar',
        color: '#E65100',
        chipColor: '#FFF3E0',
    },
    budgetSummary: {
        label: 'Budget Summary',
        description: 'Budget utilization across simulation years',
        icon: 'fas fa-dollar-sign',
        color: '#6A1B9A',
        chipColor: '#F3E5F5',
    },
    scenarioParameters: {
        label: 'Scenario Parameters',
        description: 'Scenario settings, criteria, funding, and treatments',
        icon: 'fas fa-sliders-h',
        color: '#455A64',
        chipColor: '#ECEFF1',
    },
    conditionTracking: {
        label: 'Condition Tracking',
        description: 'Track condition attribute changes over the simulation',
        icon: 'fas fa-heartbeat',
        color: '#00838F',
        chipColor: '#E0F7FA',
    },
    fundedTreatmentList: {
        label: 'Funded Treatment List',
        description: 'Flat list of all funded treatment events',
        icon: 'fas fa-list-alt',
        color: '#BF360C',
        chipColor: '#FBE9E7',
    },
    barChart: {
        label: 'Bar Chart',
        description: 'Compare values across categories or years',
        icon: 'fas fa-chart-bar',
        color: '#1565C0',
        chipColor: '#E3F2FD',
    },
    pieChart: {
        label: 'Pie Chart',
        description: 'Show a part-to-whole breakdown',
        icon: 'fas fa-chart-pie',
        color: '#7A4E9D',
        chipColor: '#F3E5F5',
    },
    lineChart: {
        label: 'Line Chart',
        description: 'Track values over simulation years',
        icon: 'fas fa-chart-line',
        color: '#2F6F4E',
        chipColor: '#E8F5E9',
    },
    textBlock: {
        label: 'Text Block',
        description: 'Free-text annotation or heading',
        icon: 'fas fa-align-left',
        color: '#616161',
        chipColor: '#FAFAFA',
    },
};

export function isChartComponentType(type: ComponentType | string): boolean {
    return type === 'barChart' || type === 'pieChart' || type === 'lineChart';
}

export const scenarioParametersSectionOptions: { title: string; value: ScenarioParametersSectionKey }[] = [
    { title: 'Scenario', value: 'scenario' },
    { title: 'Investment', value: 'investment' },
    { title: 'Analysis', value: 'analysis' },
    { title: 'Scenario Criteria', value: 'scenarioCriteria' },
    { title: 'Analysis Priorities', value: 'priorities' },
    { title: 'Cash Flow Rules', value: 'cashFlow' },
    { title: 'Funding by Year', value: 'funding' },
    { title: 'Budget Criteria', value: 'budgetCriteria' },
    { title: 'Treatments', value: 'treatments' },
];

// ============================================================
// Simulation Fields Metadata
// ============================================================

export interface SimulationFieldInfo {
    field: SimulationField;
    label: string;
    defaultFormat: ColumnFormat;
}

export const simulationFieldOptions: SimulationFieldInfo[] = [
    { field: 'year', label: 'Year', defaultFormat: 'number' },
    { field: 'appliedTreatment', label: 'Treatment Applied', defaultFormat: 'text' },
    { field: 'cost', label: 'Treatment Cost', defaultFormat: 'currency' },
    { field: 'budget', label: 'Budget', defaultFormat: 'text' },
    { field: 'treatmentCause', label: 'Treatment Cause', defaultFormat: 'text' },
    { field: 'treatmentStatus', label: 'Treatment Status', defaultFormat: 'text' },
];

// ============================================================
// Format & Aggregate Options
// ============================================================

export const formatOptions = [
    { title: 'Text', value: 'text' as ColumnFormat },
    { title: 'Number', value: 'number' as ColumnFormat },
    { title: 'Currency', value: 'currency' as ColumnFormat },
    { title: 'Percent', value: 'percent' as ColumnFormat },
];

export const aggregateFunctionOptions = [
    { title: 'Sum', value: 'sum' as AggregateFunction },
    { title: 'Average', value: 'avg' as AggregateFunction },
    { title: 'Minimum', value: 'min' as AggregateFunction },
    { title: 'Maximum', value: 'max' as AggregateFunction },
    { title: 'Count', value: 'count' as AggregateFunction },
];

export const timeScopeOptions = [
    { title: 'Initial State', value: 'initial' },
    { title: 'Specific Year', value: 'year' },
    { title: 'All Simulation Years', value: 'allYears' },
];

export const textBlockStyleOptions = [
    { title: 'Heading', value: 'heading' },
    { title: 'Subheading', value: 'subheading' },
    { title: 'Body', value: 'body' },
    { title: 'Note', value: 'note' },
];
