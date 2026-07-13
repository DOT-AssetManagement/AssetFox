<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Data Scope</div>
        <v-select
            v-model="localConfig.timeScope"
            :items="timeScopeOptions"
            variant="outlined"
            density="compact"
            class="mb-3"
            @update:model-value="emitUpdate"
        />
        <v-text-field
            v-if="localConfig.timeScope === 'year'"
            v-model.number="localConfig.selectedYear"
            label="Year"
            type="number"
            variant="outlined"
            density="compact"
            class="mb-3"
            @update:model-value="emitUpdate"
        />

        <v-divider class="mb-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Network Attribute Columns</div>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showAttrPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Attributes
        </v-btn>
        <ColumnChipList
            :columns="localConfig.attributeColumns"
            chip-color="#1565C0"
            @update:columns="onAttributeColumnsUpdate"
        />

        <AttributeColumnPicker
            v-model="showAttrPicker"
            :existing-attribute-names="existingAttrNames"
            @save="onAttrPickerSave"
        />

        <v-divider class="my-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Options</div>
        <v-checkbox
            v-model="localConfig.showTotals"
            label="Show totals row"
            density="compact"
            hide-details
            @update:model-value="emitUpdate"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { clone } from 'ramda';
import {
    AssetDetailConfig,
    ColumnDef,
    ReportAttributeOption,
    timeScopeOptions,
} from '@/shared/models/iAM/report-schema';
import AttributeColumnPicker from '../shared/AttributeColumnPicker.vue';
import ColumnChipList from '../shared/ColumnChipList.vue';

const props = defineProps<{ config: AssetDetailConfig }>();
const emit = defineEmits<{ (e: 'update:config', val: AssetDetailConfig): void }>();

const localConfig = ref<AssetDetailConfig>(clone(props.config));
const showAttrPicker = ref(false);

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

const existingAttrNames = computed(() => localConfig.value.attributeColumns.map(c => c.attributeName));

function onAttrPickerSave(attributes: ReportAttributeOption[]) {
    const existing = new Map(localConfig.value.attributeColumns.map(c => [c.attributeName, c]));
    localConfig.value.attributeColumns = attributes.map(attribute =>
        existing.get(attribute.name) || {
            attributeName: attribute.name,
            displayHeader: attribute.name,
            format: attribute.type === 'NUMBER' ? 'number' : 'text',
            width: 12,
        } as ColumnDef
    );
    emitUpdate();
}

function onAttributeColumnsUpdate(cols: ColumnDef[]) {
    localConfig.value.attributeColumns = cols;
    emitUpdate();
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
