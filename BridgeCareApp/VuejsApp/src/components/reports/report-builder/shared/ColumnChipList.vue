<template>
    <div>
        <draggable
            :list="localColumns"
            item-key="attributeName"
            handle=".chip-drag-handle"
            :animation="200"
            ghost-class="chip-ghost"
            @change="onOrderChange"
        >
            <template #item="{ element, index }">
                <div class="d-flex align-center mb-1 chip-row pa-1 rounded" style="min-height: 36px;">
                    <i class="fas fa-grip-vertical chip-drag-handle mr-2" style="color: #CCC; cursor: grab; font-size: 10px;" />
                    <v-chip
                        size="small"
                        variant="tonal"
                        :color="chipColor"
                        class="mr-2"
                        style="min-width: 0;"
                    >
                        <span class="text-truncate" style="max-width: 120px;">{{ element.displayHeader || element.attributeName || element.field }}</span>
                    </v-chip>
                    <v-select
                        v-model="element.format"
                        :items="formatOptions"
                        variant="plain"
                        density="compact"
                        hide-details
                        style="max-width: 90px; font-size: 12px;"
                        @update:model-value="emitUpdate"
                    />
                    <v-text-field
                        v-model="element.displayHeader"
                        placeholder="Header"
                        variant="plain"
                        density="compact"
                        hide-details
                        style="max-width: 120px; font-size: 12px;"
                        @update:model-value="emitUpdate"
                    />
                    <v-btn icon size="x-small" variant="text" @click="removeColumn(index)" class="ml-auto">
                        <i class="fas fa-times" style="font-size: 10px; color: #999;" />
                    </v-btn>
                </div>
            </template>
        </draggable>
        <div v-if="localColumns.length === 0" class="text-caption text-grey pa-2 text-center" style="font-style: italic;">
            No columns selected
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import draggable from 'vuedraggable';
import { formatOptions } from '@/shared/models/iAM/report-schema';

const props = defineProps<{
    columns: any[];
    chipColor?: string;
}>();

const emit = defineEmits<{
    (e: 'update:columns', val: any[]): void;
}>();

const localColumns = ref<any[]>(clone(props.columns));

watch(() => props.columns, (val) => {
    localColumns.value = clone(val);
}, { deep: true });

function onOrderChange() {
    emitUpdate();
}

function removeColumn(index: number) {
    localColumns.value.splice(index, 1);
    emitUpdate();
}

function emitUpdate() {
    emit('update:columns', clone(localColumns.value));
}
</script>

<style scoped>
.chip-row:hover {
    background-color: #F8F9FA;
}
.chip-ghost {
    opacity: 0.4;
    border: 2px dashed #2A578D;
    border-radius: 6px;
}
</style>
