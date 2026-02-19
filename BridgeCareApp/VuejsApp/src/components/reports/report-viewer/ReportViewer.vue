<template>
    <v-container fluid class="report-viewer-container">
        <!-- Header -->
        <v-row class="viewer-header" align="center">
            <v-col cols="auto">
                <v-btn
                    variant="text"
                    class="ghd-blue"
                    @click="goBack"
                >
                    <i class="fas fa-arrow-left mr-2" />
                    Back to Reports
                </v-btn>
            </v-col>
            <v-col>
                <h2 class="viewer-title">{{ schemaName }}</h2>
                <span class="viewer-subtitle" v-if="metadata && metadata.hasOutput">
                    {{ scenarioName }}
                    &middot; {{ metadata.assetCount.toLocaleString() }} assets
                    &middot; {{ metadata.years.length }} years
                </span>
                <span class="viewer-subtitle" v-else-if="!isLoadingMetadata">
                    No simulation output available
                </span>
            </v-col>
        </v-row>

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

        <!-- Component sections -->
        <template v-if="!isLoadingMetadata && metadata && metadata.hasOutput">
            <template v-for="component in visibleComponents" :key="component.id">
                <ViewerTextBlock
                    v-if="component.type === 'textBlock'"
                    :component="component"
                />
                <ViewerDataTable
                    v-else
                    :component="component"
                    :schemaId="schemaId"
                    :simulationId="simulationId"
                />
            </template>

            <v-alert
                v-if="visibleComponents.length === 0 && schemaDefinition"
                type="info"
                variant="tonal"
                class="mt-4"
            >
                This report template has no visible components. Open the Report Builder to add components.
            </v-alert>
        </template>
    </v-container>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
import { useStore } from 'vuex';
import { useRouter } from 'vue-router';
import { ReportSchemaDTO, ReportSchemaDefinition, ReportComponent } from '@/shared/models/iAM/report-schema';
import { ViewerMetadata } from '@/shared/models/iAM/viewer';
import ViewerDataTable from './ViewerDataTable.vue';
import ViewerTextBlock from './ViewerTextBlock.vue';

const store = useStore();
const router = useRouter();

// Route query params
const schemaId = ref('');
const simulationId = ref('');
const scenarioName = ref('');
const schemaName = ref('');

// Schema definition
const schemaDefinition = ref<ReportSchemaDefinition | null>(null);

// Store state
const metadata = computed<ViewerMetadata>(() => store.state.viewerModule.viewerMetadata);
const isLoadingMetadata = computed<boolean>(() => store.state.viewerModule.isLoadingMetadata);

// Visible components sorted by order
const visibleComponents = computed<ReportComponent[]>(() => {
    if (!schemaDefinition.value || !schemaDefinition.value.components) return [];
    return schemaDefinition.value.components
        .filter((c: ReportComponent) => c.visible !== false)
        .sort((a: ReportComponent, b: ReportComponent) => a.order - b.order);
});

function goBack() {
    router.push({
        path: '/ReportsAndOutputs/Scenario',
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
