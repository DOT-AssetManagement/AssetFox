<template>
    <div>
        <div class="d-flex align-center ga-2">
            <v-btn
                variant="outlined"
                size="small"
                class="text-none"
                @click="onOpenFilter"
            >
                <i class="fas fa-filter mr-2" style="font-size: 11px;" />
                {{ hasFilter ? 'Edit Filter' : 'Add Filter' }}
            </v-btn>
            <v-chip
                v-if="hasFilter"
                size="small"
                color="#E3F2FD"
                closable
                @click:close="onClearFilter"
            >
                <i class="fas fa-filter mr-1" style="font-size: 9px;" />
                Filtered
            </v-chip>
        </div>

        <div v-if="hasFilter" class="text-caption text-grey mt-1 pa-1" style="font-family: monospace; font-size: 11px; word-break: break-all;">
            {{ truncatedExpression }}
        </div>

        <GeneralCriterionEditorDialog
            :dialogData="filterDialogData"
            @submit="onFilterSubmit"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { isNil } from 'ramda';
import GeneralCriterionEditorDialog from '@/shared/modals/GeneralCriterionEditorDialog.vue';
import { GeneralCriterionEditorDialogData } from '@/shared/models/modals/general-criterion-editor-dialog-data';

const props = defineProps<{
    modelValue: string;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', val: string): void;
}>();

const filterDialogData = ref<GeneralCriterionEditorDialogData>({
    CriteriaExpression: props.modelValue || '',
    showDialog: false,
});

const hasFilter = computed(() => !!props.modelValue && props.modelValue.trim().length > 0);

const truncatedExpression = computed(() => {
    if (!props.modelValue) return '';
    return props.modelValue.length > 100
        ? props.modelValue.substring(0, 100) + '...'
        : props.modelValue;
});

watch(() => props.modelValue, (val) => {
    filterDialogData.value.CriteriaExpression = val || '';
});

function onOpenFilter() {
    filterDialogData.value = {
        CriteriaExpression: props.modelValue || '',
        showDialog: true,
    };
}

function onFilterSubmit(expression: string | null, resultsCount: number | null) {
    filterDialogData.value.showDialog = false;
    // The dialog emits null on Cancel; keep the existing filter.
    if (isNil(expression)) return;
    emit('update:modelValue', expression);
}

function onClearFilter() {
    emit('update:modelValue', '');
}
</script>
