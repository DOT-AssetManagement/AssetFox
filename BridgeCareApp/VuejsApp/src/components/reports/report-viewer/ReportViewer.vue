<template>
    <div class="report-viewer-outer">
        <div class="report-viewer-container">
            <!-- Header -->
            <div class="viewer-header d-flex align-center">
                <v-btn
                    variant="text"
                    class="ghd-blue mr-4"
                    @click="goBack"
                >
                    <i class="fas fa-arrow-left mr-2" />
                    Back to Reports
                </v-btn>
                <div>
                    <h2 class="viewer-title">{{ schemaName }}</h2>
                    <span class="viewer-subtitle" v-if="metadata && metadata.hasOutput">
                        {{ scenarioName }}
                        &middot; {{ metadata.assetCount.toLocaleString() }} assets
                        &middot; {{ metadata.years.length }} years
                    </span>
                    <span class="viewer-subtitle" v-else-if="!isLoadingMetadata">
                        No simulation output available
                    </span>
                </div>
            </div>

            <v-progress-linear
                v-if="isLoadingMetadata"
                indeterminate
                color="#2A578D"
                class="mt-2"
            />

            <v-alert
                v-if="!isLoadingMetadata && metadata && !metadata.hasOutput"
                type="warning"
                variant="tonal"
                class="mt-4"
            >
                This simulation has no output data. Please run the simulation first before viewing reports.
            </v-alert>

            <!-- Page tabs + content -->
            <template v-if="!isLoadingMetadata && metadata && metadata.hasOutput && schemaDefinition">
                <!-- Tab bar (only show if more than 1 page) -->
                <v-tabs
                    v-if="pages.length > 1"
                    v-model="activePageIndex"
                    density="compact"
                    color="#2A578D"
                    slider-color="#2A578D"
                    class="viewer-page-tabs mt-3"
                >
                    <v-tab
                        v-for="(page, idx) in pages"
                        :key="page.id"
                        :value="idx"
                        size="small"
                        class="text-none"
                    >
                        {{ page.name }}
                    </v-tab>
                </v-tabs>

                <!-- Active page content -->
                <PageViewer
                    v-if="activePage"
                    :components="activePage.components"
                    :schemaId="schemaId"
                    :simulationId="simulationId"
                />

                <v-alert
                    v-if="activePage && activePage.components.length === 0"
                    type="info"
                    variant="tonal"
                    class="mt-4"
                >
                    This page has no components. Open the Report Builder to add components.
                </v-alert>

                <v-alert
                    v-if="pages.length === 0"
                    type="info"
                    variant="tonal"
                    class="mt-4"
                >
                    This report template has no pages. Open the Report Builder to add content.
                </v-alert>
            </template>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import { useStore } from 'vuex';
import { useRouter } from 'vue-router';
import { ReportSchemaDTO, ReportSchemaDefinition, ReportPage } from '@/shared/models/iAM/report-schema';
import { ViewerMetadata } from '@/shared/models/iAM/viewer';
import PageViewer from './PageViewer.vue';

const store = useStore();
const router = useRouter();

// Route query params
const schemaId = ref('');
const simulationId = ref('');
const scenarioName = ref('');
const schemaName = ref('');

// Schema definition
const schemaDefinition = ref<ReportSchemaDefinition | null>(null);

// Page state
const activePageIndex = ref(0);

// Store state
const metadata = computed<ViewerMetadata>(() => store.state.viewerModule.viewerMetadata);
const isLoadingMetadata = computed<boolean>(() => store.state.viewerModule.isLoadingMetadata);

// Pages from schema
const pages = computed<ReportPage[]>(() => {
    if (!schemaDefinition.value || !schemaDefinition.value.pages) return [];
    return schemaDefinition.value.pages.sort((a, b) => a.order - b.order);
});

const activePage = computed<ReportPage | null>(() =>
    pages.value[activePageIndex.value] || null
);

function goBack() {
    router.push({
        path: '/ReportsAndOutputs/Scenario/',
        query: {
            scenarioId: simulationId.value,
            scenarioName: scenarioName.value,
            networkId: router.currentRoute.value.query.networkId as string,
            networkName: router.currentRoute.value.query.networkName as string,
        },
    });
}

onMounted(async () => {
    // Read route query params
    schemaId.value = router.currentRoute.value.query.schemaId as string;
    simulationId.value = router.currentRoute.value.query.scenarioId as string;
    scenarioName.value = router.currentRoute.value.query.scenarioName as string;
    schemaName.value = (router.currentRoute.value.query.schemaName as string) || 'Report';

    if (!schemaId.value || !simulationId.value) {
        store.dispatch('addErrorNotification', 'Missing schema or simulation ID.');
        goBack();
        return;
    }

    // Load metadata
    await store.dispatch('loadViewerMetadata', { simulationId: simulationId.value });

    // Load schema
    await store.dispatch('getReportSchema', { id: schemaId.value });
    const selectedSchema: ReportSchemaDTO = store.state.reportSchemaModule.selectedReportSchema;
    if (selectedSchema && selectedSchema.schemaJson) {
        try {
            schemaDefinition.value = JSON.parse(selectedSchema.schemaJson) as ReportSchemaDefinition;
        } catch (e) {
            store.dispatch('addErrorNotification', 'Failed to parse report template.');
        }
    }
});

onBeforeUnmount(() => {
    store.dispatch('clearViewerState');
});
</script>

<style scoped>
.report-viewer-outer {
    width: 100%;
    display: flex;
    justify-content: center;
    padding: 24px 16px;
    min-height: 100vh;
    background: #F5F6F8;
}

.report-viewer-container {
    width: 90%;
    max-width: 1800px;
}

.viewer-header {
    padding: 8px 0;
}

.viewer-title {
    font-size: 1.3rem;
    font-weight: 600;
    color: #002E6C;
    font-family: 'Montserrat', sans-serif;
    margin: 0;
}

.viewer-subtitle {
    font-size: 0.85rem;
    color: #666;
}

.viewer-page-tabs {
    border-bottom: 1px solid #E8E8E8;
}

.viewer-page-tabs :deep(.v-tab) {
    font-size: 13px !important;
    letter-spacing: 0 !important;
    min-width: 80px !important;
    text-transform: none !important;
}
</style>
