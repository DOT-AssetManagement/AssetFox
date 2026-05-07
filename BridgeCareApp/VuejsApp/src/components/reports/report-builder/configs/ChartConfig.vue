<template>
    <div class="chart-config">
        <div class="chart-config__sentence mb-3">
            <span>Show</span>
            <button type="button" class="chart-token" @click="focusValue">
                {{ valueLabel }}
            </button>
            <span>by</span>
            <button type="button" class="chart-token" @click="focusCategory">
                {{ categoryLabel }}
            </button>
            <template v-if="showSeriesControls">
                <span>{{ localConfig.display.stacked ? 'stacked by' : 'split by' }}</span>
                <button type="button" class="chart-token" @click="focusSeries">
                    {{ seriesLabel }}
                </button>
            </template>
        </div>

        <v-alert
            v-for="issue in validationIssues"
            :key="issue.message"
            :type="issue.blocker ? 'error' : 'warning'"
            variant="tonal"
            density="compact"
            class="mb-2"
        >
            {{ issue.message }}
        </v-alert>

        <v-tabs v-model="activeTab" density="compact" color="#2A578D" class="mb-3">
            <v-tab value="data" class="text-none">Data</v-tab>
            <v-tab value="shape" class="text-none">Shape</v-tab>
        </v-tabs>

        <v-window v-model="activeTab" class="chart-config__window">
            <v-window-item value="data">
                <v-autocomplete
                    ref="valueInput"
                    :model-value="valueId"
                    :items="valueOptions"
                    item-title="label"
                    item-value="id"
                    label="Value"
                    variant="outlined"
                    density="compact"
                    class="mb-2"
                    @update:model-value="onValueSelect"
                />
                <v-select
                    v-if="showAggregatePicker"
                    v-model="selectedAggregate"
                    :items="aggregateOptions"
                    label="Aggregation"
                    variant="outlined"
                    density="compact"
                    class="mb-2"
                    @update:model-value="emitUpdate"
                />
                <v-autocomplete
                    ref="categoryInput"
                    :model-value="categoryId"
                    :items="categoryOptions"
                    item-title="label"
                    item-value="id"
                    label="Category"
                    variant="outlined"
                    density="compact"
                    class="mb-2"
                    @update:model-value="onCategorySelect"
                />
                <v-autocomplete
                    v-if="showSeriesControls"
                    ref="seriesInput"
                    :model-value="seriesId"
                    :items="seriesOptions"
                    item-title="label"
                    item-value="id"
                    label="Series / stack (optional)"
                    variant="outlined"
                    density="compact"
                    clearable
                    class="mb-2"
                    @update:model-value="onSeriesSelect"
                />
                <v-checkbox
                    v-if="showExcludeNoTreatment"
                    v-model="localConfig.filters.excludeNoTreatment"
                    label='Exclude "No Treatment"'
                    density="compact"
                    hide-details
                    class="mb-2"
                    @update:model-value="emitUpdate"
                />
                <v-text-field
                    v-if="chartKind === 'pie'"
                    :model-value="pieTopN"
                    type="number"
                    label="Top N"
                    min="1"
                    max="100"
                    step="1"
                    variant="outlined"
                    density="compact"
                    class="mb-2"
                    @update:model-value="onPieTopNChange"
                />
                <v-row dense>
                    <v-col cols="6">
                        <v-select
                            v-model="localConfig.yearScope.mode"
                            :items="yearScopeOptions"
                            label="Year scope"
                            variant="outlined"
                            density="compact"
                            @update:model-value="emitUpdate"
                        />
                    </v-col>
                    <v-col v-if="localConfig.yearScope.mode === 'selectedYear'" cols="6">
                        <v-text-field
                            v-model.number="localConfig.yearScope.selectedYear"
                            type="number"
                            label="Year"
                            variant="outlined"
                            density="compact"
                            @update:model-value="emitUpdate"
                        />
                    </v-col>
                </v-row>
                <v-row v-if="localConfig.yearScope.mode === 'range'" dense>
                    <v-col cols="6">
                        <v-text-field
                            v-model.number="localConfig.yearScope.startYear"
                            type="number"
                            label="Start"
                            variant="outlined"
                            density="compact"
                            @update:model-value="emitUpdate"
                        />
                    </v-col>
                    <v-col cols="6">
                        <v-text-field
                            v-model.number="localConfig.yearScope.endYear"
                            type="number"
                            label="End"
                            variant="outlined"
                            density="compact"
                            @update:model-value="emitUpdate"
                        />
                    </v-col>
                </v-row>
            </v-window-item>

            <v-window-item value="shape">
                <v-select
                    v-model="localConfig.mode"
                    :items="modeOptions"
                    label="Chart mode"
                    variant="outlined"
                    density="compact"
                    class="mb-2"
                    @update:model-value="onModeChange"
                />
                <v-checkbox
                    v-if="chartKind === 'bar'"
                    v-model="localConfig.display.horizontal"
                    label="Horizontal bars"
                    density="compact"
                    hide-details
                    class="mb-1"
                    @update:model-value="emitUpdate"
                />
                <v-checkbox
                    v-if="chartKind === 'line'"
                    v-model="localConfig.display.showMarkers"
                    label="Show point markers"
                    density="compact"
                    hide-details
                    class="mb-1"
                    @update:model-value="emitUpdate"
                />
                <v-checkbox
                    v-model="localConfig.display.showLegend"
                    label="Show legend"
                    density="compact"
                    hide-details
                    class="mb-1"
                    @update:model-value="emitUpdate"
                />
                <v-checkbox
                    v-model="localConfig.display.showLabels"
                    label="Show value labels"
                    density="compact"
                    hide-details
                    @update:model-value="emitUpdate"
                />
            </v-window-item>
        </v-window>
    </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { clone } from 'ramda';
