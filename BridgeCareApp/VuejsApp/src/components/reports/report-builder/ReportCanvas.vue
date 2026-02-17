<template>
    <div
        class="report-canvas"
        @dragover.prevent="onDragOver"
        @drop.prevent="onDrop"
    >
        <draggable
            v-if="components.length > 0"
            :list="localComponents"
            item-key="id"
            handle=".drag-handle"
            :animation="200"
            ghost-class="canvas-ghost"
            @change="onDragChange"
        >
            <template #item="{ element }">
                <div class="canvas-item">
                    <ComponentCard
                        :component="element"
                        :selected="selectedId === element.id"
                        @select="$emit('select', element.id)"
                        @delete="$emit('delete', element.id)"
                    />
                </div>
            </template>
        </draggable>

        <!-- Empty state -->
        <div v-else class="canvas-empty">
            <div class="canvas-empty__inner">
                <i class="fas fa-layer-group canvas-empty__icon" />
                <div class="canvas-empty__title">Start building your report</div>
                <div class="canvas-empty__desc">
                    Click or drag a component from the palette to add it here
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import draggable from 'vuedraggable';
import {
    ReportComponent,
    ComponentType,
    createDefaultComponent,
} from '@/shared/models/iAM/report-schema';
import ComponentCard from './ComponentCard.vue';

const props = defineProps<{
    components: ReportComponent[];
    selectedId: string;
}>();

const emit = defineEmits<{
    (e: 'update:components', val: ReportComponent[]): void;
    (e: 'select', id: string): void;
    (e: 'delete', id: string): void;
    (e: 'add-component', type: ComponentType): void;
}>();

const localComponents = ref<ReportComponent[]>(clone(props.components));

watch(() => props.components, (val) => {
    localComponents.value = clone(val);
}, { deep: true });

function onDragChange() {
    localComponents.value.forEach((c, i) => { c.order = i; });
    emit('update:components', clone(localComponents.value));
}

function onDragOver(event: DragEvent) {
    if (event.dataTransfer?.types.includes('componenttype')) {
        event.dataTransfer.dropEffect = 'copy';
    }
}

function onDrop(event: DragEvent) {
    const type = event.dataTransfer?.getData('componentType') as ComponentType | undefined;
    if (type) {
        emit('add-component', type);
    }
}
</script>

<style scoped>
.report-canvas {
    min-height: 100%;
    padding: 16px;
    border-radius: 10px;
    background-color: #FAFBFC;
    background-image: radial-gradient(circle, #D0D5DD 1px, transparent 1px);
    background-size: 20px 20px;
    box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.04);
}

.canvas-item {
    margin-bottom: 10px;
}

.canvas-ghost {
    opacity: 0.4;
    border: 2px dashed #2A578D !important;
    border-radius: 10px;
}

.canvas-empty {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 400px;
}

.canvas-empty__inner {
    text-align: center;
    padding: 32px;
}

.canvas-empty__icon {
    font-size: 40px;
    color: #D0D5DD;
    margin-bottom: 16px;
}

.canvas-empty__title {
    font-size: 16px;
    font-weight: 600;
    color: #888;
    margin-bottom: 6px;
}

.canvas-empty__desc {
    font-size: 13px;
    color: #B0B0B0;
    max-width: 240px;
    margin: 0 auto;
    line-height: 1.5;
}
</style>
