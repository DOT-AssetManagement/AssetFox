<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Asset Report Fields</div>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showAttrPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Fields
        </v-btn>
        <ColumnChipList
            :columns="localConfig.columns"
            chip-color="#BF360C"
            @update:columns="(cols) => { localConfig.columns = cols; emitUpdate(); }"
        />

        <v-divider class="my-3" />

        <SimulationFieldPicker
            v-model="localConfig.simulationColumns"
            @update:model-value="(cols) => { localConfig.simulationColumns = cols; emitUpdate(); }"
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
    FundedTreatmentListConfig,
    ReportAttributeOption,
    ReportCalculatedField,
    columnDefFromReportField,
    reportColumnKey,
    reportFieldOptionKey,
} from '@/shared/models/iAM/report-schema';
import ReportFieldPicker from '../shared/ReportFieldPicker.vue';
import SimulationFieldPicker from '../shared/SimulationFieldPicker.vue';
import ColumnChipList from '../shared/ColumnChipList.vue';

const props = defineProps<{ config: FundedTreatmentListConfig; calculatedFields: ReportCalculatedField[] }>();
const emit = defineEmits<{ (e: 'update:config', val: FundedTreatmentListConfig): void }>();

const localConfig = ref<FundedTreatmentListConfig>(clone(props.config));
const showAttrPicker = ref(false);

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

function onAttrPickerSave(attributes: ReportAttributeOption[]) {
    const existing = new Map(localConfig.value.columns.map(c => [reportColumnKey(c), c]));
    localConfig.value.columns = attributes.map(attribute =>
        existing.get(reportFieldOptionKey(attribute)) || columnDefFromReportField(attribute)
    );
    emitUpdate();
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
