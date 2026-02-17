<template>
    <div class="report-builder">
        <!-- Top bar -->
        <div class="report-builder__topbar">
            <div class="d-flex align-center flex-grow-1 ga-3" style="min-width: 0;">
                <v-text-field
                    v-model="schemaName"
                    placeholder="Report Name"
                    variant="plain"
                    density="compact"
                    hide-details
                    class="report-builder__name-field"
                    @update:model-value="markDirty"
                />
                <v-text-field
                    v-model="schemaDescription"
                    placeholder="Description (optional)"
                    variant="plain"
                    density="compact"
                    hide-details
                    class="report-builder__desc-field"
                    @update:model-value="markDirty"
                />
            </div>
            <div class="d-flex align-center ga-2 flex-shrink-0">
                <v-btn
                    size="small"
                    variant="outlined"
                    class="text-none"
                    @click="onNew"
                >
                    <i class="fas fa-file mr-2" style="font-size: 10px;" />
                    New
                </v-btn>
                <v-btn
                    size="small"
                    variant="outlined"
                    class="text-none"
                    @click="showLoadDialog = true"
                >
                    <i class="fas fa-folder-open mr-2" style="font-size: 10px;" />
                    Load
                </v-btn>
                <v-btn
                    size="small"
                    color="#2A578D"
                    variant="flat"
                    class="text-none"
                    :disabled="!schemaName"
                    :loading="saving"
                    @click="onSave"
                >
                    <i class="fas fa-save mr-2" style="font-size: 10px;" />
                    Save
                    <i v-if="saveSuccess" class="fas fa-check ml-1" style="font-size: 10px;" />
                </v-btn>
            </div>
        </div>

        <!-- Main content -->
        <div class="report-builder__body">
            <!-- Left: Palette -->
            <div class="report-builder__palette">
                <ComponentPalette @add-component="onAddComponent" />
            </div>

            <!-- Center: Canvas -->
            <div class="report-builder__canvas">
                <ReportCanvas
                    :components="components"
                    :selected-id="selectedComponentId"
                    @update:components="onComponentsUpdate"
                    @select="onSelectComponent"
                    @delete="onDeleteComponent"
                    @add-component="onAddComponent"
                />
            </div>

            <!-- Right: Properties -->
            <div class="report-builder__properties">
                <ComponentProperties
                    :component="selectedComponent"
                    @update:component="onComponentUpdate"
                />
            </div>
        </div>

        <!-- Load dialog -->
        <v-dialog v-model="showLoadDialog" max-width="520">
            <v-card class="rounded-lg">
                <v-card-title class="d-flex align-center pa-4">
                    <i class="fas fa-folder-open mr-2" style="font-size: 14px; color: #888;" />
                    <span class="text-subtitle-1 font-weight-medium">Load Report Schema</span>
                    <v-spacer />
                    <v-btn icon size="x-small" variant="text" @click="showLoadDialog = false">
                        <i class="fas fa-times" style="font-size: 12px; color: #999;" />
                    </v-btn>
                </v-card-title>
                <v-divider />
                <v-card-text class="pa-0" style="max-height: 400px; overflow-y: auto;">
                    <v-list v-if="availableSchemas.length > 0" density="compact">
                        <v-list-item
                            v-for="schema in availableSchemas"
                            :key="schema.id"
                            @click="onLoadSchema(schema)"
                            class="load-schema-item"
                        >
                            <template #prepend>
                                <i class="fas fa-file-alt mr-3" style="font-size: 12px; color: #AAA;" />
                            </template>
                            <v-list-item-title class="text-body-2 font-weight-medium">{{ schema.name }}</v-list-item-title>
                            <v-list-item-subtitle class="text-caption">{{ schema.description || 'No description' }}</v-list-item-subtitle>
                        </v-list-item>
                    </v-list>
                    <div v-else class="text-center pa-8">
                        <i class="fas fa-inbox" style="font-size: 28px; color: #D0D5DD;" />
                        <div class="text-caption text-grey mt-2">No saved schemas</div>
                    </div>
                </v-card-text>
            </v-card>
        </v-dialog>

        <!-- Delete confirmation dialog -->
        <v-dialog v-model="showDeleteConfirm" max-width="360">
            <v-card class="rounded-lg pa-4">
                <div class="text-subtitle-1 font-weight-medium mb-2">Remove component?</div>
                <div class="text-body-2 text-grey mb-4">This component and its configuration will be removed from the report.</div>
                <div class="d-flex justify-end ga-2">
                    <v-btn size="small" variant="text" class="text-none" @click="showDeleteConfirm = false">Cancel</v-btn>
                    <v-btn size="small" color="error" variant="flat" class="text-none" @click="confirmDelete">Remove</v-btn>
                </div>
            </v-card>
        </v-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { clone } from 'ramda';
import { useStore } from 'vuex';
import {
    ReportSchemaDTO,
    ReportSchemaDefinition,
    ReportComponent,
    ComponentType,
    emptyReportSchemaDTO,
    emptyPageSettings,
    createDefaultComponent,
} from '@/shared/models/iAM/report-schema';
import ComponentPalette from './ComponentPalette.vue';
import ReportCanvas from './ReportCanvas.vue';
import ComponentProperties from './ComponentProperties.vue';

const store = useStore();

// Schema state
const schemaName = ref('');
const schemaDescription = ref('');
const components = ref<ReportComponent[]>([]);
const currentSchemaId = ref('');

