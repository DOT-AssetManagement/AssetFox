<template>
    <v-card class="viewer-card elevation-2 mb-4">
        <v-card-title class="viewer-card-title d-flex align-center">
            <i :class="componentIcon" class="mr-2" />
            <span>{{ component.title }}</span>
            <v-chip
                size="x-small"
                :color="componentChipColor"
                variant="flat"
                class="ml-3"
            >
                {{ componentTypeLabel }}
            </v-chip>
            <v-spacer />
            <span v-if="totalPointCount > 0" class="total-badge">
                {{ totalPointCount.toLocaleString() }} points
            </span>
            <v-btn
                icon
                size="x-small"
                variant="text"
                class="ml-2"
                :title="showData ? 'Hide data' : 'Show data'"
                @click="showData = !showData"
            >
                <i class="fas fa-table" style="font-size: 11px; color: #777;" />
            </v-btn>
        </v-card-title>

        <v-card-text class="viewer-chart-body">
            <v-progress-linear
                v-if="isLoading"
                indeterminate
                color="#2A578D"
                class="mb-2"
            />

            <v-alert
                v-if="errorMessage"
                type="error"
                variant="tonal"
                density="compact"
                class="mb-2"
            >
                {{ errorMessage }}
            </v-alert>

            <div v-if="!isLoading && tableRows.length === 0 && !errorMessage" class="viewer-chart-empty">
                No chart data available
            </div>

            <VChart
                v-if="tableRows.length > 0"
                :option="chartOption"
                autoresize
                class="viewer-chart"
            />

            <div v-if="showData && tableRows.length > 0" class="viewer-chart-data">
                <table>
                    <thead>
                        <tr>
                            <th>{{ categoryHeader }}</th>
                            <th v-if="hasExplicitSeries">{{ seriesHeader }}</th>
                            <th>{{ valueHeader }}</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(row, index) in tableRows" :key="index">
                            <td>{{ row.category }}</td>
                            <td v-if="hasExplicitSeries">{{ row.series }}</td>
                            <td>{{ formatValue(row.value) }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from 'vue';
import { use } from 'echarts/core';
import { BarChart, LineChart, PieChart } from 'echarts/charts';
import {
    DatasetComponent,
    GraphicComponent,
    GridComponent,
    LegendComponent,
    TooltipComponent,
    TransformComponent,
} from 'echarts/components';
import { CanvasRenderer } from 'echarts/renderers';
import VChart from 'vue-echarts';
import type { EChartsOption } from 'echarts';
import {
    ChartConfig,
    ReportComponent,
    componentTypeInfo,
    standardChartColors,
} from '@/shared/models/iAM/report-schema';
import { PagedViewerResult } from '@/shared/models/iAM/viewer';
import ViewerService from '@/services/viewer.service';
import { hasValue } from '@/shared/utils/has-value-util';

use([
    BarChart,
    LineChart,
    PieChart,
    DatasetComponent,
    GraphicComponent,
    GridComponent,
    LegendComponent,
    TooltipComponent,
    TransformComponent,
    CanvasRenderer,
]);

interface ChartRow {
    category: string;
    series: string;
    value: number;
}

const props = defineProps<{
    component: ReportComponent;
    schemaId: string;
    simulationId: string;
}>();

const tableRows = ref<ChartRow[]>([]);
const isLoading = ref(false);
const errorMessage = ref('');
const showData = ref(false);
let activeFetchController: AbortController | null = null;

const config = computed(() => props.component.config as ChartConfig);
const typeInfo = componentTypeInfo[props.component.type];
const componentIcon = typeInfo?.icon || 'fas fa-chart-bar';
const componentTypeLabel = typeInfo?.label || props.component.type;
const componentChipColor = typeInfo?.color || '#E3F2FD';
const totalPointCount = computed(() => tableRows.value.length);
const categoryHeader = computed(() => config.value.bindings?.category?.label || 'Category');
const seriesHeader = computed(() => config.value.bindings?.series?.label || 'Series');
const valueHeader = computed(() => config.value.bindings?.value?.label || 'Value');
const hasExplicitSeries = computed(() => !!config.value.bindings?.series?.field);

const chartOption = computed<EChartsOption>(() => {
    if (config.value.chartKind === 'pie') {
        return buildPieOption();
    }

    return buildCartesianOption();
});

function buildCartesianOption(): EChartsOption {
    const categories = Array.from(new Set(tableRows.value.map(row => row.category)));
    const fallbackSeriesName = valueHeader.value || props.component.title || 'Value';
    const seriesNames = Array.from(new Set(tableRows.value.map(row => row.series || fallbackSeriesName)));
    const isLine = config.value.chartKind === 'line';
    const isStacked = hasExplicitSeries.value && (
        config.value.mode === 'stacked'
        || config.value.mode === 'stacked100'
        || config.value.display.stacked
    );
    const horizontal = config.value.chartKind === 'bar' && config.value.display.horizontal;
    const showLegend = hasExplicitSeries.value || seriesNames.length > 1;
    const legendTitle = showLegend && hasExplicitSeries.value ? seriesHeader.value : '';
    const valueAxisLayout = buildValueAxisLayout(horizontal);
    const valueAxis = {
        type: 'value',
        name: valueHeader.value,
        nameLocation: 'middle',
        nameGap: valueAxisLayout.nameGap,
        splitLine: { show: config.value.display.showGridLines },
        axisLabel: {
            formatter: (value: number) => formatAxisValue(value),
        },
    };
    const categoryAxis = {
        type: 'category',
        name: categoryHeader.value,
        nameLocation: 'middle',
        nameGap: categories.length > 8 ? 72 : 42,
        data: categories,
        axisLabel: { rotate: categories.length > 8 ? 35 : 0 },
    };

    const series = seriesNames.map((seriesName) => ({
        name: seriesName,
        type: isLine ? 'line' : 'bar',
        stack: isStacked ? 'total' : undefined,
        areaStyle: isLine && config.value.mode === 'area' ? {} : undefined,
        showSymbol: isLine ? config.value.display.showMarkers : undefined,
        label: {
            show: config.value.display.showLabels,
            position: horizontal ? 'right' : 'top',
            formatter: (params: any) => formatValue(toNumber(params.value)),
        },
        data: categories.map((category) => {
            const match = tableRows.value.find(row =>
                row.category === category && (row.series || fallbackSeriesName) === seriesName
            );
            return match?.value ?? 0;
        }),
    }));

    return {
        color: standardChartColors,
        tooltip: {
            trigger: 'axis',
            valueFormatter: (value: any) => formatValue(toNumber(value)),
        },
        legend: legendOption(showLegend),
        graphic: legendTitleGraphic(showLegend, legendTitle),
        grid: {
            left: horizontal ? 104 : valueAxisLayout.left,
            right: showLegend && config.value.display.showLegend && config.value.display.legendPosition === 'right' ? 180 : 24,
            top: 32,
            bottom: showLegend && config.value.display.showLegend && config.value.display.legendPosition === 'bottom'
                ? 104
                : (horizontal ? 84 : categories.length > 8 ? 88 : 76),
            containLabel: true,
        },
        xAxis: horizontal
            ? valueAxis
            : categoryAxis,
        yAxis: horizontal
            ? {
                ...categoryAxis,
                nameGap: 60,
                axisLabel: { rotate: 0 },
            }
            : valueAxis,
        series,
    };
}

function buildPieOption(): EChartsOption {
    const legendTitle = categoryHeader.value;
    return {
        color: standardChartColors,
        tooltip: {
            trigger: 'item',
            valueFormatter: (value: any) => formatValue(toNumber(value)),
        },
        legend: legendOption(true),
        graphic: legendTitleGraphic(true, legendTitle),
        series: [
            {
                name: props.component.title,
                type: 'pie',
                radius: config.value.display.donut || config.value.mode === 'donut'
                    ? ['45%', '70%']
                    : '70%',
                label: {
                    show: config.value.display.showLabels,
                    formatter: (params: any) => `${params.name}: ${formatValue(toNumber(params.value))} (${params.percent}%)`,
                },
                data: tableRows.value.map(row => ({
                    name: row.category,
                    value: row.value,
                })),
            },
        ],
    };
}

function legendOption(hasSeries = true) {
    return {
        show: hasSeries && config.value.display.showLegend,
        type: 'scroll',
        orient: config.value.display.legendPosition === 'bottom' ? 'horizontal' : 'vertical',
        right: config.value.display.legendPosition === 'right' ? 8 : undefined,
        bottom: config.value.display.legendPosition === 'bottom' ? 0 : undefined,
        top: config.value.display.legendPosition === 'right' ? 30 : undefined,
    };
}

function legendTitleGraphic(showLegend: boolean, title: string) {
    if (!showLegend || !config.value.display.showLegend || !title) {
        return undefined;
    }

    const isBottomLegend = config.value.display.legendPosition === 'bottom';
    return {
        type: 'text',
        right: isBottomLegend ? undefined : 8,
        left: isBottomLegend ? 48 : undefined,
        top: isBottomLegend ? undefined : 6,
        bottom: isBottomLegend ? 34 : undefined,
        style: {
            text: title,
            fill: '#526071',
            font: '600 12px sans-serif',
        },
        silent: true,
    };
}

function formatValue(value: number): string {
    const format = config.value.bindings?.value?.format || 'number';
    if (format === 'currency') {
        return new Intl.NumberFormat(undefined, {
            style: 'currency',
            currency: 'USD',
            minimumFractionDigits: 2,
            maximumFractionDigits: 2,
        }).format(value);
    }
    if (format === 'percent') {
        return value.toLocaleString(undefined, { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + '%';
    }
    return value.toLocaleString();
}

function formatAxisValue(value: number): string {
    const format = config.value.bindings?.value?.format || 'number';
    if (format === 'currency') {
        return new Intl.NumberFormat(undefined, {
            style: 'currency',
            currency: 'USD',
            notation: 'compact',
            maximumFractionDigits: 1,
        }).format(value);
    }
    if (format === 'percent') {
        return value.toLocaleString(undefined, { maximumFractionDigits: 1 }) + '%';
    }
    return value.toLocaleString(undefined, { notation: 'compact', maximumFractionDigits: 1 });
}

function buildValueAxisLayout(horizontal: boolean) {
    if (horizontal) {
        return {
            left: 104,
            nameGap: 54,
        };
    }

    const labelWidth = estimateWidestValueAxisLabel();
    const nameGap = clamp(Math.ceil(labelWidth + 24), 56, 112);
    return {
        left: clamp(nameGap + 34, 92, 152),
        nameGap,
    };
}

function estimateWidestValueAxisLabel(): number {
    const values = tableRows.value
        .map(row => row.value)
        .filter(value => Number.isFinite(value));

    if (values.length === 0) {
        return estimateTextWidth(formatAxisValue(0));
    }

    const max = Math.max(...values);
    const min = Math.min(...values);
    const maxAbs = Math.max(Math.abs(max), Math.abs(min));
    const candidates = [
        0,
        min,
        max,
        maxAbs,
        maxAbs * 0.75,
        maxAbs * 0.5,
        maxAbs * 0.25,
    ];

    return Math.max(...candidates.map(value => estimateTextWidth(formatAxisValue(value))));
}

function estimateTextWidth(text: string): number {
    return Array.from(text).reduce((width, char) => {
        if (char === ',' || char === '.' || char === '$') return width + 4;
        if (char === '-' || char === '(' || char === ')') return width + 5;
        if (char === 'M' || char === 'B' || char === 'T') return width + 8;
        return width + 7;
    }, 0);
}

function clamp(value: number, min: number, max: number): number {
    return Math.min(Math.max(value, min), max);
}

async function fetchData() {
    if (activeFetchController) {
        activeFetchController.abort();
    }

    const controller = new AbortController();
    activeFetchController = controller;
    isLoading.value = true;
    errorMessage.value = '';

    try {
        const response = await ViewerService.getComponentData(
            props.schemaId,
            props.component.id,
            {
                simulationId: props.simulationId,
                page: 1,
                pageSize: 5000,
                sortBy: '',
                sortDirection: 'asc',
                columnFilters: {},
            },
            controller.signal,
        );

        if (controller.signal.aborted) return;

        if (hasValue(response, 'data')) {
            const result: PagedViewerResult = response.data;
            tableRows.value = mapRows(result);
        }
    } catch (e: any) {
        if (e?.code === 'ERR_CANCELED' || e?.name === 'CanceledError' || controller.signal.aborted) {
            return;
        }
        errorMessage.value = e?.response?.data?.error || e?.message || 'Failed to load chart data.';
    } finally {
        if (activeFetchController === controller) {
            activeFetchController = null;
            isLoading.value = false;
        }
    }
}

function mapRows(result: PagedViewerResult): ChartRow[] {
    const categoryIndex = result.columns.findIndex(col => col.key === 'category');
    const seriesIndex = result.columns.findIndex(col => col.key === 'series');
    const valueIndex = result.columns.findIndex(col => col.key === 'value');

    if (categoryIndex < 0 || valueIndex < 0) {
        return [];
    }

    return (result.rows || []).map(row => ({
        category: row[categoryIndex] === null || row[categoryIndex] === undefined ? '(None)' : String(row[categoryIndex]),
        series: seriesIndex >= 0 && row[seriesIndex] !== null && row[seriesIndex] !== undefined ? String(row[seriesIndex]) : '',
        value: toNumber(row[valueIndex]),
    }));
}

function toNumber(value: any): number {
    if (typeof value === 'number') return value;
    const parsed = parseFloat(value);
    return isNaN(parsed) ? 0 : parsed;
}

onBeforeUnmount(() => {
    if (activeFetchController) activeFetchController.abort();
});

void fetchData();
</script>

<style scoped>
.total-badge {
    color: #666;
    font-size: 0.8rem;
    font-weight: 400;
}

.viewer-chart-body {
    min-height: 360px;
    padding: 12px 16px 16px 16px !important;
}

.viewer-chart {
    width: 100%;
    height: 340px;
}

.viewer-chart-empty {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 260px;
    color: #888;
    font-size: 0.9rem;
}

.viewer-chart-data {
    max-height: 260px;
    overflow: auto;
    margin-top: 12px;
    border: 1px solid #E1E7EE;
    border-radius: 6px;
}

.viewer-chart-data table {
    width: 100%;
    border-collapse: collapse;
    font-size: 12px;
}

.viewer-chart-data th,
.viewer-chart-data td {
    padding: 6px 8px;
    border-bottom: 1px solid #EEF1F5;
    text-align: left;
}

.viewer-chart-data th {
    background: #F5F6F8;
    color: #526071;
    font-weight: 700;
}
</style>
