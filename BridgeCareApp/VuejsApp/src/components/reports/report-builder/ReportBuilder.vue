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
            <div class="report-builder__actions">
                <div class="report-builder__action-group">
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
                        variant="outlined"
                        class="text-none"
                        @click="showCalculatedFields = true"
                    >
                        <i class="fas fa-calculator mr-2" style="font-size: 10px;" />
                        Calculated fields
                        <v-chip v-if="calculatedFields.length" size="x-small" class="ml-2">{{ calculatedFields.length }}</v-chip>
                    </v-btn>
                    <v-btn
                        size="small"
                        variant="outlined"
                        class="text-none"
                        :disabled="!currentSchema?.canShare"
                        @click="openShareDialog()"
                    >
                        <i class="fas fa-share-alt mr-2" style="font-size: 10px;" />
                        Share
                    </v-btn>
                    <v-btn
                        v-if="canDeleteTemplate"
                        size="small"
                        variant="outlined"
                        class="text-none report-builder__delete-template-btn"
                        @click="showDeleteTemplateConfirm = true"
                    >
                        <i class="fas fa-trash mr-2" style="font-size: 10px;" />
                        Delete
                    </v-btn>
                </div>
                <div
                    v-if="hasAdminAccess || isSystemDefault || isReadOnly"
                    class="report-builder__status-group"
                >
                    <v-btn
                        v-if="hasAdminAccess"
                        size="small"
                        :variant="isSystemDefault ? 'flat' : 'outlined'"
                        :color="isSystemDefault ? '#2A578D' : undefined"
                        class="text-none report-builder__system-action"
                        :class="{ 'report-builder__system-action--active': isSystemDefault }"
                        @click="toggleSystemDefault"
                    >
                        <i class="fas fa-star mr-2" style="font-size: 10px;" />
                        System default
                    </v-btn>
                    <v-chip
                        v-else-if="isSystemDefault"
                        size="small"
                        color="#ECEFF3"
                        variant="flat"
                        class="report-builder__system-chip"
                    >
                        System default
                    </v-chip>
                    <v-chip
                        v-if="isReadOnly"
                        size="small"
                        color="#FFF4DE"
                        variant="flat"
                        class="report-builder__readonly-chip"
                    >
                        Read only
                    </v-chip>
                </div>
                <div class="report-builder__save-group">
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
                        {{ isReadOnly ? 'Save Copy' : 'Save' }}
                        <i v-if="saveSuccess" class="fas fa-check ml-1" style="font-size: 10px;" />
                    </v-btn>
                </div>
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
                    :calculated-fields="calculatedFields"
                    @update:component="onComponentUpdate"
                />
            </div>
        </div>

        <!-- Load dialog -->
        <v-dialog v-model="showLoadDialog" max-width="1120">
            <v-card class="rounded-lg">
                <v-card-title class="d-flex align-center report-builder__load-title">
                    <i class="fas fa-folder-open mr-2" style="font-size: 14px; color: #888;" />
                    <span class="text-subtitle-1 font-weight-medium">Load Report Template</span>
                    <v-spacer />
                    <v-btn icon size="x-small" variant="text" @click="showLoadDialog = false">
                        <i class="fas fa-times" style="font-size: 12px; color: #999;" />
                    </v-btn>
                </v-card-title>
                <v-divider />
                <v-card-text class="report-builder__load-body">
                    <ReportTemplateBrowser
                        :selected-id="currentSchemaId"
                        :dense="true"
                        select-text="Load"
                        :show-share="true"
                        @select="onLoadSchema"
                        @share="openShareDialog"
                    />
                </v-card-text>
            </v-card>
        </v-dialog>

        <ShareReportTemplateDialog
            v-model="showShareDialog"
            :schema="shareDialogSchema"
            @saved="onSharingSaved"
        />

        <CalculatedFieldManager
            v-model="showCalculatedFields"
            :fields="calculatedFields"
            :used-field-ids="usedCalculatedFieldIds"
            @update:fields="onCalculatedFieldsUpdate"
        />

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

        <!-- Delete template confirmation dialog -->
        <v-dialog v-model="showDeleteTemplateConfirm" max-width="420">
            <v-card class="rounded-lg pa-4">
                <div class="text-subtitle-1 font-weight-medium mb-2">Delete report template?</div>
                <div class="text-body-2 text-grey mb-4">
                    {{ deleteTemplateMessage }}
                </div>
                <div class="d-flex justify-end ga-2">
                    <v-btn
                        size="small"
                        variant="text"
                        class="text-none"
                        :disabled="deletingTemplate"
                        @click="showDeleteTemplateConfirm = false"
                    >
                        Cancel
                    </v-btn>
                    <v-btn
                        size="small"
                        color="error"
                        variant="flat"
                        class="text-none"
                        :loading="deletingTemplate"
                        @click="confirmDeleteTemplate"
                    >
                        Delete
                    </v-btn>
                </div>
            </v-card>
        </v-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { clone } from 'ramda';
