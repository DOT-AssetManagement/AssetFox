<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">
            Simulation Fields
        </div>
        <div
            v-for="field in simulationFieldOptions"
            :key="field.field"
            class="d-flex align-center sim-field-row px-2"
            style="height: 36px; border-radius: 6px; cursor: pointer;"
            @click="toggleField(field)"
        >
            <v-checkbox
                :model-value="isSelected(field.field)"
                hide-details
                density="compact"
                class="flex-grow-0 mr-2"
                @click.stop="toggleField(field)"
            />
            <span class="text-body-2">{{ field.label }}</span>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { clone } from 'ramda';
import {
    SimulationColumnDef,
    SimulationField,
    SimulationFieldInfo,
    simulationFieldOptions,
} from '@/shared/models/iAM/report-schema';

const props = defineProps<{
    modelValue: SimulationColumnDef[];
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', val: SimulationColumnDef[]): void;
}>();

function isSelected(field: SimulationField): boolean {
    return props.modelValue.some(c => c.field === field);
}

function toggleField(fieldInfo: SimulationFieldInfo) {
    const existing = props.modelValue.find(c => c.field === fieldInfo.field);
    if (existing) {
        emit('update:modelValue', props.modelValue.filter(c => c.field !== fieldInfo.field));
    } else {
        const newCol: SimulationColumnDef = {
            field: fieldInfo.field,
            displayHeader: fieldInfo.label,
            format: fieldInfo.defaultFormat,
        };
        emit('update:modelValue', [...clone(props.modelValue), newCol]);
    }
}
</script>

<style scoped>
.sim-field-row:hover {
    background-color: #F5F5F5;
}
</style>
