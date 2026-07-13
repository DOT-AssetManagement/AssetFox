<template>
    <v-dialog v-model="showDialog" max-width="600" persistent>
        <v-card class="rounded-lg">
            <v-card-title class="d-flex align-center pa-4 pb-2">
                <span class="text-subtitle-1 font-weight-medium">Select Report Fields</span>
                <v-spacer />
                <v-btn icon size="small" variant="text" @click="onCancel">
                    <i class="fas fa-times" />
                </v-btn>
            </v-card-title>

            <v-card-text class="pa-4 pt-2">
                <v-text-field
                    v-model="searchTerm"
                    placeholder="Search attributes and calculated fields..."
                    variant="outlined"
                    density="compact"
                    hide-details
                    clearable
                    class="mb-3"
                >
                    <template #prepend-inner>
                        <i class="fas fa-search" style="color: #999; font-size: 12px;" />
                    </template>
                </v-text-field>

                <div class="d-flex align-center mb-2 ga-2">
                    <v-chip
                        v-for="filter in sourceFilters"
                        :key="filter.value"
                        size="small"
                        :variant="sourceFilter === filter.value ? 'flat' : 'outlined'"
                        :color="sourceFilter === filter.value ? '#2A578D' : undefined"
                        @click="sourceFilter = filter.value"
                    >{{ filter.title }}</v-chip>
                    <v-spacer />
                    <span class="text-caption text-grey">{{ selectedKeys.size }} selected</span>
                </div>

                <v-virtual-scroll :items="filteredFields" :height="360" item-height="44">
                    <template #default="{ item }">
                        <div
                            class="d-flex align-center px-2 field-row"
                            :class="{ 'field-row--selected': selectedKeys.has(fieldKey(item)) }"
                            style="height: 44px; cursor: pointer; border-radius: 6px;"
                            @click="toggleField(item)"
                        >
                            <v-checkbox
                                :model-value="selectedKeys.has(fieldKey(item))"
                                hide-details
                                density="compact"
                                class="flex-grow-0 mr-2"
                                @click.stop="toggleField(item)"
                            />
                            <span class="text-body-2 flex-grow-1 text-truncate">{{ item.name }}</span>
                            <v-chip
                                v-if="item.source === 'calculatedField'"
                                size="x-small"
                                variant="tonal"
                                color="#7A4E9D"
                                class="ml-2"
                            >CALC</v-chip>
                            <v-chip
                                size="x-small"
                                variant="tonal"
                                :color="item.type === 'NUMBER' ? '#1565C0' : '#558B2F'"
                                class="ml-1"
                            >{{ item.type === 'NUMBER' ? 'NUM' : 'TXT' }}</v-chip>
                        </div>
                    </template>
                </v-virtual-scroll>
            </v-card-text>

            <v-card-actions class="pa-4 pt-0">
                <v-btn variant="outlined" class="text-none" @click="onCancel">Cancel</v-btn>
                <v-spacer />
                <v-btn color="#2A578D" variant="flat" class="text-none" @click="onSave">
                    Use {{ selectedKeys.size }} Field{{ selectedKeys.size !== 1 ? 's' : '' }}
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useAttributeStore } from '@/stores';
import {
    ReportAttributeOption,
    ReportCalculatedField,
} from '@/shared/models/iAM/report-schema';

const props = withDefaults(defineProps<{
    modelValue: boolean;
    existingFieldKeys: string[];
    calculatedFields: ReportCalculatedField[];
    allowedTypes?: Array<'NUMBER' | 'STRING'>;
}>(), {
    allowedTypes: () => ['NUMBER', 'STRING'],
});

const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
    (e: 'save', fields: ReportAttributeOption[]): void;
}>();

const attributeStore = useAttributeStore();
const searchTerm = ref('');
const sourceFilter = ref<'all' | 'attribute' | 'calculatedField'>('all');
const selectedKeys = ref<Set<string>>(new Set());

const sourceFilters = [
    { title: 'All', value: 'all' as const },
    { title: 'Attributes', value: 'attribute' as const },
    { title: 'Calculated', value: 'calculatedField' as const },
];

const showDialog = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

const allFields = computed<ReportAttributeOption[]>(() => {
    const attributes = (attributeStore.attributesgetter ?? []).map((attribute: any) => ({
        source: 'attribute' as const,
        fieldId: attribute.id || attribute.name,
        name: attribute.name,
        type: attribute.type === 'NUMBER' ? 'NUMBER' : 'STRING',
    }));
    const calculated = props.calculatedFields.map(field => ({
        source: 'calculatedField' as const,
        fieldId: field.id,
        name: field.name,
        type: field.resultType === 'number' ? 'NUMBER' : 'STRING',
    }));
    return [...attributes, ...calculated]
        .filter(field => props.allowedTypes.includes(field.type as 'NUMBER' | 'STRING'))
        .sort((left, right) => left.name.localeCompare(right.name));
});

const filteredFields = computed(() => {
    const term = searchTerm.value.trim().toLowerCase();
    return allFields.value.filter(field =>
        (sourceFilter.value === 'all' || field.source === sourceFilter.value)
        && (!term || field.name.toLowerCase().includes(term))
    );
});

watch(() => props.modelValue, open => {
    if (!open) return;
    const fieldsByLegacyKey = new Map(allFields.value.map(field => [
        field.source === 'attribute' ? `attribute:${field.name}` : fieldKey(field),
        fieldKey(field),
    ]));
    selectedKeys.value = new Set(props.existingFieldKeys.map(key => fieldsByLegacyKey.get(key) || key));
    searchTerm.value = '';
    sourceFilter.value = 'all';
});

function fieldKey(field: ReportAttributeOption): string {
    return `${field.source}:${field.fieldId}`;
}

function toggleField(field: ReportAttributeOption) {
    const key = fieldKey(field);
    const next = new Set(selectedKeys.value);
    next.has(key) ? next.delete(key) : next.add(key);
    selectedKeys.value = next;
}

function onSave() {
    const fieldsByKey = new Map(allFields.value.map(field => [fieldKey(field), field]));
    const selected = Array.from(selectedKeys.value)
        .map(key => fieldsByKey.get(key))
        .filter((field): field is ReportAttributeOption => !!field);
    emit('save', selected);
    showDialog.value = false;
}

function onCancel() {
    showDialog.value = false;
}
</script>

<style scoped>
.field-row:hover { background-color: #F5F5F5; }
.field-row--selected { background-color: #E3F2FD; }
</style>
