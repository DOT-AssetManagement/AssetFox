<template>
    <v-card class="viewer-card elevation-2 mb-4">
        <v-card-title class="viewer-card-title d-flex align-center">
            <i class="fas fa-sliders-h mr-2" />
            <span>{{ component.title }}</span>
            <v-chip size="x-small" color="#455A64" variant="flat" class="ml-3">
                Scenario Parameters
            </v-chip>
        </v-card-title>

        <v-card-text class="scenario-parameters-content">
            <div v-if="isLoading" class="d-flex justify-center pa-6">
                <v-progress-circular indeterminate color="primary" />
            </div>

            <v-alert v-else-if="errorMessage" type="error" variant="tonal" density="compact">
                {{ errorMessage }}
            </v-alert>

            <div v-else-if="sections.length === 0" class="text-center pa-6 text-grey">
                No scenario parameters are available for the selected sections.
            </div>

            <section
                v-for="section in sections"
                v-else
                :key="section.key"
                class="parameter-section"
            >
                <div class="parameter-section-title">{{ section.title }}</div>

                <div v-if="section.kind === 'keyValue'" class="key-value-grid">
                    <template v-for="row in section.rows" :key="row.key">
                        <div class="key-value-label">{{ row.label }}</div>
                        <div class="key-value-value">
                            {{ formatCell(row.cells[0]?.value, row.cells[0]?.format) }}
                        </div>
                    </template>
                </div>

                <div v-else class="parameter-table-wrapper">
                    <table class="parameter-table">
                        <thead>
                            <tr>
                                <th v-if="section.rowHeader">{{ section.rowHeader }}</th>
                                <th v-for="column in section.columns" :key="column.key">
                                    {{ column.label }}
                                </th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="row in section.rows" :key="row.key">
                                <th v-if="section.rowHeader" scope="row">{{ row.label }}</th>
                                <td
                                    v-for="column in section.columns"
                                    :key="column.key"
                                    :class="{ 'text-right': isNumericFormat(cellFor(row, column.key)?.format) }"
                                >
                                    {{ formatCell(cellFor(row, column.key)?.value, cellFor(row, column.key)?.format) }}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </section>
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import { onBeforeUnmount, ref, watch } from 'vue';
import { ReportComponent } from '@/shared/models/iAM/report-schema';
import { ViewerColumn } from '@/shared/models/iAM/viewer';
import ViewerService from '@/services/viewer.service';

interface ParameterCell {
    columnKey: string;
    value: unknown;
    format: string;
}

interface ParameterColumn {
    key: string;
    label: string;
    order: number;
}

interface ParameterRow {
    key: string;
    label: string;
    order: number;
    cells: ParameterCell[];
}

interface ParameterSection {
    key: string;
    title: string;
    kind: string;
    order: number;
    rowHeader: string;
    columns: ParameterColumn[];
    rows: ParameterRow[];
}

const props = defineProps<{
    component: ReportComponent;
    schemaId: string;
    simulationId: string;
}>();

const sections = ref<ParameterSection[]>([]);
const isLoading = ref(false);
const errorMessage = ref('');
let activeFetchController: AbortController | null = null;

watch(
    () => [props.schemaId, props.simulationId, props.component.id],
    () => fetchData(),
    { immediate: true },
);

async function fetchData() {
    if (activeFetchController) activeFetchController.abort();
    const controller = new AbortController();
    activeFetchController = controller;
    isLoading.value = true;
    errorMessage.value = '';

    try {
        const response = await ViewerService.getComponentData(
            props.schemaId,
            props.component.id,
            {
                simulationId: props.simulationId,
                page: 1,
                pageSize: 2147483647,
                sortBy: '',
                sortDirection: 'asc',
                columnFilters: {},
            },
            controller.signal,
        );
        if (controller.signal.aborted) return;

        sections.value = decodeSections(response.data?.columns || [], response.data?.rows || []);
    } catch (error: any) {
        if (error?.code === 'ERR_CANCELED' || error?.name === 'CanceledError' || controller.signal.aborted) return;
        console.error('Failed to load scenario parameters:', error);
        errorMessage.value = 'Scenario parameters could not be loaded.';
    } finally {
        if (activeFetchController === controller) {
            activeFetchController = null;
            isLoading.value = false;
        }
    }
}

