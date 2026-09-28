import {
    ChartConfig,
    ChartFieldBinding,
    ChartValueOrder,
    ChartValueStyling,
    ReportCalculatedField,
    standardChartColors,
} from '@/shared/models/iAM/report-schema';

// Keep in sync with ChartValueStyler.cs, which applies the same rules to the Excel export.

export const maxStyledChartValues = 30;
export const noneChartValue = '(None)';
// "(None)" is missing data, so the builder gives it a neutral color rather than a place on a color ramp.
export const noneChartColor = '#9E9E9E';
export const supportedChartValueOrders: ChartValueOrder[] = ['listed', 'alphabetical', 'alphabeticalDesc', 'valueDesc'];

export type ChartStyledRole = 'series' | 'category';

export interface ChartColorPreset {
    id: string;
    label: string;
    stops: string[];
}

export const chartColorPresets: ChartColorPreset[] = [
    { id: 'goodToPoor', label: 'Good to poor', stops: ['#2E7D32', '#7CB342', '#F9A825', '#EF6C00', '#C62828'] },
    { id: 'poorToGood', label: 'Poor to good', stops: ['#C62828', '#EF6C00', '#F9A825', '#7CB342', '#2E7D32'] },
    { id: 'sequentialBlue', label: 'Light to dark blue', stops: ['#C6DBEF', '#9ECAE1', '#6BAED6', '#3182BD', '#08519C'] },
    { id: 'standard', label: 'Standard palette', stops: standardChartColors },
];

export function chartBindingKey(binding?: ChartFieldBinding): string {
    return binding?.source && binding?.field ? `${binding.source}:${binding.field}` : '';
}

export function chartStyledRole(config: ChartConfig): ChartStyledRole | null {
    if (config?.bindings?.series?.field) return 'series';
    if (config?.bindings?.category?.field) return 'category';
    return null;
}

export function chartStyledBinding(config: ChartConfig): ChartFieldBinding | undefined {
    const role = chartStyledRole(config);
    return role ? config.bindings[role] : undefined;
}

/** The chart's styling, or undefined when there is none or it was set up for a different field. */
export function activeChartValueStyling(config: ChartConfig): ChartValueStyling | undefined {
    const styling = config?.valueStyling;
    const key = chartBindingKey(chartStyledBinding(config));
    return styling && key && styling.bindingKey === key ? styling : undefined;
}

export function normalizeChartValue(value: string): string {
    return (value ?? '').trim().toLowerCase();
}

export function isNoneChartValue(value: string): boolean {
    return normalizeChartValue(value) === normalizeChartValue(noneChartValue);
}

/** Natural, case-insensitive comparison ("2" before "10"), with "(None)" after everything else. */
export function compareChartValues(a: string, b: string): number {
    const aNone = isNoneChartValue(a);
    const bNone = isNoneChartValue(b);
    if (aNone !== bNone) return aNone ? 1 : -1;

    const aParts = splitNatural(normalizeChartValue(a));
    const bParts = splitNatural(normalizeChartValue(b));
    for (let i = 0; i < Math.min(aParts.length, bParts.length); i++) {
        const result = compareNaturalPart(aParts[i], bParts[i]);
        if (result !== 0) return result;
    }
    return aParts.length - bParts.length;
}

/** Orders the values present in the chart. Listed values come first (in list order) for the 'listed' order. */
export function orderChartValues(
    values: string[],
    styling: ChartValueStyling | undefined,
    totals?: Map<string, number>,
): string[] {
    if (!styling) return values;

    const byName = [...values].sort(compareChartValues);
    switch (styling.order) {
        case 'alphabetical':
            return byName;
        case 'alphabeticalDesc':
            return byName.reverse().sort((a, b) => noneLast(a, b));
        case 'valueDesc':
            return byName.sort((a, b) => (totals?.get(b) ?? 0) - (totals?.get(a) ?? 0));
        default: {
            const rank = new Map(styling.values.map((style, index) => [normalizeChartValue(style.value), index]));
            const listed = values
                .filter(value => rank.has(normalizeChartValue(value)))
                .sort((a, b) => rank.get(normalizeChartValue(a))! - rank.get(normalizeChartValue(b))!);
            return [...listed, ...byName.filter(value => !rank.has(normalizeChartValue(value)))];
        }
    }
}

