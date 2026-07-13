<template>
    <v-dialog v-model="showDialog" max-width="560" persistent>
        <v-card class="rounded-lg">
            <v-card-title class="d-flex align-center pa-4 pb-2">
                <span class="text-subtitle-1 font-weight-medium">Select Attribute Columns</span>
                <v-spacer />
                <v-btn icon size="small" variant="text" @click="onCancel">
                    <i class="fas fa-times" />
                </v-btn>
            </v-card-title>

            <v-card-text class="pa-4 pt-2">
                <v-text-field
                    v-model="searchTerm"
                    placeholder="Search attributes..."
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
                        size="small"
                        :variant="typeFilter === 'all' ? 'flat' : 'outlined'"
                        :color="typeFilter === 'all' ? '#2A578D' : undefined"
                        @click="typeFilter = 'all'"
                    >All</v-chip>
                    <v-chip
                        size="small"
                        :variant="typeFilter === 'NUMBER' ? 'flat' : 'outlined'"
                        :color="typeFilter === 'NUMBER' ? '#2A578D' : undefined"
                        @click="typeFilter = 'NUMBER'"
                    >Numeric</v-chip>
                    <v-chip
                        size="small"
                        :variant="typeFilter === 'STRING' ? 'flat' : 'outlined'"
                        :color="typeFilter === 'STRING' ? '#2A578D' : undefined"
                        @click="typeFilter = 'STRING'"
                    >Text</v-chip>
                    <v-spacer />
                    <span class="text-caption text-grey">{{ selectedNames.size }} selected</span>
                </div>

                <v-virtual-scroll
                    :items="filteredAttributes"
                    :height="360"
                    item-height="40"
                >
                    <template #default="{ item }">
                        <div
                            class="d-flex align-center px-2 attr-row"
                            :class="{ 'attr-row-selected': selectedNames.has(item.name) }"
                            style="height: 40px; cursor: pointer; border-radius: 6px;"
                            @click="toggleAttribute(item.name)"
                        >
                            <v-checkbox
                                :model-value="selectedNames.has(item.name)"
                                hide-details
                                density="compact"
                                class="flex-grow-0 mr-2"
                                @click.stop="toggleAttribute(item.name)"
                            />
                            <span class="text-body-2 flex-grow-1" style="line-height: 1.3;">{{ item.name }}</span>
                            <v-chip
                                size="x-small"
                                variant="tonal"
                                :color="item.type === 'NUMBER' ? '#1565C0' : '#558B2F'"
                                class="ml-2"
                            >{{ item.type === 'NUMBER' ? 'NUM' : 'TXT' }}</v-chip>
                        </div>
                    </template>
                </v-virtual-scroll>
            </v-card-text>

            <v-card-actions class="pa-4 pt-0">
                <v-btn variant="outlined" @click="onCancel" class="text-none">Cancel</v-btn>
                <v-spacer />
                <v-btn color="#2A578D" variant="flat" @click="onSave" class="text-none">
                    Add {{ selectedNames.size }} Column{{ selectedNames.size !== 1 ? 's' : '' }}
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useAttributeStore } from '@/stores';
import { ReportAttributeOption } from '@/shared/models/iAM/report-schema';

const props = defineProps<{
    modelValue: boolean;
    existingAttributeNames: string[];
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', val: boolean): void;
    (e: 'save', attributes: ReportAttributeOption[]): void;
}>();

const attributeStore = useAttributeStore();
const searchTerm = ref('');
const typeFilter = ref<'all' | 'NUMBER' | 'STRING'>('all');
const selectedNames = ref<Set<string>>(new Set());

const showDialog = computed({
    get: () => props.modelValue,
    set: (val) => emit('update:modelValue', val),
});

const allAttributes = computed<ReportAttributeOption[]>(() => {
    const attrs = attributeStore.attributesgetter ?? [];
    return attrs.map((a: any) => ({ name: a.name, type: a.type }));
});

const filteredAttributes = computed(() => {
    let list = allAttributes.value;
    if (typeFilter.value !== 'all') {
        list = list.filter(a => a.type === typeFilter.value);
    }
    if (searchTerm.value) {
        const term = searchTerm.value.toLowerCase();
        list = list.filter(a => a.name.toLowerCase().includes(term));
    }
    return list;
});

watch(() => props.modelValue, (open) => {
    if (open) {
        selectedNames.value = new Set(props.existingAttributeNames);
        searchTerm.value = '';
        typeFilter.value = 'all';
    }
});

function toggleAttribute(name: string) {
    const s = new Set(selectedNames.value);
    if (s.has(name)) {
        s.delete(name);
    } else {
        s.add(name);
    }
    selectedNames.value = s;
}

function onSave() {
    const attributesByName = new Map(allAttributes.value.map(attribute => [attribute.name, attribute]));
    const selectedAttributes = Array.from(selectedNames.value)
        .map(name => attributesByName.get(name))
        .filter((attribute): attribute is ReportAttributeOption => !!attribute);
    emit('save', selectedAttributes);
    showDialog.value = false;
}

function onCancel() {
    showDialog.value = false;
}
</script>

<style scoped>
.attr-row:hover {
    background-color: #F5F5F5;
}
.attr-row-selected {
    background-color: #E3F2FD;
}
</style>
