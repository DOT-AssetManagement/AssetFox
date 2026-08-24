<template>
    <div class="page-viewer-grid">
        <template v-for="component in sortedComponents" :key="component.id">
            <div
                class="page-viewer-cell"
                :style="cellStyle(component)"
            >
                <ViewerTextBlock
                    v-if="component.type === 'textBlock'"
                    :component="component"
                />
                <ViewerScenarioParameters
                    v-else-if="component.type === 'scenarioParameters'"
                    :component="component"
                    :schemaId="schemaId"
                    :simulationId="simulationId"
                />
                <ViewerChart
                    v-else-if="isChartComponentType(component.type)"
                    :component="component"
                    :schemaId="schemaId"
                    :simulationId="simulationId"
                />
                <ViewerDataTable
                    v-else
                    :component="component"
                    :schemaId="schemaId"
                    :simulationId="simulationId"
                />
            </div>
        </template>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { ReportComponent, isChartComponentType } from '@/shared/models/iAM/report-schema';
import ViewerDataTable from './ViewerDataTable.vue';
import ViewerTextBlock from './ViewerTextBlock.vue';
import ViewerScenarioParameters from './ViewerScenarioParameters.vue';
import ViewerChart from './ViewerChart.vue';

const props = defineProps<{
    components: ReportComponent[];
    schemaId: string;
    simulationId: string;
}>();

// Sort by gridY then gridX for document flow.
// The `visible` flag controls Excel export inclusion only — the live viewer always renders every component.
const sortedComponents = computed(() => {
    return [...props.components]
        .sort((a, b) => {
            if (a.gridY !== b.gridY) return a.gridY - b.gridY;
            return a.gridX - b.gridX;
        });
});

function cellStyle(comp: ReportComponent) {
    return {
        gridColumn: `${(comp.gridX || 0) + 1} / span ${comp.gridW || 12}`,
    };
}
</script>

<style scoped>
.page-viewer-grid {
    display: grid;
    grid-template-columns: repeat(12, 1fr);
    gap: 16px;
    padding: 16px 0;
}

.page-viewer-cell {
    min-width: 0;
}

/* Remove bottom margin from viewer cards since grid gap handles spacing */
.page-viewer-cell :deep(.viewer-card) {
    margin-bottom: 0;
}

.page-viewer-cell :deep(.viewer-text-block) {
    margin-bottom: 0;
}
</style>