/**
 * Colors for the values present in the chart: listed values use their color, others take automatic colors from
 * the standard palette, skipping colors already assigned so they don't clash.
 */
export function chartValueColors(values: string[], styling: ChartValueStyling): Map<string, string> {
    const explicit = new Map(styling.values.map(style => [normalizeChartValue(style.value), style.color]));
    const used = new Set(styling.values.map(style => style.color.toLowerCase()));
    const unused = standardChartColors.filter(color => !used.has(color.toLowerCase()));
    const automatic = unused.length > 0 ? unused : standardChartColors;
    let next = 0;
    return new Map(values.map(value => [
        value,
        explicit.get(normalizeChartValue(value)) ?? automatic[next++ % automatic.length],
    ]));
}

/** Spreads a preset's color stops across `count` values. */
export function presetColors(preset: ChartColorPreset, count: number): string[] {
    if (preset.id === 'standard') {
        return Array.from({ length: count }, (_, i) => preset.stops[i % preset.stops.length]);
    }
    if (count <= 1) return preset.stops.slice(0, count);

    const last = preset.stops.length - 1;
    return Array.from({ length: count }, (_, i) => {
        const position = (i * last) / (count - 1);
        const lower = Math.floor(position);
        const upper = Math.min(lower + 1, last);
        return mixColors(preset.stops[lower], preset.stops[upper], position - lower);
    });
}

/**
 * Every value a rules-mode text calculated field can produce, in rule order, or null when that isn't knowable.
 * Rules without a result, and a missing default, show up in charts as "(None)".
 */
export function calculatedFieldValues(field: ReportCalculatedField | undefined): string[] | null {
    if (!field || field.mode !== 'rules' || field.resultType !== 'text') return null;

    const results = [
        ...(field.rules || []).map(rule => rule.result?.value),
        field.defaultResult?.value,
        ...(field.missingResult ? [field.missingResult.value] : []),
    ];
    const values: string[] = [];
    let hasNone = false;
    results.forEach(result => {
        if (result === null || result === undefined || result.trim() === '') {
            hasNone = true;
        } else if (!values.some(value => normalizeChartValue(value) === normalizeChartValue(result))) {
            values.push(result);
        }
    });
    return hasNone ? [...values, noneChartValue] : values;
}

export function isValidChartColor(color: string): boolean {
    return /^#[0-9a-f]{6}$/i.test(color ?? '');
}

function noneLast(a: string, b: string): number {
    const aNone = isNoneChartValue(a);
    const bNone = isNoneChartValue(b);
    return aNone === bNone ? 0 : aNone ? 1 : -1;
}

function splitNatural(value: string): string[] {
    return value.match(/\d+|\D+/g) ?? [];
}

function compareNaturalPart(a: string, b: string): number {
    const aDigits = /^\d/.test(a);
    const bDigits = /^\d/.test(b);
    if (aDigits && bDigits) {
        const aTrimmed = a.replace(/^0+(?=\d)/, '');
        const bTrimmed = b.replace(/^0+(?=\d)/, '');
        if (aTrimmed.length !== bTrimmed.length) return aTrimmed.length - bTrimmed.length;
    }
    return a < b ? -1 : a > b ? 1 : 0;
}

function mixColors(from: string, to: string, amount: number): string {
    const channel = (hex: string, index: number) => parseInt(hex.slice(1 + index * 2, 3 + index * 2), 16);
    const mixed = [0, 1, 2].map(index =>
        Math.round(channel(from, index) + (channel(to, index) - channel(from, index)) * amount)
            .toString(16)
            .padStart(2, '0'));
    return `#${mixed.join('')}`.toUpperCase();
}