// UI state
const selectedComponentId = ref('');
const showLoadDialog = ref(false);
const showDeleteConfirm = ref(false);
const pendingDeleteId = ref('');
const saving = ref(false);
const saveSuccess = ref(false);

// Computed
const availableSchemas = computed<ReportSchemaDTO[]>(
    () => store.state.reportSchemaModule.reportSchemas
);

const selectedComponent = computed<ReportComponent | null>(() =>
    components.value.find(c => c.id === selectedComponentId.value) || null
);

// Methods
function markDirty() {
    store.commit('isDirtyMutator', true);
}

function onAddComponent(type: ComponentType) {
    const newComponent = createDefaultComponent(type);
    newComponent.order = components.value.length;
    components.value = [...components.value, newComponent];
    selectedComponentId.value = newComponent.id;
    markDirty();
}

function onComponentsUpdate(updated: ReportComponent[]) {
    components.value = updated;
    markDirty();
}

function onSelectComponent(id: string) {
    selectedComponentId.value = id;
}

function onComponentUpdate(updated: ReportComponent) {
    const index = components.value.findIndex(c => c.id === updated.id);
    if (index >= 0) {
        const cloned = clone(components.value);
        cloned[index] = updated;
        components.value = cloned;
        markDirty();
    }
}

function onDeleteComponent(id: string) {
    pendingDeleteId.value = id;
    showDeleteConfirm.value = true;
}

function confirmDelete() {
    components.value = components.value
        .filter(c => c.id !== pendingDeleteId.value)
        .map((c, i) => ({ ...c, order: i }));
    if (selectedComponentId.value === pendingDeleteId.value) {
        selectedComponentId.value = '';
    }
    showDeleteConfirm.value = false;
    pendingDeleteId.value = '';
    markDirty();
}

function buildSchemaJson(): string {
    const definition: ReportSchemaDefinition = {
        schemaVersion: '2.0',
        name: schemaName.value,
        description: schemaDescription.value,
        pageSettings: clone(emptyPageSettings),
        components: components.value.map((c, i) => ({ ...c, order: i })),
    };
    return JSON.stringify(definition);
}

async function onSave() {
    saving.value = true;
    saveSuccess.value = false;

    const now = new Date().toISOString();
    const dto: ReportSchemaDTO = {
        id: currentSchemaId.value || '00000000-0000-0000-0000-000000000000',
        name: schemaName.value,
        description: schemaDescription.value,
        schemaJson: buildSchemaJson(),
        reportCategory: '',
        isSystemDefault: false,
        createdByName: '',
        createdDate: now,
        modifiedDate: now,
    };

    await store.dispatch('saveReportSchema', { schema: dto });

    const saved = store.state.reportSchemaModule.selectedReportSchema;
    if (saved?.id) {
        currentSchemaId.value = saved.id;
    }

    saving.value = false;
    saveSuccess.value = true;
    setTimeout(() => { saveSuccess.value = false; }, 2000);
}

function onLoadSchema(schema: ReportSchemaDTO) {
    showLoadDialog.value = false;
    currentSchemaId.value = schema.id;
    schemaName.value = schema.name;
    schemaDescription.value = schema.description;

    try {
        const definition: ReportSchemaDefinition = JSON.parse(schema.schemaJson);
        components.value = definition.components || [];
    } catch {
        components.value = [];
    }

    selectedComponentId.value = '';
    store.commit('selectedReportSchemaMutator', schema);
    store.commit('isDirtyMutator', false);
}

function onNew() {
    currentSchemaId.value = '';
    schemaName.value = '';
    schemaDescription.value = '';
    components.value = [];
    selectedComponentId.value = '';
    store.commit('selectedReportSchemaMutator', clone(emptyReportSchemaDTO));
    store.commit('isDirtyMutator', false);
}

onMounted(async () => {
    await store.dispatch('getReportSchemas');
});
</script>

<style scoped>
.report-builder {
    display: flex;
    flex-direction: column;
    height: 100%;
    background: #F5F6F8;
    overflow: hidden;
}

.report-builder__topbar {
    display: flex;
    align-items: center;
    padding: 10px 16px;
    background: #FFFFFF;
    border-bottom: 1px solid #E8E8E8;
    flex-shrink: 0;
    gap: 16px;
}

.report-builder__name-field {
    max-width: 280px;
    font-size: 16px;
    font-weight: 600;
}

.report-builder__name-field :deep(input) {
    font-size: 16px;
    font-weight: 600;
    color: #333;
}

.report-builder__desc-field {
    max-width: 240px;
    font-size: 13px;
}

.report-builder__desc-field :deep(input) {
    font-size: 13px;
    color: #888;
}

.report-builder__body {
    display: flex;
    flex: 1;
    min-height: 0;
    overflow: hidden;
}

.report-builder__palette {
    width: 210px;
    flex-shrink: 0;
    overflow-y: auto;
    background: #FFFFFF;
    border-right: 1px solid #E8E8E8;
    padding: 12px;
}

.report-builder__canvas {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
    min-width: 0;
}

.report-builder__properties {
    width: 360px;
    flex-shrink: 0;
    overflow-y: auto;
    background: #FFFFFF;
    border-left: 1px solid #E8E8E8;
}

.load-schema-item {
    border-bottom: 1px solid #F5F5F5;
    transition: background-color 0.1s ease;
}

.load-schema-item:hover {
    background-color: #F8F9FA;
}
</style>
