<template>
    <div
        class="report-template-browser"
        :class="{
            'report-template-browser--large': large,
            'report-template-browser--action-buttons': actionStyle === 'buttons',
        }"
    >
        <div class="report-template-browser__toolbar">
            <v-text-field
                v-model="search"
                density="compact"
                variant="outlined"
                hide-details
                placeholder="Search templates"
                class="report-template-browser__search"
                @update:model-value="onSearchChanged"
            >
                <template #prepend-inner>
                    <i class="fas fa-search report-template-browser__search-icon" />
                </template>
            </v-text-field>

            <v-tabs
                v-model="scope"
                density="compact"
                color="#2A578D"
                slider-color="#2A578D"
                class="report-template-browser__tabs"
            >
                <v-tab
                    v-for="tab in scopeTabs"
                    :key="tab.value"
                    :value="tab.value"
                    class="text-none report-template-browser__tab"
                >
                    {{ tab.title }}
                </v-tab>
            </v-tabs>
        </div>

        <v-data-table-server
            :headers="headers"
            :items="templates"
            :items-length="totalTemplates"
            :items-per-page-options="pageSizeOptions"
            v-model:page="page"
            v-model:items-per-page="rowsPerPage"
            v-model:sort-by="sortBy"
            :loading="loading"
            item-key="id"
            :density="tableDensity"
            sort-asc-icon="custom:GhdTableSortAscSvg"
            sort-desc-icon="custom:GhdTableSortDescSvg"
            class="ghd-table report-template-browser__table"
            @update:options="onOptionsUpdate"
        >
            <template v-slot:item="{ item }">
                <tr
                    class="report-template-browser__row"
                    :class="{ 'report-template-browser__row--selected': item.id === selectedId }"
                    @click="emit('select', item)"
                >
                    <td>
                        <div class="report-template-browser__template-name">
                            <i class="fas fa-file-alt report-template-browser__file-icon" />
                            <div class="report-template-browser__name-stack">
                                <span class="report-template-browser__name">{{ item.name }}</span>
                                <span class="report-template-browser__description">
                                    {{ item.description || 'No description' }}
                                </span>
                            </div>
                        </div>
                    </td>
                    <td>
                        <span class="report-template-browser__muted">{{ item.ownerName || item.createdByName || 'System' }}</span>
                    </td>
                    <td>
                        <div class="report-template-browser__counts">
                            <span>{{ item.pageCount || 0 }} pages</span>
                            <span>{{ item.componentCount || 0 }} components</span>
                        </div>
                    </td>
                    <td>
                        <span
                            class="report-template-browser__badge"
                            :class="accessClass(item)"
                        >
                            {{ accessLabel(item) }}
                        </span>
                    </td>
                    <td>
                        <span class="report-template-browser__muted">{{ formatDate(item.modifiedDate) }}</span>
                    </td>
                    <td>
                        <div class="report-template-browser__actions" @click.stop>
                            <v-btn
                                v-if="showSelect"
                                size="x-small"
                                variant="flat"
                                class="text-none report-template-browser__select-btn"
                                @click="emit('select', item)"
                            >
                                <i class="fas fa-check mr-1" />
                                {{ selectText }}
                            </v-btn>
                            <v-btn
                                v-if="showView && actionStyle === 'buttons'"
                                size="small"
                                variant="flat"
                                class="text-none report-template-browser__action-btn report-template-browser__action-btn--view"
                                @click="emit('view', item)"
                            >
                                <i class="fas fa-chart-bar mr-2" />
                                View Report
                            </v-btn>
                            <v-btn
                                v-else-if="showView"
                                icon
                                size="x-small"
                                variant="text"
                                title="View report"
                                @click="emit('view', item)"
                            >
                                <i class="fas fa-eye" />
                            </v-btn>
                            <v-btn
                                v-if="showExport && actionStyle === 'buttons'"
                                size="small"
                                variant="flat"
                                class="text-none report-template-browser__action-btn report-template-browser__action-btn--export"
                                :disabled="exportingId === item.id"
                                :loading="exportingId === item.id"
                                @click="emit('export', item)"
                            >
                                <i class="fas fa-file-excel mr-2" />
                                Export Excel
                            </v-btn>
                            <v-btn
                                v-else-if="showExport"
                                icon
                                size="x-small"
                                variant="text"
                                title="Export Excel"
                                :disabled="exportingId === item.id"
                                :loading="exportingId === item.id"
                                @click="emit('export', item)"
                            >
                                <i class="fas fa-file-excel" />
                            </v-btn>
                            <v-btn
                                v-if="showEdit && item.canModify"
                                icon
                                size="x-small"
                                variant="text"
                                title="Edit template"
                                @click="emit('edit', item)"
                            >
                                <i class="fas fa-pen" />
                            </v-btn>
                            <v-btn
                                v-if="showShare && item.canShare"
                                icon
                                size="x-small"
                                variant="text"
                                title="Share template"
                                @click="emit('share', item)"
                            >
                                <i class="fas fa-share-alt" />
                            </v-btn>
                            <v-btn
                                v-if="showDelete && item.canShare && !item.isSystemDefault"
                                icon
                                size="x-small"
                                variant="text"
                                title="Delete template"
                                class="report-template-browser__delete-btn"
                                @click="emit('delete', item)"
                            >
                                <i class="fas fa-trash" />
                            </v-btn>
                        </div>
                    </td>
                </tr>
            </template>

            <template v-slot:no-data>
                <div class="report-template-browser__empty">
                    <i class="fas fa-inbox" />
                    <span>No report templates found</span>
                </div>
            </template>
        </v-data-table-server>
    </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useStore } from 'vuex';
