<template>
    <div>
        <div class="text-caption text-uppercase mb-1" style="letter-spacing: 0.08em; color: #666;">
            Unfunded Treatment Fields
        </div>
        <div class="text-caption mb-2" style="color: #999; line-height: 1.35;">
            Analysis year, candidate treatment, and estimated cost are always included.
        </div>
        <div
            v-for="field in unfundedTreatmentFieldOptions"
            :key="field.field"
            class="d-flex align-center unfunded-field-row px-2"
            :class="{ 'unfunded-field-row--required': field.required }"
            @click="toggleField(field)"
        >
            <v-checkbox
                :model-value="isSelected(field.field)"
                :disabled="field.required"
                hide-details
                density="compact"
                class="flex-grow-0 mr-2"
                @click.stop="toggleField(field)"
            />
            <span class="text-body-2">{{ field.label }}</span>
            <i
                v-if="field.required"
                class="fas fa-lock ml-auto"
                style="font-size: 9px; color: #AAA;"
                title="Required field"
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { clone } from 'ramda';
import {
    UnfundedTreatmentColumnDef,
    UnfundedTreatmentField,
    UnfundedTreatmentFieldInfo,
    unfundedTreatmentFieldOptions,
} from '@/shared/models/iAM/report-schema';

const props = defineProps<{
    modelValue: UnfundedTreatmentColumnDef[];
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', val: UnfundedTreatmentColumnDef[]): void;
}>();

function isSelected(field: UnfundedTreatmentField): boolean {
    const info = unfundedTreatmentFieldOptions.find(option => option.field === field);
    return !!info?.required || props.modelValue.some(column => column.field === field);
}

function toggleField(fieldInfo: UnfundedTreatmentFieldInfo) {
    if (fieldInfo.required) return;

    const existing = props.modelValue.find(column => column.field === fieldInfo.field);
    if (existing) {
        emit('update:modelValue', props.modelValue.filter(column => column.field !== fieldInfo.field));
        return;
    }

    const newColumn: UnfundedTreatmentColumnDef = {
        field: fieldInfo.field,
        displayHeader: fieldInfo.label,
        format: fieldInfo.defaultFormat,
    };
    emit('update:modelValue', [...clone(props.modelValue), newColumn]);
}
</script>

<style scoped>
.unfunded-field-row {
    height: 36px;
    border-radius: 6px;
    cursor: pointer;
}

.unfunded-field-row:hover {
    background-color: #F5F5F5;
}

.unfunded-field-row--required {
    cursor: default;
}
</style>
