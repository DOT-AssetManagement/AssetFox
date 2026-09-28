<template>
    <div class="colors-order">
        <div v-if="!binding" class="colors-order__hint">
            Choose a category on the Data tab first.
        </div>

        <template v-else>
            <div class="colors-order__target mb-2">
                Styling the {{ roleLabel }}: <strong>{{ binding.label }}</strong>
            </div>

            <v-alert v-if="isStale" type="info" variant="tonal" density="compact" class="mb-2">
                These colors were set for a different field, so the chart isn't using them.
                <template #append>
                    <v-btn size="small" variant="text" class="text-none" @click="onClear">Clear</v-btn>
                </template>
            </v-alert>

            <template v-else>
                <v-row dense class="mb-1">
                    <v-col cols="7">
                        <v-select
                            :model-value="order"
                            :items="orderOptions"
                            label="Order"
                            variant="outlined"
                            density="compact"
                            hide-details
                            @update:model-value="onOrderChange"
                        />
                    </v-col>
                    <v-col cols="5" class="d-flex align-center">
                        <v-menu location="bottom end">
                            <template #activator="{ props: menuProps }">
                                <v-btn
                                    v-bind="menuProps"
                                    :disabled="localValues.length === 0"
                                    variant="outlined"
                                    size="small"
                                    class="text-none w-100"
                                >
                                    <i class="fas fa-palette mr-2" style="font-size: 11px;" />
                                    Apply colors
                                </v-btn>
                            </template>
                            <v-list density="compact">
                                <v-list-item
                                    v-for="preset in chartColorPresets"
                                    :key="preset.id"
                                    @click="onApplyPreset(preset)"
                                >
                                    <div class="d-flex align-center ga-2">
                                        <span class="preset-strip">
                                            <span
                                                v-for="(color, index) in presetColors(preset, Math.max(rampValueCount, 3))"
                                                :key="index"
                                                :style="{ background: color }"
                                            />
                                        </span>
                                        <span class="text-body-2">{{ preset.label }}</span>
                                    </div>
                                </v-list-item>
                            </v-list>
                        </v-menu>
                    </v-col>
                </v-row>

                <div v-if="isSingleLine" class="colors-order__hint mb-2">
                    A single line uses one color, so only the order applies.
                </div>
                <div v-else-if="role === 'category'" class="colors-order__hint mb-2">
                    Excel exports keep this order but use the standard colors for {{ config.chartKind === 'pie' ? 'slices' : 'bars' }}.
                </div>

                <draggable
                    :list="localValues"
                    item-key="value"
                    handle=".value-row__handle"
                    :animation="150"
                    ghost-class="value-row--ghost"
                    @change="onReorder"
                >
                    <template #item="{ element, index }">
                        <div class="value-row">
                            <i class="fas fa-grip-vertical value-row__handle" aria-hidden="true" />
                            <v-menu :close-on-content-click="false" location="bottom start">
                                <template #activator="{ props: menuProps }">
                                    <button
                                        v-bind="menuProps"
                                        type="button"
                                        class="value-row__swatch"
                                        :style="{ background: element.color }"
                                        :aria-label="`Change color for ${element.value}`"
                                    />
                                </template>
                                <v-card class="pa-2">
                                    <v-color-picker
                                        :model-value="element.color"
                                        mode="hex"
                                        :modes="['hex']"
                                        show-swatches
                                        :swatches="swatches"
                                        width="260"
                                        elevation="0"
                                        @update:model-value="(color: string) => onColorChange(index, color)"
                                    />
                                </v-card>
                            </v-menu>
                            <span class="value-row__label" :title="element.value">{{ element.value }}</span>
                            <v-btn
                                icon
                                size="x-small"
                                variant="text"
                                :aria-label="`Remove ${element.value}`"
                                @click="onRemove(index)"
                            >
                                <i class="fas fa-times" style="font-size: 11px;" />
                            </v-btn>
                        </div>
                    </template>
                </draggable>

                <div v-if="localValues.length === 0 && missingKnownValues.length === 0" class="colors-order__hint mb-2">
                    Add the values you want to color or put in a set order.
                </div>

                <v-btn
                    v-if="missingKnownValues.length > 0"
                    variant="tonal"
                    size="small"
                    color="#2A578D"
                    class="text-none mb-2"
                    @click="onAddKnownValues"
                >
                    <i class="fas fa-plus mr-2" style="font-size: 10px;" />
                    Add {{ missingKnownValues.length === 1 ? `"${missingKnownValues[0]}"` : `${missingKnownValues.length} values` }}
                    from {{ binding.label }}
                </v-btn>

                <div class="d-flex align-start ga-2">
                    <v-text-field
                        v-model="newValue"
                        label="Add a value"
                        variant="outlined"
                        density="compact"
                        :error-messages="addError"
                        :hide-details="!addError"
                        @update:model-value="addError = ''"
                        @keyup.enter="onAddValue"
                    />
                    <v-btn variant="outlined" size="small" class="text-none mt-1" @click="onAddValue">Add</v-btn>
                    <v-btn
                        variant="text"
                        size="small"
                        class="text-none mt-1"
                        :disabled="localValues.length < 2"
                        @click="onReverse"
                    >
                        Reverse
                    </v-btn>
                </div>

                <div class="colors-order__hint mt-2">
                    {{ footerHint }}
                </div>
                <v-btn
                    v-if="localValues.length > 0 || order !== 'listed'"
                    variant="text"
                    size="small"
                    class="text-none px-0 mt-1"
                    @click="onClear"
                >
                    Reset to defaults
                </v-btn>
            </template>
        </template>
    </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { clone } from 'ramda';
