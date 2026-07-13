<template>
    <div>
        <v-checkbox
            v-model="localConfig.showNoTreatment"
            label='Show "No Treatment"'
            density="compact"
            hide-details
            class="mb-2"
            @update:model-value="emitUpdate"
        />

        <v-divider class="my-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Static Fields</div>
        <p class="text-caption text-grey mb-1">Fields shown after the automatically-added asset key and year columns.</p>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showAttrPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Fields
        </v-btn>
        <ColumnChipList
            :columns="localConfig.attributeColumns"
            chip-color="#1565C0"
            @update:columns="(cols) => { localConfig.attributeColumns = cols; emitUpdate(); }"
        />

        <v-divider class="my-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Per-Year Data Fields</div>
        <p class="text-caption text-grey mb-2">Fields shown after the static attributes for every simulation year.</p>
        <div class="d-flex flex-wrap ga-2 mb-2">
            <v-btn
                size="small"
                variant="outlined"
                class="text-none"
                :disabled="hasYearFieldType('treatment')"
                @click="addBuiltInYearField('treatment')"
            >
                <i class="fas fa-plus mr-2" style="font-size: 10px;" />
                Treatment
            </v-btn>
            <v-btn
                size="small"
                variant="outlined"
                class="text-none"
                :disabled="hasYearFieldType('cost')"
                @click="addBuiltInYearField('cost')"
            >
                <i class="fas fa-plus mr-2" style="font-size: 10px;" />
                Cost
            </v-btn>
            <v-btn size="small" variant="outlined" class="text-none" @click="showYearAttrPicker = true">
                <i class="fas fa-plus mr-2" style="font-size: 10px;" />
                Fields
            </v-btn>
        </div>
        <ColumnChipList
            :columns="localConfig.yearDataFields"
            chip-color="#2E7D32"
            @update:columns="onYearFieldsUpdate"
        />

        <ReportFieldPicker
            v-model="showAttrPicker"
            :existing-field-keys="localConfig.attributeColumns.map(reportColumnKey)"
            :calculated-fields="calculatedFields"
            @save="onAttrPickerSave"
        />
        <ReportFieldPicker
            v-model="showYearAttrPicker"
            :existing-field-keys="yearFieldKeys"
            :calculated-fields="calculatedFields"
            @save="onYearAttrPickerSave"
        />
    </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { clone } from 'ramda';
import {
    TreatmentTimelineConfig,
    ColumnDef,
    ReportAttributeOption,
    ReportCalculatedField,
    YearField,
    columnDefFromReportField,
    reportColumnKey,
    reportFieldOptionKey,
} from '@/shared/models/iAM/report-schema';
import ReportFieldPicker from '../shared/ReportFieldPicker.vue';
import ColumnChipList from '../shared/ColumnChipList.vue';

const props = defineProps<{ config: TreatmentTimelineConfig; calculatedFields: ReportCalculatedField[] }>();
const emit = defineEmits<{ (e: 'update:config', val: TreatmentTimelineConfig): void }>();

const localConfig = ref<TreatmentTimelineConfig>(clone(props.config));
const showAttrPicker = ref(false);
const showYearAttrPicker = ref(false);

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

const calculatedFields = computed(() => props.calculatedFields);
const yearFieldKeys = computed(() => localConfig.value.yearDataFields
    .filter(field => field.type === 'attribute' || field.type === 'calculatedField')
    .map(field => field.type === 'calculatedField'
        ? `calculatedField:${field.fieldId || ''}`
        : `attribute:${field.fieldId || field.attributeName || ''}`));

function onAttrPickerSave(attributes: ReportAttributeOption[]) {
    const existing = new Map(localConfig.value.attributeColumns.map(c => [reportColumnKey(c), c]));
    localConfig.value.attributeColumns = attributes.map(attribute =>
        existing.get(reportFieldOptionKey(attribute)) || columnDefFromReportField(attribute)
    );
    emitUpdate();
}

function onYearAttrPickerSave(attributes: ReportAttributeOption[]) {
    const selectedKeys = new Set(attributes.map(reportFieldOptionKey));
    const retainedFields = localConfig.value.yearDataFields.filter(field =>
        !['attribute', 'calculatedField'].includes(field.type)
        || selectedKeys.has(field.type === 'calculatedField'
            ? `calculatedField:${field.fieldId || ''}`
            : `attribute:${field.fieldId || field.attributeName || ''}`)
    );
    const retainedKeys = new Set(retainedFields
        .filter(field => field.type === 'attribute' || field.type === 'calculatedField')
        .map(field => field.type === 'calculatedField'
            ? `calculatedField:${field.fieldId || ''}`
            : `attribute:${field.fieldId || field.attributeName || ''}`));

    for (const attribute of attributes) {
        const key = reportFieldOptionKey(attribute);
        if (!retainedKeys.has(key)) {
            retainedFields.push({
                type: attribute.source,
                attributeName: attribute.name,
                fieldId: attribute.fieldId,
                displayHeader: attribute.name,
                format: attribute.type === 'NUMBER' ? 'number' : 'text',
            });
        }
    }

    localConfig.value.yearDataFields = retainedFields;
    emitUpdate();
}

function hasYearFieldType(type: 'treatment' | 'cost') {
    return localConfig.value.yearDataFields.some(field => field.type === type);
}

function addBuiltInYearField(type: 'treatment' | 'cost') {
    if (hasYearFieldType(type)) return;
    localConfig.value.yearDataFields.push(type === 'treatment'
        ? { type: 'treatment', displayHeader: 'Treatment', format: 'text' }
        : { type: 'cost', displayHeader: 'Cost', format: 'currency' });
    emitUpdate();
}

function onYearFieldsUpdate(fields: YearField[]) {
    localConfig.value.yearDataFields = fields;
    emitUpdate();
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
