<template>
    <div class="component-palette">
        <div class="text-caption text-uppercase mb-3 px-1" style="letter-spacing: 0.1em; color: #888; font-weight: 600;">
            Components
        </div>
        <div
            v-for="cType in componentTypes"
            :key="cType"
            class="palette-card mb-2"
            draggable="true"
            @dragstart="onDragStart($event, cType)"
            @click="$emit('add-component', cType)"
        >
            <div class="d-flex align-center">
                <div class="palette-icon-wrapper" :style="{ backgroundColor: info[cType].chipColor }">
                    <i :class="info[cType].icon" :style="{ color: info[cType].color, fontSize: '12px' }" />
                </div>
                <div class="ml-2 flex-grow-1" style="min-width: 0;">
                    <div class="palette-label">{{ info[cType].label }}</div>
                    <div class="palette-desc">{{ info[cType].description }}</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import {
    ComponentType,
    componentTypeInfo,
} from '@/shared/models/iAM/report-schema';

const info = componentTypeInfo;

const componentTypes: ComponentType[] = [
    'assetDetail',
    'treatmentTimeline',
    'workSummary',
    'budgetSummary',
    'conditionTracking',
    'fundedTreatmentList',
    'goalTracking',
    'textBlock',
];

defineEmits<{
    (e: 'add-component', type: ComponentType): void;
}>();

function onDragStart(event: DragEvent, type: ComponentType) {
    event.dataTransfer?.setData('componentType', type);
    event.dataTransfer!.effectAllowed = 'copy';
}
</script>

<style scoped>
.component-palette {
    padding: 4px;
}

.palette-card {
    padding: 10px 12px;
    border: 1px solid #E8E8E8;
    border-radius: 8px;
    cursor: pointer;
    background: #FFFFFF;
    transition: all 0.15s ease;
    user-select: none;
}

.palette-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
    border-color: #D0D5DD;
}

.palette-card:active {
    transform: translateY(0);
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
}

.palette-icon-wrapper {
    width: 32px;
    height: 32px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
}

.palette-label {
    font-size: 12px;
    font-weight: 600;
    color: #333;
    line-height: 1.2;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.palette-desc {
    font-size: 10px;
    color: #999;
    line-height: 1.3;
    margin-top: 1px;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
}
</style>
