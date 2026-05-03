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

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Identifier Columns</div>
        <p class="text-caption text-grey mb-1">Static asset ID columns shown on the left side of the timeline.</p>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showIdPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Identifiers
        </v-btn>
        <ColumnChipList
            :columns="localConfig.identifierColumns"
            chip-color="#2E7D32"
            @update:columns="(cols) => { localConfig.identifierColumns = cols; emitUpdate(); }"
        />

        <v-divider class="my-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Static Attribute Columns</div>
        <p class="text-caption text-grey mb-1">Additional asset attributes shown alongside identifiers.</p>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showAttrPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Attributes
        </v-btn>
        <ColumnChipList
            :columns="localConfig.attributeColumns"
            chip-color="#1565C0"
            @update:columns="(cols) => { localConfig.attributeColumns = cols; emitUpdate(); }"
        />

        <v-divider class="my-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Per-Year Data Fields</div>
        <p class="text-caption text-grey mb-1">What to show for each simulation year.</p>
        <div v-for="(field, index) in localConfig.yearDataFields" :key="index" class="d-flex align-center mb-2 ga-2">
            <v-select
                v-model="field.type"
                :items="yearFieldTypeOptions"
                variant="outlined"
                density="compact"
                hide-details
                style="max-width: 130px;"
                @update:model-value="emitUpdate"
            />
            <v-text-field
                v-if="field.type === 'attribute'"
                v-model="field.attributeName"
                placeholder="Attribute name"
                variant="outlined"
                density="compact"
                hide-details
                style="max-width: 160px;"
                @update:model-value="emitUpdate"
            />
            <v-text-field
                v-model="field.displayHeader"
                placeholder="Header"
                variant="outlined"
                density="compact"
                hide-details
                style="max-width: 120px;"
                @update:model-value="emitUpdate"
            />
            <v-btn icon size="x-small" variant="text" @click="removeYearField(index)">
                <i class="fas fa-times" style="font-size: 10px; color: #999;" />
            </v-btn>
        </div>
        <v-btn size="small" variant="text" class="text-none" @click="addYearField">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Year Field
        </v-btn>

        <AttributeColumnPicker
            v-model="showIdPicker"
            :existing-attribute-names="localConfig.identifierColumns.map(c => c.attributeName)"
            @save="onIdPickerSave"
        />

        <AttributeColumnPicker
            v-model="showAttrPicker"
            :existing-attribute-names="localConfig.attributeColumns.map(c => c.attributeName)"
            @save="onAttrPickerSave"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import { TreatmentTimelineConfig, ColumnDef, YearField, emptyYearField } from '@/shared/models/iAM/report-schema';
import AttributeColumnPicker from '../shared/AttributeColumnPicker.vue';
import ColumnChipList from '../shared/ColumnChipList.vue';

const props = defineProps<{ config: TreatmentTimelineConfig }>();
const emit = defineEmits<{ (e: 'update:config', val: TreatmentTimelineConfig): void }>();

const localConfig = ref<TreatmentTimelineConfig>(clone(props.config));
const showIdPicker = ref(false);
const showAttrPicker = ref(false);

const yearFieldTypeOptions = [
    { title: 'Treatment', value: 'treatment' },
    { title: 'Cost', value: 'cost' },
    { title: 'Attribute', value: 'attribute' },
];

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

function onIdPickerSave(names: string[]) {
    const existing = new Map(localConfig.value.identifierColumns.map(c => [c.attributeName, c]));
    localConfig.value.identifierColumns = names.map(name =>
        existing.get(name) || { attributeName: name, displayHeader: name, format: 'text', width: 12 } as ColumnDef
    );
    emitUpdate();
}

function onAttrPickerSave(names: string[]) {
    const existing = new Map(localConfig.value.attributeColumns.map(c => [c.attributeName, c]));
    localConfig.value.attributeColumns = names.map(name =>
        existing.get(name) || { attributeName: name, displayHeader: name, format: 'text', width: 12 } as ColumnDef
    );
    emitUpdate();
}

function addYearField() {
    localConfig.value.yearDataFields.push(clone(emptyYearField));
    emitUpdate();
}

function removeYearField(index: number) {
    localConfig.value.yearDataFields.splice(index, 1);
    emitUpdate();
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
