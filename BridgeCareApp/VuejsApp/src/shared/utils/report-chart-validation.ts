import {
    ChartConfig,
    ChartFieldBinding,
    ComponentType,
    ReportPage,
    isChartComponentType,
} from '@/shared/models/iAM/report-schema';
import {
    isValidChartColor,
    maxStyledChartValues,
    normalizeChartValue,
    supportedChartValueOrders,
} from '@/shared/utils/report-chart-styling';

export interface ChartValidationIssue {
    componentId: string;
    pageIndex: number;
    message: string;
    blocker: boolean;
}

const supportedAggregations = ['sum', 'avg', 'weightedAvg', 'min', 'max', 'count'];

function hasBinding(binding?: ChartFieldBinding): boolean {
    return !!binding?.source && !!binding?.field && !!binding?.label;
}

function bindingLabel(binding?: ChartFieldBinding): string {
    return binding?.label || binding?.field || 'Selected field';
}

function compatibleWithValue(value: ChartFieldBinding, binding: ChartFieldBinding): boolean {
    if (value.source === 'allocation') {
        return ['allocation', 'simulationYear', 'treatment', 'assetDetail', 'attribute', 'calculatedField'].includes(binding.source);
    }

    if (value.source === 'assetDetail') {
        return ['simulationYear', 'assetDetail', 'attribute', 'calculatedField'].includes(binding.source);
    }

    if (value.source === 'attribute') {
        return ['simulationYear', 'assetDetail', 'attribute', 'calculatedField'].includes(binding.source);
    }

    if (value.source === 'calculatedField') {
        return ['simulationYear', 'assetDetail', 'attribute', 'calculatedField'].includes(binding.source);
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

    if (value?.aggregate && !supportedAggregations.includes(value.aggregate)) {
        add(`${bindingLabel(value)} uses an unsupported aggregation.`);
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

    const styling = config?.valueStyling;
    if (styling) {
        const values = styling.values || [];
        if (!supportedChartValueOrders.includes(styling.order)) {
            add('Colors and order uses an unsupported order.');
        }
        if (values.length > maxStyledChartValues) {
            add(`Colors and order can list at most ${maxStyledChartValues} values.`);
        }
        if (values.some(style => !style.value?.trim())) {
            add('Colors and order has a blank value.');
        }
        const seen = new Set<string>();
        const duplicate = values.find(style => {
            const key = normalizeChartValue(style.value);
            if (!key) return false;
            if (seen.has(key)) return true;
            seen.add(key);
            return false;
        });
        if (duplicate) {
            add(`Colors and order lists "${duplicate.value}" more than once.`);
        }
        const invalidColor = values.find(style => !isValidChartColor(style.color));
        if (invalidColor) {
            add(`"${invalidColor.value}" has an invalid color.`);
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