import {
    ReportSchemaDTO,
    ReportSchemaDefinition,
    ReportComponent,
    ReportCalculatedField,
    ReportPage,
    emptyReportSchemaDTO,
    componentTypeInfo,
    createDefaultPage,
} from '@/shared/models/iAM/report-schema';
import { validateReportChartComponents } from '@/shared/utils/report-chart-validation';
import ComponentPalette from './ComponentPalette.vue';
import PageEditor from './PageEditor.vue';
import ComponentProperties from './ComponentProperties.vue';
import ReportTemplateBrowser from '@/components/reports/ReportTemplateBrowser.vue';
import ShareReportTemplateDialog from '@/components/reports/ShareReportTemplateDialog.vue';
import CalculatedFieldManager from './calculated-fields/CalculatedFieldManager.vue';
import { useAuthenticationStore, useNotificationStore, useReportSchemaStore } from '@/stores';

const authenticationStore = useAuthenticationStore();
const notificationStore = useNotificationStore();
const reportSchemaStore = useReportSchemaStore();

// Schema state
const schemaName = ref('');
const schemaDescription = ref('');
const pages = ref<ReportPage[]>([createDefaultPage()]);
const calculatedFields = ref<ReportCalculatedField[]>([]);
const currentSchemaId = ref('');
const currentSchema = ref<ReportSchemaDTO | null>(null);
const isSystemDefault = ref(false);

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
const showShareDialog = ref(false);
const shareDialogSchema = ref<ReportSchemaDTO | null>(null);
const showDeleteTemplateConfirm = ref(false);
const deletingTemplate = ref(false);
const showCalculatedFields = ref(false);

// Computed
const activePage = computed<ReportPage | null>(() =>
    pages.value[activePageIndex.value] || null
);

const selectedComponent = computed<ReportComponent | null>(() => {
    if (!activePage.value) return null;
    return activePage.value.components.find(c => c.id === selectedComponentId.value) || null;
});

const hasAdminAccess = computed<boolean>(() => authenticationStore.hasAdminAccess);

const isReadOnly = computed<boolean>(() =>
    !!currentSchemaId.value && currentSchema.value?.canModify === false
);

const canDeleteTemplate = computed<boolean>(() =>
    !!currentSchemaId.value && (hasAdminAccess.value || currentSchema.value?.canShare === true)
);

const deleteTemplateMessage = computed<string>(() => {
    const name = schemaName.value || currentSchema.value?.name || 'this report template';
    return `This will permanently delete "${name}" and remove it from shared template lists.`;
});