import draggable from 'vuedraggable';
import {
    ChartConfig,
    ChartValueOrder,
    ChartValueStyle,
    ChartValueStyling,
    ReportCalculatedField,
    standardChartColors,
} from '@/shared/models/iAM/report-schema';
import {
    ChartColorPreset,
    calculatedFieldValues,
    chartBindingKey,
    chartColorPresets,
    chartStyledBinding,
    chartStyledRole,
    isNoneChartValue,
    isValidChartColor,
    maxStyledChartValues,
    noneChartColor,
    normalizeChartValue,
    presetColors,
} from '@/shared/utils/report-chart-styling';

const props = defineProps<{
    config: ChartConfig;
    calculatedFields: ReportCalculatedField[];
}>();

const emit = defineEmits<{
    (e: 'update:valueStyling', val: ChartValueStyling | undefined): void;
}>();

const orderOptions: { title: string; value: ChartValueOrder }[] = [
    { title: 'As listed below', value: 'listed' },
    { title: 'A to Z', value: 'alphabetical' },
    { title: 'Z to A', value: 'alphabeticalDesc' },
    { title: 'Largest first', value: 'valueDesc' },
];

// v-color-picker lays each inner array out as a column.
const swatches = [
    ['#2E7D32', '#7CB342', '#F9A825', '#EF6C00', '#C62828'],
    standardChartColors.slice(0, 4),
    standardChartColors.slice(4, 8),
    ['#C6DBEF', '#9ECAE1', '#6BAED6', '#3182BD', '#08519C'],
    ['#E0E0E0', '#9E9E9E', '#616161', '#212121'],
];

const newValue = ref('');
const addError = ref('');

const binding = computed(() => chartStyledBinding(props.config));
const role = computed(() => chartStyledRole(props.config));
const bindingKey = computed(() => chartBindingKey(binding.value));
const isStale = computed(() => !!props.config.valueStyling && props.config.valueStyling.bindingKey !== bindingKey.value);
const activeStyling = computed(() => (isStale.value ? undefined : props.config.valueStyling));
const order = computed<ChartValueOrder>(() => activeStyling.value?.order ?? 'listed');
const isSingleLine = computed(() => props.config.chartKind === 'line' && role.value === 'category');

const localValues = ref<ChartValueStyle[]>(clone(activeStyling.value?.values ?? []));
watch(activeStyling, (styling) => {
    localValues.value = clone(styling?.values ?? []);
}, { deep: true });

const roleLabel = computed(() => {
    if (role.value === 'series') {
        return props.config.display?.stacked ? 'stack field' : 'series field';
    }
    return props.config.chartKind === 'pie' ? 'slice field' : 'category field';
});

const knownValues = computed<string[]>(() => {
    if (binding.value?.source !== 'calculatedField') return [];
    const field = props.calculatedFields.find(candidate => candidate.id === binding.value?.field);
    return calculatedFieldValues(field) ?? [];
});

const rampValueCount = computed(() => localValues.value.filter(style => !isNoneChartValue(style.value)).length);

const missingKnownValues = computed(() => {
    const listed = new Set(localValues.value.map(style => normalizeChartValue(style.value)));
    return knownValues.value.filter(value => !listed.has(normalizeChartValue(value)));
});

