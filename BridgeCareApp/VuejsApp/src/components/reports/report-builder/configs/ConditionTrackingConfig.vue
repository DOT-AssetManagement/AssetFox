<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Identifier Columns</div>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showIdPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Add Identifiers
        </v-btn>
        <ColumnChipList
            :columns="localConfig.identifierColumns"
            chip-color="#00838F"
            @update:columns="(cols) => { localConfig.identifierColumns = cols; emitUpdate(); }"
        />

        <v-divider class="my-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Condition Attributes to Track</div>
        <p class="text-caption text-grey mb-2">Select numeric attributes whose values will be shown for each simulation year.</p>
        <v-btn size="small" variant="outlined" class="text-none mb-2" @click="showCondPicker = true">
            <i class="fas fa-plus mr-2" style="font-size: 10px;" />
            Select Condition Attributes
        </v-btn>
        <div class="d-flex flex-wrap ga-1 mb-2">
            <v-chip
                v-for="(attr, i) in localConfig.conditionAttributes"
                :key="attr"
                size="small"
                variant="tonal"
                color="#00838F"
                closable
                @click:close="removeCondAttr(i)"
            >{{ attr }}</v-chip>
        </div>

        <v-divider class="my-3" />

        <v-checkbox
            v-model="localConfig.showInitialState"
            label="Include initial state column"
            density="compact"
            hide-details
            @update:model-value="emitUpdate"
        />

        <AttributeColumnPicker
            v-model="showIdPicker"
            :existing-attribute-names="localConfig.identifierColumns.map(c => c.attributeName)"
            @save="onIdPickerSave"
        />

        <AttributeColumnPicker
            v-model="showCondPicker"
            :existing-attribute-names="localConfig.conditionAttributes"
            @save="onCondPickerSave"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import { ConditionTrackingConfig, ColumnDef } from '@/shared/models/iAM/report-schema';
import AttributeColumnPicker from '../shared/AttributeColumnPicker.vue';
import ColumnChipList from '../shared/ColumnChipList.vue';

const props = defineProps<{ config: ConditionTrackingConfig }>();
const emit = defineEmits<{ (e: 'update:config', val: ConditionTrackingConfig): void }>();

const localConfig = ref<ConditionTrackingConfig>(clone(props.config));
const showIdPicker = ref(false);
const showCondPicker = ref(false);

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

function onIdPickerSave(names: string[]) {
    const existing = new Map(localConfig.value.identifierColumns.map(c => [c.attributeName, c]));
    localConfig.value.identifierColumns = names.map(name =>
        existing.get(name) || { attributeName: name, displayHeader: name, format: 'text', width: 12 } as ColumnDef
    );
    emitUpdate();
}

function onCondPickerSave(names: string[]) {
    localConfig.value.conditionAttributes = names;
    emitUpdate();
}

function removeCondAttr(index: number) {
    localConfig.value.conditionAttributes.splice(index, 1);
    emitUpdate();
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
