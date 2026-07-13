<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Static Fields</div>
        <p class="text-caption text-grey mb-1">Asset fields shown to the left of the per-year condition columns. The asset key column is added automatically.</p>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showAttrPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Fields
        </v-btn>
        <ColumnChipList
            :columns="localConfig.attributeColumns"
            chip-color="#00838F"
            @update:columns="(cols) => { localConfig.attributeColumns = cols; emitUpdate(); }"
        />

        <v-divider class="my-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Condition Fields to Track</div>
        <p class="text-caption text-grey mb-2">Select attributes or calculated fields whose values will be shown for each simulation year.</p>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showCondPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Select Condition Fields
        </v-btn>
        <ColumnChipList
            :columns="localConfig.conditionFields || []"
            chip-color="#00838F"
            @update:columns="onConditionFieldsUpdate"
        />

        <v-divider class="my-3" />

        <v-checkbox
            v-model="localConfig.showInitialState"
            label="Include initial state column"
            density="compact"
            hide-details
            @update:model-value="emitUpdate"
        />

        <ReportFieldPicker
            v-model="showAttrPicker"
            :existing-field-keys="localConfig.attributeColumns.map(reportColumnKey)"
            :calculated-fields="calculatedFields"
            @save="onAttrPickerSave"
        />

        <ReportFieldPicker
            v-model="showCondPicker"
            :existing-field-keys="(localConfig.conditionFields || []).map(reportColumnKey)"
            :calculated-fields="calculatedFields"
            @save="onCondPickerSave"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import {
    ConditionTrackingConfig,
    ColumnDef,
    ReportAttributeOption,
    ReportCalculatedField,
    columnDefFromReportField,
    reportColumnKey,
    reportFieldOptionKey,
} from '@/shared/models/iAM/report-schema';
import ReportFieldPicker from '../shared/ReportFieldPicker.vue';
import ColumnChipList from '../shared/ColumnChipList.vue';

const props = defineProps<{ config: ConditionTrackingConfig; calculatedFields: ReportCalculatedField[] }>();
const emit = defineEmits<{ (e: 'update:config', val: ConditionTrackingConfig): void }>();

const localConfig = ref<ConditionTrackingConfig>(normalizeConfig(props.config));
const showAttrPicker = ref(false);
const showCondPicker = ref(false);

watch(() => props.config, (val) => { localConfig.value = normalizeConfig(val); }, { deep: true });

function onAttrPickerSave(attributes: ReportAttributeOption[]) {
    const existing = new Map(localConfig.value.attributeColumns.map(c => [reportColumnKey(c), c]));
    localConfig.value.attributeColumns = attributes.map(attribute =>
        existing.get(reportFieldOptionKey(attribute)) || columnDefFromReportField(attribute)
    );
    emitUpdate();
}

function onCondPickerSave(attributes: ReportAttributeOption[]) {
    const existing = new Map((localConfig.value.conditionFields || []).map(column => [reportColumnKey(column), column]));
    localConfig.value.conditionFields = attributes.map(attribute =>
        existing.get(reportFieldOptionKey(attribute)) || columnDefFromReportField(attribute)
    );
    localConfig.value.conditionAttributes = localConfig.value.conditionFields
        .filter(column => column.source !== 'calculatedField')
        .map(column => column.attributeName);
    emitUpdate();
}

function onConditionFieldsUpdate(fields: ColumnDef[]) {
    localConfig.value.conditionFields = fields;
    localConfig.value.conditionAttributes = fields
        .filter(column => column.source !== 'calculatedField')
        .map(column => column.attributeName);
    emitUpdate();
}

function normalizeConfig(config: ConditionTrackingConfig): ConditionTrackingConfig {
    const normalized = clone(config);
    normalized.conditionFields ||= (normalized.conditionAttributes || []).map(attributeName => ({
        source: 'attribute',
        fieldId: attributeName,
        attributeName,
        displayHeader: attributeName,
        format: 'number',
        width: 12,
    }));
    return normalized;
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