const footerHint = computed(() => {
    const others = 'Values you don\'t list use automatic colors';
    switch (order.value) {
        case 'alphabetical':
            return `${others}. The chart orders all values A to Z.`;
        case 'alphabeticalDesc':
            return `${others}. The chart orders all values Z to A.`;
        case 'valueDesc':
            return `${others}. The chart puts the largest values first.`;
        default:
            return `${others} and come after the listed ones, A to Z.`;
    }
});

function emitStyling(values: ChartValueStyle[], nextOrder: ChartValueOrder) {
    if (values.length === 0 && nextOrder === 'listed') {
        emit('update:valueStyling', undefined);
        return;
    }

    emit('update:valueStyling', {
        bindingKey: bindingKey.value,
        order: nextOrder,
        values: clone(values),
    });
}

function nextColor(values: ChartValueStyle[], value: string): string {
    if (isNoneChartValue(value)) return noneChartColor;

    const used = new Set(values.map(style => style.color.toLowerCase()));
    return standardChartColors.find(color => !used.has(color.toLowerCase()))
        ?? standardChartColors[values.length % standardChartColors.length];
}

function onAddValue() {
    const value = newValue.value.trim();
    if (!value) {
        addError.value = 'Enter a value first.';
        return;
    }
    if (localValues.value.some(style => normalizeChartValue(style.value) === normalizeChartValue(value))) {
        addError.value = 'That value is already listed.';
        return;
    }
    if (localValues.value.length >= maxStyledChartValues) {
        addError.value = `You can list up to ${maxStyledChartValues} values.`;
        return;
    }

    const values = [...localValues.value, { value, color: nextColor(localValues.value, value) }];
    newValue.value = '';
    emitStyling(values, order.value);
}

function onAddKnownValues() {
    const values = [...localValues.value];
    missingKnownValues.value
        .slice(0, Math.max(0, maxStyledChartValues - values.length))
        .forEach(value => values.push({ value, color: nextColor(values, value) }));
    emitStyling(values, order.value);
}

function onRemove(index: number) {
    const values = localValues.value.filter((_, i) => i !== index);
    emitStyling(values, order.value);
}

function onReorder() {
    // Dragging means the user wants their own order.
    emitStyling(localValues.value, 'listed');
}

function onReverse() {
    emitStyling([...localValues.value].reverse(), 'listed');
}

function onOrderChange(nextOrder: ChartValueOrder) {
    emitStyling(localValues.value, nextOrder);
}

function onApplyPreset(preset: ChartColorPreset) {
    // The ramp spans the real values; "(None)" keeps a neutral color so Poor, not missing data, gets the last stop.
    const colors = presetColors(preset, rampValueCount.value);
    let next = 0;
    const values = localValues.value.map(style => ({
        ...style,
        color: isNoneChartValue(style.value) ? noneChartColor : colors[next++],
    }));
    emitStyling(values, order.value);
}

function onColorChange(index: number, color: string) {
    const hex = (color || '').slice(0, 7).toUpperCase();
    if (!isValidChartColor(hex) || localValues.value[index]?.color === hex) return;

    const values = localValues.value.map((style, i) => (i === index ? { ...style, color: hex } : style));
    emitStyling(values, order.value);
}

function onClear() {
    emit('update:valueStyling', undefined);
}
</script>

<style scoped>
.colors-order__target {
    font-size: 12px;
    color: #526071;
}

.colors-order__hint {
    font-size: 12px;
    color: #7A8594;
    line-height: 1.5;
}

.value-row {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 4px 6px 4px 8px;
    margin-bottom: 6px;
    border: 1px solid #DDE4EC;
    border-radius: 6px;
    background: #FFFFFF;
    font-size: 13px;
}

.value-row--ghost {
    opacity: 0.4;
}

.value-row__handle {
    color: #A0AAB6;
    font-size: 11px;
    cursor: grab;
}

.value-row__swatch {
    width: 20px;
    height: 20px;
    flex: none;
    border-radius: 4px;
    border: 1px solid rgba(0, 0, 0, 0.2);
    cursor: pointer;
}

.value-row__label {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.preset-strip {
    display: inline-flex;
    border-radius: 3px;
    overflow: hidden;
    border: 1px solid rgba(0, 0, 0, 0.15);
}

.preset-strip span {
    width: 14px;
    height: 12px;
}
</style>
