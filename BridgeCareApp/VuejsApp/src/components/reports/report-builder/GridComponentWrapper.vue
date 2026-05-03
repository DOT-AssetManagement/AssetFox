<template>
    <div
        class="grid-component-wrapper"
        :class="{ 'grid-component-wrapper--selected': selected, 'grid-component-wrapper--hidden': !component.visible }"
        @click.stop="$emit('select')"
    >
        <!-- Header — entire bar is the drag surface -->
        <div class="grid-component-header">
            <v-chip
                size="x-small"
                variant="flat"
                :style="{ backgroundColor: typeInfo.chipColor, color: typeInfo.color }"
                class="mr-2"
            >
                <i :class="typeInfo.icon" style="font-size: 9px;" class="mr-1" />
                {{ typeInfo.label }}
            </v-chip>

            <span class="grid-component-title">{{ component.title }}</span>

            <v-chip
                v-if="hasFilter"
                size="x-small"
                color="#E3F2FD"
                variant="flat"
                class="mr-1"
            >
                <i class="fas fa-filter mr-1" style="font-size: 8px;" />
                Filtered
            </v-chip>

            <v-chip
                v-if="!component.visible"
                size="x-small"
                color="#F5F5F5"
                variant="flat"
                class="mr-1"
            >
                <i class="fas fa-eye-slash" style="font-size: 8px;" />
            </v-chip>

            <v-btn
                icon
                size="x-small"
                variant="text"
                @click.stop="$emit('delete')"
                class="grid-component-action-btn"
            >
                <i class="fas fa-times" style="font-size: 10px; color: #CCC;" />
            </v-btn>
        </div>

        <!-- Preview body -->
        <div class="grid-component-body">
            <div v-if="previewText" class="grid-component-preview">
                {{ previewText }}
            </div>
            <div v-else class="grid-component-preview grid-component-preview--empty">
                Click to configure
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import {
    ReportComponent,
    componentTypeInfo,
    AssetDetailConfig,
    TreatmentTimelineConfig,
    WorkSummaryConfig,
    FundedTreatmentListConfig,
    ConditionTrackingConfig,
    GoalTrackingConfig,
    TextBlockConfig,
} from '@/shared/models/iAM/report-schema';

const props = defineProps<{
    component: ReportComponent;
    selected: boolean;
}>();

defineEmits<{
    (e: 'select'): void;
    (e: 'delete'): void;
}>();

const typeInfo = computed(() => componentTypeInfo[props.component.type]);
const hasFilter = computed(() => !!props.component.criteriaExpression && props.component.criteriaExpression.trim().length > 0);

const previewText = computed(() => {
    const config = props.component.config;
    const type = props.component.type;

    switch (type) {
        case 'assetDetail': {
            const c = config as AssetDetailConfig;
            const colCount = (c.attributeColumns?.length || 0) + (c.simulationColumns?.length || 0);
            const scope = c.timeScope === 'initial' ? 'Initial State' : c.timeScope === 'year' ? `Year ${c.selectedYear || '?'}` : 'All Years';
            return `${colCount} column${colCount !== 1 ? 's' : ''} · ${scope}${c.showTotals ? ' · Totals' : ''}`;
        }
        case 'treatmentTimeline': {
            const c = config as TreatmentTimelineConfig;
            const attrCols = c.attributeColumns?.length || 0;
            const yearFields = c.yearDataFields?.length || 0;
            return `${attrCols} attribute column${attrCols !== 1 ? 's' : ''} · ${yearFields} year field${yearFields !== 1 ? 's' : ''}`;
        }
        case 'workSummary': {
            const c = config as WorkSummaryConfig;
            return `Group by: ${c.groupByAttribute || '(none)'} · ${c.aggregateFunction}(${c.aggregateField})`;
        }
        case 'budgetSummary':
            return 'Budget utilization per year';
        case 'conditionTracking': {
            const c = config as ConditionTrackingConfig;
            const count = c.conditionAttributes?.length || 0;
            return `${count} condition attribute${count !== 1 ? 's' : ''}${c.showInitialState ? ' · Initial state' : ''}`;
        }
        case 'fundedTreatmentList': {
            const c = config as FundedTreatmentListConfig;
            const colCount = (c.columns?.length || 0) + (c.simulationColumns?.length || 0);
            return `${colCount} column${colCount !== 1 ? 's' : ''}`;
        }
        case 'goalTracking': {
            const c = config as GoalTrackingConfig;
            return `${c.goalType === 'target' ? 'Target' : 'Deficient'} goals`;
        }
        case 'textBlock': {
            const c = config as TextBlockConfig;
            const preview = c.content ? c.content.substring(0, 60) + (c.content.length > 60 ? '...' : '') : '(empty)';
            return `${c.style} — ${preview}`;
        }
        default:
            return '';
    }
});
</script>

<style scoped>
.grid-component-wrapper {
    display: flex;
    flex-direction: column;
    height: 100%;
    background: #FFFFFF;
    border: 1.5px solid #E8E8E8;
    border-radius: 10px;
    overflow: hidden;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.grid-component-wrapper:hover {
    border-color: #D0D5DD;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.grid-component-wrapper--selected {
    border-color: #2A578D;
    box-shadow: 0 0 0 1px #2A578D, 0 4px 12px rgba(42, 87, 141, 0.12);
}

.grid-component-wrapper--hidden {
    opacity: 0.5;
}

.grid-component-header {
    display: flex;
    align-items: center;
    padding: 8px 10px;
    border-bottom: 1px solid #F0F0F0;
    flex-shrink: 0;
    cursor: grab;
    user-select: none;
}

.grid-component-header:active {
    cursor: grabbing;
}

.grid-component-title {
    font-size: 12px;
    font-weight: 500;
    color: #333;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    flex: 1;
    min-width: 0;
}

.grid-component-action-btn {
    opacity: 0.3;
    transition: opacity 0.15s ease;
    flex-shrink: 0;
}

.grid-component-wrapper:hover .grid-component-action-btn {
    opacity: 1;
}

.grid-component-body {
    flex: 1;
    padding: 8px 10px;
    overflow: hidden;
    display: flex;
    align-items: flex-start;
}

.grid-component-preview {
    font-size: 11px;
    color: #888;
    padding: 4px 8px;
    background: #FAFBFC;
    border-radius: 6px;
    border: 1px dashed #E8E8E8;
    width: 100%;
}

.grid-component-preview--empty {
    color: #CCC;
    font-style: italic;
}
</style>