function decodeSections(columns: ViewerColumn[], rows: unknown[][]): ParameterSection[] {
    const index = new Map(columns.map((column, columnIndex) => [column.key, columnIndex]));
    const value = (row: unknown[], key: string): any => {
        const columnIndex = index.get(key);
        return columnIndex === undefined ? undefined : row[columnIndex];
    };
    const sectionMap = new Map<string, ParameterSection>();

    for (const encodedRow of rows) {
        const sectionKey = String(value(encodedRow, 'sectionKey') || '');
        if (!sectionMap.has(sectionKey)) {
            sectionMap.set(sectionKey, {
                key: sectionKey,
                title: String(value(encodedRow, 'sectionTitle') || ''),
                kind: String(value(encodedRow, 'sectionKind') || 'table'),
                order: Number(value(encodedRow, 'sectionOrder') || 0),
                rowHeader: String(value(encodedRow, 'rowHeader') || ''),
                columns: [],
                rows: [],
            });
        }

        const section = sectionMap.get(sectionKey)!;
        const columnKey = String(value(encodedRow, 'columnKey') || '');
        if (!section.columns.some(column => column.key === columnKey)) {
            section.columns.push({
                key: columnKey,
                label: String(value(encodedRow, 'columnLabel') || ''),
                order: Number(value(encodedRow, 'columnOrder') || 0),
            });
        }

        const rowKey = String(value(encodedRow, 'rowKey') || '');
        let parameterRow = section.rows.find(row => row.key === rowKey);
        if (!parameterRow) {
            parameterRow = {
                key: rowKey,
                label: String(value(encodedRow, 'rowLabel') || ''),
                order: Number(value(encodedRow, 'rowOrder') || 0),
                cells: [],
            };
            section.rows.push(parameterRow);
        }

        parameterRow.cells.push({
            columnKey,
            value: value(encodedRow, 'value'),
            format: String(value(encodedRow, 'format') || 'text'),
        });
    }

    const decoded = [...sectionMap.values()].sort((left, right) => left.order - right.order);
    for (const section of decoded) {
        section.columns.sort((left, right) => left.order - right.order);
        section.rows.sort((left, right) => left.order - right.order);
    }
    return decoded;
}

function cellFor(row: ParameterRow, columnKey: string): ParameterCell | undefined {
    return row.cells.find(cell => cell.columnKey === columnKey);
}

function isNumericFormat(format?: string): boolean {
    return format === 'number' || format === 'currency' || format === 'percent' || format === 'year';
}

function formatCell(value: unknown, format?: string): string {
    if (value === null || value === undefined || value === '') return '';
    if (format === 'boolean') {
        if (typeof value === 'boolean') return value ? 'Yes' : 'No';
        if (String(value).toLowerCase() === 'true') return 'Yes';
        if (String(value).toLowerCase() === 'false') return 'No';
    }
    if (format === 'date') {
        const date = new Date(String(value));
        return Number.isNaN(date.getTime()) ? String(value) : date.toLocaleString();
    }
    if (format === 'currency') {
        const number = Number(value);
        return Number.isNaN(number)
            ? String(value)
            : number.toLocaleString(undefined, { style: 'currency', currency: 'USD' });
    }
    if (format === 'percent') {
        const number = Number(value);
        return Number.isNaN(number) ? String(value) : `${number.toLocaleString(undefined, { maximumFractionDigits: 2 })}%`;
    }
    if (format === 'number') {
        const number = Number(value);
        return Number.isNaN(number) ? String(value) : number.toLocaleString();
    }
    if (format === 'year') return String(value);
    return String(value);
}

onBeforeUnmount(() => activeFetchController?.abort());
</script>

<style scoped>
.scenario-parameters-content {
    padding: 16px;
}

.parameter-section + .parameter-section {
    margin-top: 20px;
}

.parameter-section-title {
    padding: 8px 12px;
    border-radius: 6px 6px 0 0;
    background: #455A64;
    color: white;
    font-size: 13px;
    font-weight: 600;
}

.key-value-grid {
    display: grid;
    grid-template-columns: minmax(180px, 30%) minmax(0, 1fr);
    border: 1px solid #DCE1E5;
    border-top: 0;
}

.key-value-label,
.key-value-value {
    padding: 8px 12px;
    border-top: 1px solid #E8ECEF;
    overflow-wrap: anywhere;
}

.key-value-label {
    background: #F4F6F7;
    font-weight: 600;
}

.parameter-table-wrapper {
    overflow-x: auto;
    border: 1px solid #DCE1E5;
    border-top: 0;
}

.parameter-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 12px;
}

.parameter-table th,
.parameter-table td {
    padding: 8px 10px;
    border-right: 1px solid #E8ECEF;
    border-bottom: 1px solid #E8ECEF;
    text-align: left;
    vertical-align: top;
    white-space: pre-wrap;
}

.parameter-table thead th,
.parameter-table tbody th {
    background: #F4F6F7;
    font-weight: 600;
}

.parameter-table tr:last-child td,
.parameter-table tr:last-child th {
    border-bottom: 0;
}

.parameter-table th:last-child,
.parameter-table td:last-child {
    border-right: 0;
}

@media (max-width: 700px) {
    .key-value-grid {
        grid-template-columns: 1fr;
    }

    .key-value-value {
        border-top: 0;
    }
}
</style>
