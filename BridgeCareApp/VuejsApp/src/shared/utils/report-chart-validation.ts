import {
    ChartConfig,
    ChartFieldBinding,
    ComponentType,
    ReportPage,
    isChartComponentType,
} from '@/shared/models/iAM/report-schema';

export interface ChartValidationIssue {
    componentId: string;
    pageIndex: number;
    message: string;
    blocker: boolean;
}

function hasBinding(binding?: ChartFieldBinding): boolean {
    return !!binding?.source && !!binding?.field && !!binding?.label;
}

function bindingLabel(binding?: ChartFieldBinding): string {
    return binding?.label || binding?.field || 'Selected field';
}

function compatibleWithValue(value: ChartFieldBinding, binding: ChartFieldBinding): boolean {
    if (value.source === 'allocation') {
        return ['allocation', 'simulationYear', 'treatment', 'assetDetail', 'attribute'].includes(binding.source);
    }

    if (value.source === 'assetDetail') {
        return ['simulationYear', 'assetDetail', 'attribute'].includes(binding.source);
    }

    if (value.source === 'attribute') {
        return ['simulationYear', 'assetDetail', 'attribute'].includes(binding.source);
    }

    return false;
}

export function validateChartConfig(
    componentId: string,
    componentType: ComponentType | string,
    config: ChartConfig,
    pageIndex = 0,
): ChartValidationIssue[] {
    const issues: ChartValidationIssue[] = [];
    const chartKind = config?.chartKind || (componentType === 'pieChart' ? 'pie' : componentType === 'lineChart' ? 'line' : 'bar');
    const category = config?.bindings?.category;
    const value = config?.bindings?.value;
    const series = config?.bindings?.series;

    function add(message: string, blocker = true) {
        issues.push({ componentId, pageIndex, message, blocker });
    }

    if (!hasBinding(category)) {
        add('Choose a category field.');
    }

    if (!hasBinding(value)) {
        add('Choose a value field.');
    } else if (value?.role !== 'measure') {
        add(`${bindingLabel(value)} must be used as a measure.`);
    }

    if (chartKind === 'pie' && hasBinding(series)) {
        add('Pie charts do not support a series split.');
    }

    if (chartKind === 'line' && category && category.format !== 'year') {
        add('Line charts work best with Year as the category.', false);
    }

    const topN = config?.limit?.topN;
    if (topN !== undefined && topN !== null) {
        const numericTopN = Number(topN);
        if (!Number.isInteger(numericTopN) || numericTopN < 1 || numericTopN > 100) {
            add('Top N must be between 1 and 100.');
        }
    }

    if (value && category && !compatibleWithValue(value, category)) {
        add(`${bindingLabel(category)} cannot be joined to ${bindingLabel(value)}.`);
    }

    if (value && series && !compatibleWithValue(value, series)) {
        add(`${bindingLabel(series)} cannot be joined to ${bindingLabel(value)}.`);
    }

    return issues;
}

export function validateReportChartComponents(pages: ReportPage[]): ChartValidationIssue[] {
    const issues: ChartValidationIssue[] = [];

    pages.forEach((page, pageIndex) => {
        (page.components || []).forEach((component) => {
            if (!isChartComponentType(component.type)) {
                return;
            }

            issues.push(...validateChartConfig(
                component.id,
                component.type,
                component.config as ChartConfig,
                pageIndex,
            ));
        });
    });

    return issues;
}

export function hasBlockingChartIssues(pages: ReportPage[]): boolean {
    return validateReportChartComponents(pages).some((issue) => issue.blocker);
}