import { useStore } from 'vuex';
import {
    ChartAggregation,
    ChartConfig,
    ChartFieldBinding,
    ChartKind,
    ComponentType,
} from '@/shared/models/iAM/report-schema';
import { validateChartConfig } from '@/shared/utils/report-chart-validation';

interface FieldOption extends ChartFieldBinding {
    id: string;
    dataType: string;
}

const props = defineProps<{
    config: ChartConfig;
    componentType: ComponentType;
}>();

const emit = defineEmits<{
    (e: 'update:config', val: ChartConfig): void;
}>();

const store = useStore();
const activeTab = ref('data');
const localConfig = ref<ChartConfig>(clone(props.config));
const valueInput = ref<any>(null);
const categoryInput = ref<any>(null);
const seriesInput = ref<any>(null);

watch(() => props.config, (val) => {
    localConfig.value = clone(val);
}, { deep: true });

const chartKind = computed<ChartKind>(() =>
    localConfig.value.chartKind || (props.componentType === 'pieChart' ? 'pie' : props.componentType === 'lineChart' ? 'line' : 'bar')
);

const attributeOptions = computed<FieldOption[]>(() => {
    const attrs = store.state.attributeModule?.attributes ?? [];
    return attrs.map((a: any) => ({
        id: `attribute:${a.name}`,
        source: 'attribute',
        field: a.name,
        label: a.name,
        role: 'dimension',
        format: a.type === 'NUMBER' ? 'number' : 'text',
        dataType: a.type,
    }));
});

const builtInCategoryOptions: FieldOption[] = [
    { id: 'simulationYear:year', source: 'simulationYear', field: 'year', label: 'Year', role: 'dimension', format: 'year', dataType: 'NUMBER' },
    { id: 'assetDetail:appliedTreatment', source: 'assetDetail', field: 'appliedTreatment', label: 'Applied Treatment', role: 'dimension', format: 'text', dataType: 'STRING' },
    { id: 'treatment:treatmentName', source: 'treatment', field: 'treatmentName', label: 'Treatment', role: 'dimension', format: 'text', dataType: 'STRING' },
    { id: 'allocation:budgetName', source: 'allocation', field: 'budgetName', label: 'Budget', role: 'dimension', format: 'text', dataType: 'STRING' },
];

const categoryOptions = computed<FieldOption[]>(() => [
    ...builtInCategoryOptions,
    ...attributeOptions.value,
]);

const seriesOptions = computed<FieldOption[]>(() => categoryOptions.value.map(option => ({
    ...option,
    role: 'series',
})));

const valueOptions = computed<FieldOption[]>(() => [
    { id: 'allocation:allocatedAmount', source: 'allocation', field: 'allocatedAmount', label: 'Dollars Spent', role: 'measure', aggregate: 'sum', format: 'currency', dataType: 'NUMBER' },
    { id: 'assetDetail:assetCount', source: 'assetDetail', field: 'assetCount', label: 'Asset Count', role: 'measure', aggregate: 'count', format: 'number', dataType: 'NUMBER' },
    ...attributeOptions.value
        .filter(option => option.dataType === 'NUMBER')
        .map(option => ({
            ...option,
            id: `attribute:${option.field}:measure`,
            role: 'measure' as const,
            aggregate: 'avg' as ChartAggregation,
        })),
]);

const valueId = computed(() => bindingId(localConfig.value.bindings.value));
const categoryId = computed(() => bindingId(localConfig.value.bindings.category));
const seriesId = computed(() => bindingId(localConfig.value.bindings.series));

const valueLabel = computed(() => localConfig.value.bindings.value?.label || 'a value');
const categoryLabel = computed(() => localConfig.value.bindings.category?.label || 'a category');
const seriesLabel = computed(() => localConfig.value.bindings.series?.label || 'none');
const showSeriesControls = computed(() =>
    chartKind.value !== 'pie' && localConfig.value.mode !== 'single'
);
const showExcludeNoTreatment = computed(() =>
    isTreatmentBinding(localConfig.value.bindings.category)
    || isTreatmentBinding(localConfig.value.bindings.series)
);

const validationIssues = computed(() =>
    validateChartConfig('preview', props.componentType, localConfig.value)
);