const usedCalculatedFieldIds = computed<string[]>(() => {
    const ids = new Set<string>();
    const pattern = /calculatedField:([0-9a-f-]{36})/gi;
    for (const page of pages.value) {
        for (const component of page.components) {
            const json = JSON.stringify(component.config || {});
            for (const match of json.matchAll(pattern)) ids.add(match[1]);
            const collectFieldIds = (value: unknown) => {
                if (!value || typeof value !== 'object') return;
                if (Array.isArray(value)) {
                    value.forEach(collectFieldIds);
                    return;
                }
                const record = value as Record<string, unknown>;
                if (record.source === 'calculatedField' && typeof record.fieldId === 'string') ids.add(record.fieldId);
                if (record.source === 'calculatedField' && typeof record.field === 'string') ids.add(record.field);
                if (record.type === 'calculatedField' && typeof record.fieldId === 'string') ids.add(record.fieldId);
                Object.values(record).forEach(collectFieldIds);
            };
            collectFieldIds(component.config);
        }
    }
    return Array.from(ids);
});

// Methods
function markDirty() {
    reportSchemaStore.isDirtyMutator(true);
}

function onCalculatedFieldsUpdate(fields: ReportCalculatedField[]) {
    calculatedFields.value = clone(fields);
    markDirty();
}

