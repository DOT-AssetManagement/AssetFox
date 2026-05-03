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

        <!-- Page tabs -->
        <div class="report-builder__page-tabs">
            <v-tabs
                v-model="activePageIndex"
                density="compact"
                color="#2A578D"
                slider-color="#2A578D"
                class="page-tabs"
            >
                <v-tab
                    v-for="(page, idx) in pages"
                    :key="page.id"
                    :value="idx"
                    size="small"
                    class="text-none page-tab"
                >
                    <template v-if="renamingPageId === page.id">
                        <v-text-field
                            v-model="renameValue"
                            variant="plain"
                            density="compact"
                            hide-details
                            autofocus
                            class="page-tab-rename"
                            @keydown.enter="commitRename"
                            @keydown.escape="cancelRename"
                            @blur="commitRename"
                            @click.stop
                        />
                    </template>
                    <template v-else>
                        <span @dblclick.stop="startRename(page)">{{ page.name }}</span>
                    </template>
                    <v-btn
                        v-if="pages.length > 1 && renamingPageId !== page.id"
                        icon
                        size="x-small"
                        variant="text"
                        class="ml-1 page-tab-close"
                        @click.stop="onDeletePage(idx)"
                    >
                        <i class="fas fa-times" style="font-size: 8px; color: #999;" />
                    </v-btn>
                </v-tab>
            </v-tabs>
            <v-btn
                size="x-small"
                variant="text"
                class="text-none ml-1"
                @click="onAddPage"
            >
                <i class="fas fa-plus" style="font-size: 10px;" />
            </v-btn>
        </div>

        <!-- Main content -->
        <div class="report-builder__body">
            <!-- Left: Palette -->
            <div class="report-builder__palette">
                <ComponentPalette />
            </div>

            <!-- Center: Grid Editor -->
            <div class="report-builder__canvas">
                <PageEditor
                    v-if="activePage"
                    :components="activePage.components"
                    :selected-id="selectedComponentId"
                    @update:components="onComponentsUpdate"
                    @select="onSelectComponent"
                    @delete="onDeleteComponent"
                    @component-dropped="onComponentDropped"
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

        <!-- Delete component confirmation dialog -->
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

        <!-- Delete page confirmation dialog -->
        <v-dialog v-model="showDeletePageConfirm" max-width="360">
            <v-card class="rounded-lg pa-4">
                <div class="text-subtitle-1 font-weight-medium mb-2">Delete page?</div>
                <div class="text-body-2 text-grey mb-4">
                    This page and all its components will be removed from the report.
                </div>
                <div class="d-flex justify-end ga-2">
                    <v-btn size="small" variant="text" class="text-none" @click="showDeletePageConfirm = false">Cancel</v-btn>
                    <v-btn size="small" color="error" variant="flat" class="text-none" @click="confirmDeletePage">Delete</v-btn>
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
    ReportPage,
    emptyReportSchemaDTO,
    emptyPageSettings,
    componentTypeInfo,
    createDefaultPage,
} from '@/shared/models/iAM/report-schema';
import ComponentPalette from './ComponentPalette.vue';
import PageEditor from './PageEditor.vue';
import ComponentProperties from './ComponentProperties.vue';

const store = useStore();

// Schema state
const schemaName = ref('');
const schemaDescription = ref('');
const pages = ref<ReportPage[]>([createDefaultPage()]);
const currentSchemaId = ref('');

// Page state
const activePageIndex = ref(0);
const renamingPageId = ref('');
const renameValue = ref('');
const showDeletePageConfirm = ref(false);
const pendingDeletePageIndex = ref(-1);

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

const activePage = computed<ReportPage | null>(() =>
    pages.value[activePageIndex.value] || null
);

const selectedComponent = computed<ReportComponent | null>(() => {
    if (!activePage.value) return null;
    return activePage.value.components.find(c => c.id === selectedComponentId.value) || null;
});

// Methods
function markDirty() {
    store.commit('isDirtyMutator', true);
}

// --- Page management ---

function onAddPage() {
    const pageNum = pages.value.length + 1;
    const newPage = createDefaultPage(`Page ${pageNum}`);
    newPage.order = pages.value.length;
    pages.value = [...pages.value, newPage];
    activePageIndex.value = pages.value.length - 1;
    markDirty();
}

function onDeletePage(idx: number) {
    if (pages.value.length <= 1) return;
    if (pages.value[idx].components.length > 0) {
        pendingDeletePageIndex.value = idx;
        showDeletePageConfirm.value = true;
    } else {
        removePageAtIndex(idx);
    }
}

function confirmDeletePage() {
    removePageAtIndex(pendingDeletePageIndex.value);
    showDeletePageConfirm.value = false;
    pendingDeletePageIndex.value = -1;
}

