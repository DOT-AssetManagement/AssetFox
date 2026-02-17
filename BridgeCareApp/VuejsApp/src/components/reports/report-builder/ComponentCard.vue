<template>
    <div
        class="component-card"
        :class="{ 'component-card--selected': selected, 'component-card--hidden': !component.visible }"
        @click.stop="$emit('select')"
    >
        <!-- Header -->
        <div class="component-card__header">
            <i class="fas fa-grip-vertical drag-handle" style="color: #CCC; cursor: grab; font-size: 11px; margin-right: 8px;" />

            <v-chip
                size="x-small"
                variant="flat"
                :style="{ backgroundColor: typeInfo.chipColor, color: typeInfo.color }"
                class="mr-2"
            >
                <i :class="typeInfo.icon" style="font-size: 9px;" class="mr-1" />
                {{ typeInfo.label }}
            </v-chip>

            <span class="component-card__title flex-grow-1">{{ component.title }}</span>

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
                @click.stop="expanded = !expanded"
                class="component-card__action-btn"
            >
                <i :class="expanded ? 'fas fa-chevron-up' : 'fas fa-chevron-down'" style="font-size: 10px; color: #999;" />
            </v-btn>

            <v-btn
                icon
                size="x-small"
                variant="text"
                @click.stop="onDelete"
                class="component-card__action-btn"
            >
                <i class="fas fa-times" style="font-size: 10px; color: #CCC;" />
            </v-btn>
        </div>

        <!-- Expanded preview -->
        <v-expand-transition>
            <div v-show="expanded" class="component-card__body">
                <div v-if="previewText" class="component-card__preview">
                    {{ previewText }}
                </div>
            </div>
        </v-expand-transition>
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
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

const emit = defineEmits<{
    (e: 'select'): void;
    (e: 'delete'): void;
}>();

const expanded = ref(false);

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
            const idCols = c.identifierColumns?.length || 0;
            const yearFields = c.yearDataFields?.length || 0;
            return `${idCols} ID column${idCols !== 1 ? 's' : ''} · ${yearFields} year field${yearFields !== 1 ? 's' : ''}`;
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

function onDelete() {
    emit('delete');
}
</script>

<style scoped>
.component-card {
    background: #FFFFFF;
    border: 1.5px solid #E8E8E8;
    border-radius: 10px;
    transition: all 0.2s ease;
    overflow: hidden;
}

.component-card:hover {
    border-color: #D0D5DD;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.component-card--selected {
    border-color: #2A578D;
    box-shadow: 0 0 0 1px #2A578D, 0 4px 12px rgba(42, 87, 141, 0.12);
}

.component-card--hidden {
    opacity: 0.5;
}

.component-card__header {
    display: flex;
    align-items: center;
    padding: 10px 12px;
    min-height: 42px;
}

.component-card__title {
    font-size: 13px;
    font-weight: 500;
    color: #333;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    min-width: 0;
}

.component-card__action-btn {
    opacity: 0.4;
    transition: opacity 0.15s ease;
}

.component-card:hover .component-card__action-btn {
    opacity: 1;
}

.component-card__body {
    padding: 0 12px 10px 12px;
}

.component-card__preview {
    font-size: 11px;
    color: #888;
    padding: 6px 10px;
    background: #FAFBFC;
    border-radius: 6px;
    border: 1px dashed #E8E8E8;
    margin-left: 20px;
}
</style>