function toggleSystemDefault() {
    isSystemDefault.value = !isSystemDefault.value;
    markDirty();
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

function buildSchemaJson(nameOverride = schemaName.value): string {
    const definition: ReportSchemaDefinition = {
        schemaVersion: '1.1',
        name: nameOverride,
        description: schemaDescription.value,
        calculatedFields: clone(calculatedFields.value),
        pages: pages.value.map((p, pi) => ({
            ...p,
            order: pi,
            components: p.components.map((c, ci) => prepareComponentForSave(c, ci)),
        })),
    };
    return JSON.stringify(definition);
}

async function onSave() {
    const chartIssues = validateReportChartComponents(pages.value);
    const firstBlocker = chartIssues.find(issue => issue.blocker);
    if (firstBlocker) {
        activePageIndex.value = firstBlocker.pageIndex;
        selectedComponentId.value = firstBlocker.componentId;
        notificationStore.addErrorNotification({
            message: 'Fix chart configuration before saving.',
            longMessage: firstBlocker.message,
        });
        return;
    }

    saving.value = true;
    saveSuccess.value = false;

    const now = new Date().toISOString();
    const savingAsCopy = isReadOnly.value;
    const name = savingAsCopy && schemaName.value === currentSchema.value?.name
        ? `${schemaName.value} Copy`
        : schemaName.value;
    const dto: ReportSchemaDTO = {
        ...clone(emptyReportSchemaDTO),
        id: savingAsCopy ? '00000000-0000-0000-0000-000000000000' : currentSchemaId.value || '00000000-0000-0000-0000-000000000000',
        name,
        description: schemaDescription.value,
        schemaJson: buildSchemaJson(name),
        reportCategory: currentSchema.value?.reportCategory || '',
        isSystemDefault: !savingAsCopy && hasAdminAccess.value && isSystemDefault.value,
        isSharedWithAll: savingAsCopy ? false : currentSchema.value?.isSharedWithAll || false,
        createdDate: now,
        modifiedDate: now,
    };

    const saved = await reportSchemaStore.saveReportSchema(dto) as ReportSchemaDTO | null;

    if (saved?.id) {
        currentSchemaId.value = saved.id;
        currentSchema.value = saved;
        schemaName.value = saved.name;
    }

    saving.value = false;
    saveSuccess.value = true;
    setTimeout(() => { saveSuccess.value = false; }, 2000);
}

async function onLoadSchema(schema: ReportSchemaDTO) {
    showLoadDialog.value = false;
    let fullSchema = schema;

    if (!fullSchema.schemaJson) {
        await reportSchemaStore.getReportSchema(schema.id);
        fullSchema = reportSchemaStore.selectedReportSchema as ReportSchemaDTO;
    }

    loadSchemaIntoBuilder(fullSchema);
}

function loadSchemaIntoBuilder(schema: ReportSchemaDTO) {
    currentSchemaId.value = schema.id;
    currentSchema.value = schema;
    schemaName.value = schema.name;
    schemaDescription.value = schema.description;
    isSystemDefault.value = schema.isSystemDefault;

    try {
        const definition = JSON.parse(schema.schemaJson) as ReportSchemaDefinition;
        calculatedFields.value = clone(definition.calculatedFields || []);
        pages.value = definition.pages && definition.pages.length > 0
            ? prepareLoadedPages(definition.pages)
            : [createDefaultPage()];
    } catch {
        pages.value = [createDefaultPage()];
        calculatedFields.value = [];
    }

    activePageIndex.value = 0;
    selectedComponentId.value = '';
    reportSchemaStore.selectedReportSchemaMutator(schema);
    reportSchemaStore.isDirtyMutator(false);
}

function onNew() {
    currentSchemaId.value = '';
    currentSchema.value = null;
    schemaName.value = '';
    schemaDescription.value = '';
    isSystemDefault.value = false;
    pages.value = [createDefaultPage()];
    calculatedFields.value = [];
    activePageIndex.value = 0;
    selectedComponentId.value = '';
    reportSchemaStore.selectedReportSchemaMutator(clone(emptyReportSchemaDTO));
    reportSchemaStore.isDirtyMutator(false);
}

async function confirmDeleteTemplate() {
    if (!canDeleteTemplate.value || !currentSchemaId.value) {
        showDeleteTemplateConfirm.value = false;
        return;
    }

    deletingTemplate.value = true;
    const deleted = await reportSchemaStore.deleteReportSchema(currentSchemaId.value) as boolean;
    deletingTemplate.value = false;
    showDeleteTemplateConfirm.value = false;

    if (deleted) {
        onNew();
    }
}

function openShareDialog(schema?: ReportSchemaDTO) {
    const target = schema || currentSchema.value;
    if (!target?.id || !target.canShare) {
        return;
    }

    shareDialogSchema.value = target;
    showShareDialog.value = true;
}

function onSharingSaved(schema: ReportSchemaDTO) {
    if (schema.id === currentSchemaId.value) {
        currentSchema.value = schema;
        reportSchemaStore.selectedReportSchemaMutator(schema);
    }
    shareDialogSchema.value = schema;
}
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

.report-builder__actions,
.report-builder__action-group,
.report-builder__status-group,
.report-builder__save-group {
    display: flex;
    align-items: center;
}

.report-builder__actions {
    flex-shrink: 0;
    gap: 12px;
}

.report-builder__action-group,
.report-builder__status-group {
    gap: 8px;
}

.report-builder__status-group {
    min-height: 30px;
    padding-left: 12px;
    border-left: 1px solid #E1E7EE;
}

.report-builder__delete-template-btn {
    border-color: #E3B5B1 !important;
    color: #B42318 !important;
}

.report-builder__delete-template-btn :deep(.v-btn__content),
.report-builder__delete-template-btn i {
    color: #B42318 !important;
}

.report-builder__readonly-chip {
    color: #7A4E00 !important;
    font-size: 11px !important;
    font-weight: 700 !important;
}

.report-builder__system-action {
    height: 30px !important;
    border-color: #C8D3DF !important;
    color: #526071 !important;
    font-size: 12px !important;
    font-weight: 700 !important;
    letter-spacing: 0 !important;
    padding: 0 12px !important;
}

.report-builder__system-action :deep(.v-btn__content) {
    line-height: 1;
}

.report-builder__system-action--active {
    color: #FFFFFF !important;
    box-shadow: none !important;
}

.report-builder__system-chip {
    color: #526071 !important;
    font-size: 11px !important;
    font-weight: 700 !important;
}

.report-builder__load-title {
    min-height: 42px;
    padding: 8px 12px !important;
}

.report-builder__load-body {
    padding: 8px 10px 10px 10px !important;
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
    width: 280px;
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