function removePageAtIndex(idx: number) {
    pages.value = pages.value.filter((_, i) => i !== idx).map((p, i) => ({ ...p, order: i }));
    if (activePageIndex.value >= pages.value.length) {
        activePageIndex.value = pages.value.length - 1;
    }
    selectedComponentId.value = '';
    markDirty();
}

function startRename(page: ReportPage) {
    renamingPageId.value = page.id;
    renameValue.value = page.name;
}

function commitRename() {
    if (renamingPageId.value && renameValue.value.trim()) {
        const page = pages.value.find(p => p.id === renamingPageId.value);
        if (page) {
            page.name = renameValue.value.trim();
            pages.value = [...pages.value]; // trigger reactivity
            markDirty();
        }
    }
    renamingPageId.value = '';
    renameValue.value = '';
}

function cancelRename() {
    renamingPageId.value = '';
    renameValue.value = '';
}

// --- Component management ---

function onComponentDropped(comp: ReportComponent) {
    if (!activePage.value) return;
    comp.order = activePage.value.components.length;
    activePage.value.components = [...activePage.value.components, comp];
    pages.value = [...pages.value]; // trigger reactivity
    selectedComponentId.value = comp.id;
    markDirty();
}

function onComponentsUpdate(updated: ReportComponent[]) {
    if (!activePage.value) return;
    activePage.value.components = updated;
    pages.value = [...pages.value];
    markDirty();
}

function onSelectComponent(id: string) {
    selectedComponentId.value = id;
}

function onComponentUpdate(updated: ReportComponent) {
    if (!activePage.value) return;
    const index = activePage.value.components.findIndex(c => c.id === updated.id);
    if (index >= 0) {
        const cloned = clone(activePage.value.components);
        cloned[index] = updated;
        activePage.value.components = cloned;
        pages.value = [...pages.value];
        markDirty();
    }
}

function onDeleteComponent(id: string) {
    pendingDeleteId.value = id;
    showDeleteConfirm.value = true;
}

function confirmDelete() {
    if (!activePage.value) return;
    activePage.value.components = activePage.value.components
        .filter(c => c.id !== pendingDeleteId.value)
        .map((c, i) => ({ ...c, order: i }));
    pages.value = [...pages.value];
    if (selectedComponentId.value === pendingDeleteId.value) {
        selectedComponentId.value = '';
    }
    showDeleteConfirm.value = false;
    pendingDeleteId.value = '';
    markDirty();
}

// --- Save / Load ---

function getRequiredComponentTitle(component: ReportComponent): string {
    return component.title?.trim()
        || componentTypeInfo[component.type]?.label
        || 'Report Component';
}

function prepareComponentForSave(component: ReportComponent, order: number): ReportComponent {
    return {
        ...component,
        title: getRequiredComponentTitle(component),
        order,
        config: clone(component.config),
    };
}

function prepareLoadedPages(loadedPages: ReportPage[]): ReportPage[] {
    return loadedPages.map((page, pageIndex) => ({
        ...page,
        order: pageIndex,
        components: (page.components || []).map((component, componentIndex) =>
            prepareComponentForSave(component, componentIndex)
        ),
    }));
}

function buildSchemaJson(): string {
    const definition: ReportSchemaDefinition = {
        schemaVersion: '1.0',
        name: schemaName.value,
        description: schemaDescription.value,
        pageSettings: clone(emptyPageSettings),
        pages: pages.value.map((p, pi) => ({
            ...p,
            order: pi,
            components: p.components.map((c, ci) => prepareComponentForSave(c, ci)),
        })),
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
        const definition = JSON.parse(schema.schemaJson) as ReportSchemaDefinition;
        pages.value = definition.pages && definition.pages.length > 0
            ? prepareLoadedPages(definition.pages)
            : [createDefaultPage()];
    } catch {
        pages.value = [createDefaultPage()];
    }

    activePageIndex.value = 0;
    selectedComponentId.value = '';
    store.commit('selectedReportSchemaMutator', schema);
    store.commit('isDirtyMutator', false);
}

function onNew() {
    currentSchemaId.value = '';
    schemaName.value = '';
    schemaDescription.value = '';
    pages.value = [createDefaultPage()];
    activePageIndex.value = 0;
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

.report-builder__page-tabs {
    display: flex;
    align-items: center;
    background: #FFFFFF;
    border-bottom: 1px solid #E8E8E8;
    padding: 0 12px;
    flex-shrink: 0;
}

.page-tabs {
    flex: 1;
    min-width: 0;
}

.page-tab {
    font-size: 12px !important;
    letter-spacing: 0 !important;
    min-width: 80px !important;
    text-transform: none !important;
}

.page-tab-close {
    opacity: 0;
    transition: opacity 0.15s ease;
}

.page-tab:hover .page-tab-close {
    opacity: 1;
}

.page-tab-rename {
    max-width: 100px;
    font-size: 12px;
}

.page-tab-rename :deep(input) {
    font-size: 12px;
    padding: 0;
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
