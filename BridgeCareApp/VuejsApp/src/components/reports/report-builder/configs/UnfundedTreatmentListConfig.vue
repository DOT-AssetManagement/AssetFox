<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Asset Report Fields</div>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showAttrPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Fields
        </v-btn>
        <ColumnChipList
            :columns="localConfig.columns"
            chip-color="#8D4B20"
            @update:columns="onColumnsUpdate"
        />

        <v-divider class="my-3" />

        <UnfundedTreatmentFieldPicker
            v-model="localConfig.candidateColumns"
            @update:model-value="onCandidateColumnsUpdate"
        />

        <ReportFieldPicker
            v-model="showAttrPicker"
            :existing-field-keys="localConfig.columns.map(reportColumnKey)"
            :calculated-fields="calculatedFields"
            @save="onAttrPickerSave"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import {
    ColumnDef,
    ReportAttributeOption,
    ReportCalculatedField,
    UnfundedTreatmentColumnDef,
    UnfundedTreatmentListConfig,
    columnDefFromReportField,
    reportColumnKey,
    reportFieldOptionKey,
} from '@/shared/models/iAM/report-schema';
import ReportFieldPicker from '../shared/ReportFieldPicker.vue';
import UnfundedTreatmentFieldPicker from '../shared/UnfundedTreatmentFieldPicker.vue';
import ColumnChipList from '../shared/ColumnChipList.vue';

const props = defineProps<{ config: UnfundedTreatmentListConfig; calculatedFields: ReportCalculatedField[] }>();
const emit = defineEmits<{ (e: 'update:config', val: UnfundedTreatmentListConfig): void }>();

const localConfig = ref<UnfundedTreatmentListConfig>(clone(props.config));
const showAttrPicker = ref(false);

watch(() => props.config, (value) => { localConfig.value = clone(value); }, { deep: true });

function onAttrPickerSave(attributes: ReportAttributeOption[]) {
    const existing = new Map(localConfig.value.columns.map(column => [reportColumnKey(column), column]));
    localConfig.value.columns = attributes.map(attribute =>
        existing.get(reportFieldOptionKey(attribute)) || columnDefFromReportField(attribute)
    );
    emitUpdate();
}

function onColumnsUpdate(columns: ColumnDef[]) {
    localConfig.value.columns = columns;
    emitUpdate();
}

function onCandidateColumnsUpdate(columns: UnfundedTreatmentColumnDef[]) {
    localConfig.value.candidateColumns = columns;
    emitUpdate();
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