import { ReportSchemaDTO } from '@/shared/models/iAM/report-schema';
import { PagingPage } from '@/shared/models/iAM/paging';
import { ReportSchemaPagingRequest } from '@/services/report-schema.service';

const props = withDefaults(defineProps<{
    selectedId?: string;
    initialScope?: string;
    showSelect?: boolean;
    selectText?: string;
    showView?: boolean;
    showExport?: boolean;
    showEdit?: boolean;
    showShare?: boolean;
    showDelete?: boolean;
    exportingId?: string;
    dense?: boolean;
    large?: boolean;
    actionStyle?: 'icons' | 'buttons';
}>(), {
    selectedId: '',
    initialScope: 'all',
    showSelect: true,
    selectText: 'Select',
    showView: false,
    showExport: false,
    showEdit: false,
    showShare: false,
    showDelete: false,
    exportingId: '',
    dense: false,
    large: false,
    actionStyle: 'icons',
});

const emit = defineEmits<{
    (e: 'select', schema: ReportSchemaDTO): void;
    (e: 'view', schema: ReportSchemaDTO): void;
    (e: 'export', schema: ReportSchemaDTO): void;
    (e: 'edit', schema: ReportSchemaDTO): void;
    (e: 'share', schema: ReportSchemaDTO): void;
    (e: 'delete', schema: ReportSchemaDTO): void;
}>();

const store = useStore();

const scope = ref(props.initialScope);
const search = ref('');
const page = ref(1);
const rowsPerPage = ref(props.dense ? 5 : 10);
const sortBy = ref<{ key: string; order: string }[]>([]);
const loading = ref(false);
let searchTimer: ReturnType<typeof setTimeout> | null = null;

const scopeTabs = [
    { title: 'All', value: 'all' },
    { title: 'Mine', value: 'mine' },
    { title: 'Shared', value: 'shared' },
    { title: 'System', value: 'system' },
];

const headers = computed(() => [
    { title: 'Template', key: 'name', sortable: true },
    { title: 'Owner', key: 'ownerName', sortable: true },
    { title: 'Contents', key: 'componentCount', sortable: false },
    { title: 'Access', key: 'access', sortable: false },
    { title: 'Modified', key: 'modifiedDate', sortable: true },
    {
        title: 'Actions',
        key: 'actions',
        sortable: false,
        width: props.actionStyle === 'buttons' ? '300px' : '210px',
    },
]);

const tableDensity = computed(() => (props.dense ? 'compact' : props.large ? 'comfortable' : 'compact'));

