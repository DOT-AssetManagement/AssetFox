<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Asset Attribute Columns</div>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showAttrPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Attributes
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

        <AttributeColumnPicker
            v-model="showAttrPicker"
            :existing-attribute-names="localConfig.columns.map(c => c.attributeName)"
            @save="onAttrPickerSave"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import { FundedTreatmentListConfig, ColumnDef, ReportAttributeOption } from '@/shared/models/iAM/report-schema';
import AttributeColumnPicker from '../shared/AttributeColumnPicker.vue';
import SimulationFieldPicker from '../shared/SimulationFieldPicker.vue';
import ColumnChipList from '../shared/ColumnChipList.vue';

const props = defineProps<{ config: FundedTreatmentListConfig }>();
const emit = defineEmits<{ (e: 'update:config', val: FundedTreatmentListConfig): void }>();

const localConfig = ref<FundedTreatmentListConfig>(clone(props.config));
const showAttrPicker = ref(false);

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

function onAttrPickerSave(attributes: ReportAttributeOption[]) {
    const existing = new Map(localConfig.value.columns.map(c => [c.attributeName, c]));
    localConfig.value.columns = attributes.map(attribute =>
        existing.get(attribute.name) || {
            attributeName: attribute.name,
            displayHeader: attribute.name,
            format: attribute.type === 'NUMBER' ? 'number' : 'text',
            width: 12,
        } as ColumnDef
    );
    emitUpdate();
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
