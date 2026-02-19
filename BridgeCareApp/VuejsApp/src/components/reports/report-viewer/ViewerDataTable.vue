<template>
    <v-card class="viewer-card elevation-2 mb-4">
        <v-card-title class="viewer-card-title d-flex align-center">
            <i :class="componentIcon" class="mr-2" />
            <span>{{ component.title }}</span>
            <v-chip
                size="x-small"
                :color="componentChipColor"
                variant="flat"
                class="ml-3"
            >
                {{ componentTypeLabel }}
            </v-chip>
            <v-spacer />
            <span class="total-badge" v-if="totalRowCount > 0">
                {{ totalRowCount.toLocaleString() }} rows
            </span>
        </v-card-title>

        <v-card-text class="pa-0">
            <v-data-table-server
                :headers="tableHeaders"
                :items="tableRows"
                :items-length="totalRowCount"
                :items-per-page-options="[
                    { value: 25, title: '25' },
                    { value: 50, title: '50' },
                    { value: 100, title: '100' },
                ]"
                v-model:page="page"
                v-model:items-per-page="pageSize"
                v-model:sort-by="sortBy"
                sort-asc-icon="custom:GhdTableSortAscSvg"
                sort-desc-icon="custom:GhdTableSortDescSvg"
                :loading="isLoading"
                @update:options="onOptionsUpdate"
                class="ghd-table viewer-data-table"
                density="compact"
            >
                <!-- Filter row -->
                <template v-slot:thead>
                    <tr class="filter-row">
                        <td
                            v-for="header in tableHeaders"
                            :key="'filter-' + header.key"
                            class="filter-cell"
                        >
                            <v-text-field
                                v-if="header.filterable !== false"
                                v-model="columnFilters[header.key]"
                                density="compact"
                                variant="outlined"
                                hide-details
                                placeholder="Filter..."
                                class="filter-input"
                                @update:model-value="onFilterDebounced"
                            />
                        </td>
                    </tr>
                </template>

                <!-- Formatted cells -->
                <template v-slot:item="{ item }">
                    <tr>
                        <td
                            v-for="(header, colIdx) in tableHeaders"
                            :key="header.key"
                            :class="{ 'text-right': isNumericFormat(header.format) }"
                        >
                            {{ formatCell(item[colIdx], header.format) }}
                        </td>
                    </tr>
                </template>

                <template v-slot:no-data>
                    <div class="text-center pa-4 text-grey">
                        {{ isLoading ? 'Loading...' : 'No data available' }}
                    </div>
                </template>
            </v-data-table-server>
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { ReportComponent, componentTypeInfo } from '@/shared/models/iAM/report-schema';
import { ViewerColumn, PagedViewerResult } from '@/shared/models/iAM/viewer';
import ViewerService from '@/services/viewer.service';
import { hasValue } from '@/shared/utils/has-value-util';

interface TableHeader {
    title: string;
    key: string;
    sortable: boolean;
    filterable: boolean;
    format: string;
}

const props = defineProps<{
    component: ReportComponent;
    schemaId: string;
    simulationId: string;
}>();

// Pagination state
const page = ref(1);
const pageSize = ref(50);
const sortBy = ref<{ key: string; order: string }[]>([]);
const totalRowCount = ref(0);
const isLoading = ref(false);

// Column filters
const columnFilters = reactive<Record<string, string>>({});
let filterDebounceTimer: ReturnType<typeof setTimeout> | null = null;

// Table data
const tableHeaders = ref<TableHeader[]>([]);
const tableRows = ref<any[][]>([]);

// Component metadata
const typeInfo = componentTypeInfo[props.component.type];
const componentIcon = typeInfo?.icon || 'fas fa-table';
const componentTypeLabel = typeInfo?.label || props.component.type;
const componentChipColor = typeInfo?.color || '#E3F2FD';

function isNumericFormat(format: string): boolean {
    return format === 'number' || format === 'currency' || format === 'percent';
}

function formatCell(value: any, format: string): string {
    if (value === null || value === undefined || value === '') return '';

    switch (format) {
        case 'currency': {
            const num = typeof value === 'number' ? value : parseFloat(value);
            if (isNaN(num)) return String(value);
            return '$' + num.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        }
        case 'percent': {
            const num = typeof value === 'number' ? value : parseFloat(value);
            if (isNaN(num)) return String(value);
            return num.toLocaleString(undefined, { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + '%';
        }
        case 'number': {
            const num = typeof value === 'number' ? value : parseFloat(value);
            if (isNaN(num)) return String(value);
            return num.toLocaleString();
        }
        case 'year': {
            // Year format: render as plain integer without comma separators (e.g. 2026, not 2,026)
            const num = typeof value === 'number' ? value : parseInt(value, 10);
            if (isNaN(num)) return String(value);
            return String(num);
        }
        default:
            return String(value);
    }
}

function onFilterDebounced() {
    if (filterDebounceTimer) clearTimeout(filterDebounceTimer);
    filterDebounceTimer = setTimeout(() => {
        page.value = 1;
        fetchData();
    }, 300);
}

function onOptionsUpdate(options: any) {
    // Vuetify fires this on page/pageSize/sort changes (including initial mount).
    // Sync local state from the Vuetify-provided options object to ensure consistency.
    if (options.page !== undefined) page.value = options.page;
    if (options.itemsPerPage !== undefined) pageSize.value = options.itemsPerPage;
    if (options.sortBy !== undefined) sortBy.value = options.sortBy;
    fetchData();
}

async function fetchData() {
    isLoading.value = true;
    try {
        const sortKey = sortBy.value.length > 0 ? sortBy.value[0].key : '';
        const sortDir = sortBy.value.length > 0 ? sortBy.value[0].order : 'asc';

        // Build clean column filters (strip empty values)
        const filters: Record<string, string> = {};
        for (const key in columnFilters) {
            if (columnFilters[key] && columnFilters[key].trim()) {
                filters[key] = columnFilters[key].trim();
            }
        }

        const response = await ViewerService.getComponentData(
            props.schemaId,
            props.component.id,
            {
                simulationId: props.simulationId,
                page: page.value,
                pageSize: pageSize.value,
                sortBy: sortKey,
                sortDirection: sortDir,
                columnFilters: filters,
            },
        );

        if (hasValue(response, 'data')) {
            const result: PagedViewerResult = response.data;

            // Build headers from response columns (first time or if columns change)
            if (result.columns && result.columns.length > 0) {
                tableHeaders.value = result.columns.map((col: ViewerColumn) => ({
                    title: col.header,
                    key: col.key,
                    sortable: col.sortable,
                    filterable: col.filterable,
                    format: col.format || 'text',
                }));
            }

            tableRows.value = result.rows || [];
            totalRowCount.value = result.totalRowCount;
        }
    } catch (e: any) {
        console.error('Failed to fetch viewer data:', e);
    } finally {
        isLoading.value = false;
    }
}

// Note: no onMounted fetchData() call needed — Vuetify's v-data-table-server
// fires update:options on initial render, which triggers onOptionsUpdate → fetchData().
</script>

<style scoped>
.filter-row {
    background: #F5F6F8;
}

.filter-cell {
    padding: 4px 8px !important;
}

.filter-input {
    font-size: 0.8rem;
}

.filter-input :deep(.v-field) {
    min-height: 28px;
}

.filter-input :deep(.v-field__input) {
    padding: 2px 8px;
    min-height: 28px;
    font-size: 0.8rem;
}

.total-badge {
    color: #666;
    font-size: 0.8rem;
    font-weight: 400;
}

.viewer-data-table :deep(th) {
    white-space: nowrap;
}
</style>