const pageSizeOptions = [
    { value: 5, title: '5' },
    { value: 10, title: '10' },
    { value: 25, title: '25' },
    { value: 50, title: '50' },
];

const templatePage = computed<PagingPage<ReportSchemaDTO>>(
    () => store.state.reportSchemaModule.reportSchemaPage,
);
const templates = computed<ReportSchemaDTO[]>(() => templatePage.value?.items || []);
const totalTemplates = computed<number>(() => templatePage.value?.totalItems || 0);

watch(scope, () => {
    page.value = 1;
    void fetchTemplates();
});

function onSearchChanged() {
    if (searchTimer) {
        clearTimeout(searchTimer);
    }
    searchTimer = setTimeout(() => {
        page.value = 1;
        void fetchTemplates();
    }, 250);
}

function onOptionsUpdate(options: any) {
    if (options.page !== undefined) {
        page.value = options.page;
    }
    if (options.itemsPerPage !== undefined) {
        rowsPerPage.value = options.itemsPerPage;
    }
    if (options.sortBy !== undefined) {
        sortBy.value = options.sortBy;
    }
    void fetchTemplates();
}

async function fetchTemplates() {
    loading.value = true;
    const sort = sortBy.value?.[0];
    const request: ReportSchemaPagingRequest = {
        page: page.value,
        rowsPerPage: rowsPerPage.value,
        sortColumn: sort?.key || '',
        isDescending: sort?.order === 'desc',
        search: search.value,
        scope: scope.value,
        syncModel: {
            libraryId: null,
            isModified: false,
            rowsForDeletion: [],
            updateRows: [],
            addedRows: [],
        },
    };

    try {
        await store.dispatch('getReportSchemaPage', { request });
    } finally {
        loading.value = false;
    }
}

function accessLabel(schema: ReportSchemaDTO): string {
    if (schema.isSystemDefault) {
        return 'System';
    }
    if (schema.isSharedWithAll) {
        return 'All users';
    }
    if (schema.currentUserAccessLevel === 2) {
        return 'Owner';
    }
    if (schema.currentUserAccessLevel === 1) {
        return 'Can edit';
    }
    return 'Can view';
}

function accessClass(schema: ReportSchemaDTO): string {
    if (schema.isSystemDefault) {
        return 'report-template-browser__badge--system';
    }
    if (schema.isSharedWithAll) {
        return 'report-template-browser__badge--public';
    }
    if (schema.currentUserAccessLevel === 2) {
        return 'report-template-browser__badge--owner';
    }
    if (schema.currentUserAccessLevel === 1) {
        return 'report-template-browser__badge--edit';
    }
    return 'report-template-browser__badge--view';
}

function formatDate(value: string): string {
    if (!value) {
        return '';
    }
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return value;
    }
    return date.toLocaleDateString();
}

onMounted(() => {
    void fetchTemplates();
});
</script>

<style scoped>
.report-template-browser {
    border: 1px solid #DDE6F1;
    border-radius: 8px;
    background: #FFFFFF;
    overflow: hidden;
}

.report-template-browser__toolbar {
    display: flex;
    align-items: center;
    gap: 14px;
    padding: 12px;
    background: #F7F9FC;
    border-bottom: 1px solid #E5ECF4;
}

.report-template-browser__search {
    width: 280px;
    max-width: 100%;
    flex-shrink: 0;
}

.report-template-browser__search :deep(.v-field) {
    background: #FFFFFF;
    border-radius: 6px;
}

.report-template-browser__search-icon {
    color: #6F7F91;
    font-size: 12px;
}

.report-template-browser__tabs {
    min-width: 0;
}

.report-template-browser__tab {
    font-size: 12px !important;
    letter-spacing: 0 !important;
    min-width: 72px !important;
}

.report-template-browser__row {
    cursor: pointer;
}

.report-template-browser__row:hover {
    background: #F5F8FC;
}

.report-template-browser__row--selected {
    background: #EAF2FB;
}

.report-template-browser--large .report-template-browser__table :deep(th) {
    font-size: 13px !important;
}

.report-template-browser--large .report-template-browser__table :deep(td) {
    padding-top: 8px !important;
    padding-bottom: 8px !important;
}