const showAggregatePicker = computed(() => {
    const value = localConfig.value.bindings.value;
    return !!value && value.field !== 'assetCount';
});
const pieTopN = computed(() => localConfig.value.limit.topN ?? '');

const selectedAggregate = computed({
    get: () => localConfig.value.bindings.value?.aggregate || 'sum',
    set: (val: ChartAggregation) => {
        if (localConfig.value.bindings.value) {
            localConfig.value.bindings.value.aggregate = val;
        }
    },
});

const aggregateOptions = [
    { title: 'Sum', value: 'sum' },
    { title: 'Average', value: 'avg' },
    { title: 'Minimum', value: 'min' },
    { title: 'Maximum', value: 'max' },
    { title: 'Count', value: 'count' },
];

const yearScopeOptions = [
    { title: 'All years', value: 'allYears' },
    { title: 'Selected year', value: 'selectedYear' },
    { title: 'Year range', value: 'range' },
];

const modeOptions = computed(() => {
    switch (chartKind.value) {
        case 'pie':
            return [
                { title: 'Pie', value: 'pie' },
                { title: 'Donut', value: 'donut' },
            ];
        case 'line':
            return [
                { title: 'Line', value: 'line' },
                { title: 'Area', value: 'area' },
            ];
        default:
            return [
                { title: 'Single', value: 'single' },
                { title: 'Grouped', value: 'grouped' },
                { title: 'Stacked', value: 'stacked' },
                { title: '100% Stacked', value: 'stacked100' },
            ];
    }
});

function bindingId(binding?: ChartFieldBinding): string {
    if (!binding?.source || !binding?.field) return '';
    if (binding.role === 'measure' && binding.source === 'attribute') {
        return `${binding.source}:${binding.field}:measure`;
    }
    return `${binding.source}:${binding.field}`;
}

function findOption(options: FieldOption[], id: string): FieldOption | undefined {
    return options.find(option => option.id === id);
}

function toBinding(option: FieldOption): ChartFieldBinding {
    return {
        source: option.source,
        field: option.field,
        label: option.label,
        role: option.role,
        aggregate: option.aggregate,
        format: option.format,
    };
}

function onValueSelect(id: string) {
    const option = findOption(valueOptions.value, id);
    localConfig.value.bindings.value = option ? toBinding(option) : undefined;
    emitUpdate();
}

function onCategorySelect(id: string) {
    const option = findOption(categoryOptions.value, id);
    localConfig.value.bindings.category = option ? toBinding(option) : undefined;
    emitUpdate();
}

function onSeriesSelect(id: string | null) {
    const option = id ? findOption(seriesOptions.value, id) : undefined;
    localConfig.value.bindings.series = option ? toBinding(option) : undefined;
    emitUpdate();
}

function onPieTopNChange(value: string | number | null) {
    if (value === null || value === '') {
        localConfig.value.limit.topN = 8;
        emitUpdate();
        return;
    }

    const parsed = Math.trunc(Number(value));
    if (Number.isFinite(parsed)) {
        localConfig.value.limit.topN = clamp(parsed, 1, 100);
    }

    emitUpdate();
}

function onModeChange() {
    localConfig.value.display.stacked = localConfig.value.mode === 'stacked' || localConfig.value.mode === 'stacked100';
    localConfig.value.display.donut = localConfig.value.mode === 'donut';
    if (localConfig.value.mode === 'single') {
        localConfig.value.bindings.series = undefined;
    }
    emitUpdate();
}

function focusValue() {
    activeTab.value = 'data';
    valueInput.value?.focus?.();
}

function focusCategory() {
    activeTab.value = 'data';
    categoryInput.value?.focus?.();
}

function focusSeries() {
    activeTab.value = 'data';
    seriesInput.value?.focus?.();
}

function isTreatmentBinding(binding?: ChartFieldBinding): boolean {
    return !!binding
        && ((binding.source === 'assetDetail' && binding.field === 'appliedTreatment')
            || (binding.source === 'treatment' && binding.field === 'treatmentName'));
}

function clamp(value: number, min: number, max: number): number {
    return Math.min(Math.max(value, min), max);
}

function emitUpdate() {
    if (!showExcludeNoTreatment.value && localConfig.value.filters.excludeNoTreatment) {
        localConfig.value.filters.excludeNoTreatment = false;
    }

    emit('update:config', clone(localConfig.value));
}
</script>

<style scoped>
.chart-config__sentence {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 6px;
    font-size: 12px;
    color: #526071;
    line-height: 1.5;
}

.chart-token {
    border: 1px solid #C8D3DF;
    border-radius: 6px;
    background: #F7FAFD;
    color: #214A78;
    font-size: 12px;
    font-weight: 700;
    padding: 3px 7px;
    cursor: pointer;
}

.chart-token:hover {
    border-color: #2A578D;
    background: #EEF5FC;
}

.chart-config__window {
    padding-top: 10px;
}

.chart-config :deep(.v-window__container),
.chart-config :deep(.v-window-item) {
    overflow: visible;
}
</style>
