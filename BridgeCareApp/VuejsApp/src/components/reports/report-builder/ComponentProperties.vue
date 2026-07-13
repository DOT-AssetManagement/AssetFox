<template>
    <div class="component-properties" v-if="component">
        <!-- General -->
        <div class="props-section">
            <div class="props-section__header">
                <i :class="typeInfo.icon" :style="{ color: typeInfo.color }" style="font-size: 11px;" class="mr-2" />
                General
            </div>
            <v-text-field
                v-model="localComponent.title"
                label="Title"
                variant="outlined"
                density="compact"
                :rules="[titleRequiredRule]"
                class="mb-2"
                @update:model-value="emitUpdate"
                @blur="ensureTitle"
            />
            <v-checkbox
                v-if="localComponent.type !== 'textBlock'"
                v-model="localComponent.visible"
                label="Include in excel report"
                density="compact"
                hide-details
                class="mb-1"
                @update:model-value="emitUpdate"
            />
            <v-select
                v-if="localComponent.type !== 'textBlock' && !isChartComponentType(localComponent.type)"
                v-model="localComponent.pageSize"
                :items="pageSizeOptions"
                label="Rows per page"
                variant="outlined"
                density="compact"
                hide-details
                class="mb-2"
                @update:model-value="emitUpdate"
            />
            <v-checkbox
                v-if="localComponent.type !== 'textBlock' && !isChartComponentType(localComponent.type)"
                v-model="localComponent.stripedRows"
                label="Alternating row colors"
                density="compact"
                hide-details
                class="mb-1"
                @update:model-value="emitUpdate"
            />
        </div>

        <v-divider class="my-2" />

        <!-- Configuration (type-specific) -->
        <div class="props-section">
            <div class="props-section__header">
                <i class="fas fa-cog mr-2" style="font-size: 11px; color: #888;" />
                Configuration
            </div>

            <AssetDetailConfigEditor
                v-if="localComponent.type === 'assetDetail'"
                :config="localComponent.config as AssetDetailConfig"
                :calculated-fields="calculatedFields"
                @update:config="onConfigUpdate"
            />
            <TreatmentTimelineConfigEditor
                v-else-if="localComponent.type === 'treatmentTimeline'"
                :config="localComponent.config as TreatmentTimelineConfig"
                :calculated-fields="calculatedFields"
                @update:config="onConfigUpdate"
            />
            <WorkSummaryConfigEditor
                v-else-if="localComponent.type === 'workSummary'"
                :config="localComponent.config as WorkSummaryConfig"
                :calculated-fields="calculatedFields"
                @update:config="onConfigUpdate"
            />
            <BudgetSummaryConfigEditor
                v-else-if="localComponent.type === 'budgetSummary'"
                :config="localComponent.config as BudgetSummaryConfig"
                @update:config="onConfigUpdate"
            />
            <ConditionTrackingConfigEditor
                v-else-if="localComponent.type === 'conditionTracking'"
                :config="localComponent.config as ConditionTrackingConfig"
                :calculated-fields="calculatedFields"
                @update:config="onConfigUpdate"
            />
            <FundedTreatmentListConfigEditor
                v-else-if="localComponent.type === 'fundedTreatmentList'"
                :config="localComponent.config as FundedTreatmentListConfig"
                :calculated-fields="calculatedFields"
                @update:config="onConfigUpdate"
            />
            <TextBlockConfigEditor
                v-else-if="localComponent.type === 'textBlock'"
                :config="localComponent.config as TextBlockConfig"
                @update:config="onConfigUpdate"
            />
            <ChartConfigEditor
                v-else-if="isChartComponentType(localComponent.type)"
                :component-type="localComponent.type"
                :config="localComponent.config as ChartConfig"
                :calculated-fields="calculatedFields"
                @update:config="onConfigUpdate"
            />
        </div>

        <v-divider class="my-2" />

        <!-- Filter -->
        <div class="props-section">
            <div class="props-section__header">
                <i class="fas fa-filter mr-2" style="font-size: 11px; color: #888;" />
                Filter
            </div>
            <ComponentFilterButton
                :model-value="localComponent.criteriaExpression"
                @update:model-value="onFilterUpdate"
            />
        </div>
    </div>

    <!-- Empty state -->
    <div v-else class="properties-empty">
        <i class="fas fa-hand-pointer properties-empty__icon" />
        <div class="properties-empty__text">Select a component to edit its properties</div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { clone } from 'ramda';
import {
    ReportComponent,
    ReportCalculatedField,
    ComponentConfig,
    AssetDetailConfig,
    TreatmentTimelineConfig,
    WorkSummaryConfig,
    BudgetSummaryConfig,
    ConditionTrackingConfig,
    FundedTreatmentListConfig,
    TextBlockConfig,
    ChartConfig,
    componentTypeInfo,
    isChartComponentType,
    pageSizeOptions,
} from '@/shared/models/iAM/report-schema';
import ComponentFilterButton from './shared/ComponentFilterButton.vue';
import AssetDetailConfigEditor from './configs/AssetDetailConfig.vue';
import TreatmentTimelineConfigEditor from './configs/TreatmentTimelineConfig.vue';
import WorkSummaryConfigEditor from './configs/WorkSummaryConfig.vue';
import BudgetSummaryConfigEditor from './configs/BudgetSummaryConfig.vue';
import ConditionTrackingConfigEditor from './configs/ConditionTrackingConfig.vue';
import FundedTreatmentListConfigEditor from './configs/FundedTreatmentListConfig.vue';
import TextBlockConfigEditor from './configs/TextBlockConfig.vue';
import ChartConfigEditor from './configs/ChartConfig.vue';

const props = defineProps<{
    component: ReportComponent | null;
    calculatedFields: ReportCalculatedField[];
}>();

const emit = defineEmits<{
    (e: 'update:component', val: ReportComponent): void;
}>();

const localComponent = ref<ReportComponent | null>(null);

watch(() => props.component, (val) => {
    localComponent.value = val ? clone(val) : null;
}, { deep: true, immediate: true });

const typeInfo = computed(() =>
    localComponent.value ? componentTypeInfo[localComponent.value.type] : componentTypeInfo.assetDetail
);

const titleRequiredRule = (value: string) =>
    !!value?.trim() || 'Title is required';

function onConfigUpdate(config: ComponentConfig) {
    if (!localComponent.value) return;
    localComponent.value.config = config;
    emitUpdate();
}

function onFilterUpdate(expression: string) {
    if (!localComponent.value) return;
    localComponent.value.criteriaExpression = expression;
    emitUpdate();
}

function ensureTitle() {
    if (!localComponent.value) return;
    if (!localComponent.value.title?.trim()) {
        localComponent.value.title = componentTypeInfo[localComponent.value.type]?.label || 'Report Component';
        emitUpdate();
    } else {
        localComponent.value.title = localComponent.value.title.trim();
        emitUpdate();
    }
}

function emitUpdate() {
    if (!localComponent.value) return;
    emit('update:component', clone(localComponent.value));
}
</script>

<style scoped>
.component-properties {
    padding: 12px;
}

.props-section {
    margin-bottom: 4px;
}

.props-section__header {
    font-size: 11px;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: #888;
    margin-bottom: 10px;
    display: flex;
    align-items: center;
}

.properties-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    min-height: 300px;
    padding: 24px;
}

.properties-empty__icon {
    font-size: 28px;
    color: #D0D5DD;
    margin-bottom: 12px;
}

.properties-empty__text {
    font-size: 13px;
    color: #B0B0B0;
    text-align: center;
    line-height: 1.5;
}
</style>