.report-template-browser__template-name {
    display: flex;
    align-items: center;
    gap: 10px;
    min-width: 0;
}

.report-template-browser__file-icon {
    color: #2A578D;
    font-size: 13px;
    flex-shrink: 0;
}

.report-template-browser--large .report-template-browser__file-icon {
    font-size: 15px;
}

.report-template-browser__name-stack {
    min-width: 0;
    display: flex;
    flex-direction: column;
}

.report-template-browser__name {
    color: #24364B;
    font-size: 13px;
    font-weight: 700;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 360px;
}

.report-template-browser--large .report-template-browser__name {
    font-size: 14px;
}

.report-template-browser__description {
    color: #6F7F91;
    font-size: 11px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 420px;
}

.report-template-browser--large .report-template-browser__description {
    font-size: 12px;
}

.report-template-browser__muted,
.report-template-browser__counts {
    color: #5B6675;
    font-size: 12px;
}

.report-template-browser--large .report-template-browser__muted,
.report-template-browser--large .report-template-browser__counts {
    font-size: 13px;
}

.report-template-browser__counts {
    display: flex;
    flex-direction: column;
    gap: 1px;
}

.report-template-browser__badge {
    display: inline-flex;
    align-items: center;
    min-height: 22px;
    border-radius: 999px;
    padding: 2px 9px;
    font-size: 11px;
    font-weight: 700;
    white-space: nowrap;
}

.report-template-browser--large .report-template-browser__badge {
    font-size: 12px;
    min-height: 24px;
}

.report-template-browser__badge--system {
    background: #ECEFF3;
    color: #526071;
}

.report-template-browser__badge--public {
    background: #E4F4EA;
    color: #246044;
}

.report-template-browser__badge--owner {
    background: #E7F0FA;
    color: #24527F;
}

.report-template-browser__badge--edit {
    background: #FFF4DE;
    color: #7A4E00;
}

.report-template-browser__badge--view {
    background: #EEF2F6;
    color: #4A5B6B;
}

.report-template-browser__actions {
    display: flex;
    align-items: center;
    justify-content: flex-end;
    gap: 4px;
    min-width: 180px;
}

.report-template-browser--action-buttons .report-template-browser__actions {
    gap: 8px;
    min-width: 280px;
}

.report-template-browser__actions :deep(.v-btn) {
    color: #2A578D;
}

.report-template-browser__action-btn {
    border-radius: 5px !important;
    font-family: 'Montserrat', sans-serif !important;
    font-size: 13px !important;
    font-weight: 700 !important;
    letter-spacing: 0 !important;
    min-height: 34px;
    padding: 0 13px !important;
    text-transform: none !important;
}

.report-template-browser--large .report-template-browser__action-btn {
    font-size: 14px !important;
    min-height: 38px;
    padding: 0 16px !important;
}

.report-template-browser__action-btn--view {
    background-color: #2A578D !important;
    color: #FFFFFF !important;
}

.report-template-browser__action-btn--export {
    background-color: #2F6F4E !important;
    color: #FFFFFF !important;
}

.report-template-browser__action-btn :deep(.v-btn__content),
.report-template-browser__action-btn :deep(.v-progress-circular),
.report-template-browser__action-btn i {
    color: #FFFFFF !important;
}

.report-template-browser__action-btn:not(.v-btn--disabled) {
    box-shadow: 0 1px 3px rgba(18, 32, 52, 0.16) !important;
}

.report-template-browser__select-btn {
    background: #2A578D !important;
    color: #FFFFFF !important;
    border-radius: 5px !important;
    letter-spacing: 0 !important;
}

.report-template-browser__delete-btn {
    color: #B42318 !important;
}

.report-template-browser__empty {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    min-height: 120px;
    color: #7A8696;
    font-size: 13px;
}

@media (max-width: 900px) {
    .report-template-browser__toolbar {
        align-items: stretch;
        flex-direction: column;
    }

    .report-template-browser__search {
        width: 100%;
    }

    .report-template-browser__name {
        max-width: 220px;
    }

    .report-template-browser__description {
        max-width: 260px;
    }
}
</style>
