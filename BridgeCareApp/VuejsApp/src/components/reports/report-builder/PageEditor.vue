<template>
    <div class="page-editor" ref="editorEl">
        <div ref="gridEl" class="grid-stack"></div>

        <!-- Teleport Vue content into each gridstack-created widget -->
        <template v-for="tp in teleportTargets" :key="tp.compId">
            <Teleport :to="tp.selector" v-if="tp.mounted">
                <GridComponentWrapper
                    :component="getComponent(tp.compId)!"
                    :selected="selectedId === tp.compId"
                    @select="$emit('select', tp.compId)"
                    @delete="$emit('delete', tp.compId)"
                />
            </Teleport>
        </template>

        <!-- Empty state -->
        <div v-if="components.length === 0" class="page-editor-empty">
            <div class="page-editor-empty__inner">
                <i class="fas fa-layer-group page-editor-empty__icon" />
                <div class="page-editor-empty__title">Start building your report</div>
                <div class="page-editor-empty__desc">
                    Drag a component from the palette onto this page
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onBeforeUnmount, watch, nextTick } from 'vue';
import { GridStack, GridStackNode } from 'gridstack';
import 'gridstack/dist/gridstack.min.css';
import {
    ReportComponent,
    ComponentType,
    createDefaultComponent,
    pageSizeToGridH,
} from '@/shared/models/iAM/report-schema';
import GridComponentWrapper from './GridComponentWrapper.vue';

const props = defineProps<{
    components: ReportComponent[];
    selectedId: string;
}>();

const emit = defineEmits<{
    (e: 'update:components', val: ReportComponent[]): void;
    (e: 'select', id: string): void;
    (e: 'delete', id: string): void;
    (e: 'component-dropped', comp: ReportComponent): void;
}>();

const editorEl = ref<HTMLElement | null>(null);
const gridEl = ref<HTMLElement | null>(null);

let grid: GridStack | null = null;
let suppressChangeEvent = false;

// Teleport tracking: maps component IDs to their DOM mount targets
const teleportTargets = reactive<{ compId: string; selector: string; mounted: boolean }[]>([]);

function getComponent(id: string): ReportComponent | undefined {
    return props.components.find(c => c.id === id);
}

function addWidgetToGrid(comp: ReportComponent) {
    if (!grid) return;

    const contentId = `gs-content-${comp.id}`;

    // Create widget via gridstack API
    const el = grid.addWidget({
        id: comp.id,
        x: comp.gridX,
        y: comp.gridY,
        w: comp.gridW,
        h: comp.gridH,
        minW: 3,
        noResize: false,
        content: `<div id="${contentId}" class="gs-vue-content"></div>`,
    });

    // Track teleport target
    nextTick(() => {
        const existing = teleportTargets.find(t => t.compId === comp.id);
        if (!existing) {
            teleportTargets.push({
                compId: comp.id,
                selector: `#${contentId}`,
                mounted: true,
            });
        } else {
            existing.mounted = true;
        }
    });
}

function removeWidgetFromGrid(compId: string) {
    if (!grid) return;
    const el = grid.el.querySelector(`[gs-id="${compId}"]`) as HTMLElement;
    if (el) {
        grid.removeWidget(el, true, false);
    }
    // Clean up teleport
    const idx = teleportTargets.findIndex(t => t.compId === compId);
    if (idx >= 0) {
        teleportTargets.splice(idx, 1);
    }
}

function initGrid() {
    if (!gridEl.value) return;

    // Override gridstack's default textContent renderer so our HTML content
    // string (Teleport target div) is rendered as actual DOM, not literal text.
    // This is the documented approach for framework integrations.
    GridStack.renderCB = (el, w) => { el.innerHTML = w.content ?? ''; };

    grid = GridStack.init({
        column: 12,
        cellHeight: 80,
        margin: 6,
        animate: true,
        float: false,
        acceptWidgets: '.palette-card',
        draggable: {
            handle: '.grid-component-header',
        },
        resizable: {
            handles: 'e, w',
        },
    }, gridEl.value);

    // Sync position changes back to data
    grid.on('change', (_event: Event, nodes: GridStackNode[]) => {
        if (suppressChangeEvent) return;

        const updates: Record<string, { x: number; y: number; w: number; h: number }> = {};
        nodes.forEach((node) => {
            if (node.id) {
                updates[node.id as string] = {
                    x: node.x ?? 0,
                    y: node.y ?? 0,
                    w: node.w ?? 12,
                    h: node.h ?? 2,
                };
            }
        });

        if (Object.keys(updates).length === 0) return;

        const updated = props.components.map(c => {
            const u = updates[c.id];
            if (u) {
                return { ...c, gridX: u.x, gridY: u.y, gridW: u.w, gridH: u.h };
            }
            return c;
        });

        emit('update:components', updated);
    });

    // Handle external drops from palette
    grid.on('dropped', (_event: Event, previousNode: GridStackNode, newNode: GridStackNode) => {
        // Extract the component type — check both the new element and the previous source element
        const el = newNode.el;
        if (!el) return;
        let compType = el.getAttribute('data-gs-type') as ComponentType | null;
        if (!compType && previousNode?.el) {
            compType = previousNode.el.getAttribute('data-gs-type') as ComponentType | null;
        }
        if (!compType) return;

        // Remove the temporary dropped element — we'll re-add via our data flow
        grid!.removeWidget(el, true, false);

        // Create the real component at the position gridstack chose
        const newComp = createDefaultComponent(compType);
        newComp.gridX = newNode.x ?? 0;
        newComp.gridY = newNode.y ?? 0;
        newComp.gridW = newNode.w ?? 12;
        newComp.gridH = pageSizeToGridH(newComp.pageSize);

        emit('component-dropped', newComp);
    });

    // Load existing components
    suppressChangeEvent = true;
    grid.batchUpdate(true);
    props.components.forEach(comp => addWidgetToGrid(comp));
    grid.batchUpdate(false);
    suppressChangeEvent = false;
}

// Watch for component additions/removals
watch(() => props.components, (newComps, oldComps) => {
    if (!grid) return;
    const oldIds = new Set((oldComps || []).map(c => c.id));
    const newIds = new Set(newComps.map(c => c.id));

    // Find added components
    const added = newComps.filter(c => !oldIds.has(c.id));
    // Find removed component IDs
    const removed = (oldComps || []).filter(c => !newIds.has(c.id));

    if (added.length === 0 && removed.length === 0) {
        // Only property changes — sync sizes
        syncWidgetProperties(newComps);
        return;
    }

    suppressChangeEvent = true;
    grid.batchUpdate(true);

    removed.forEach(c => removeWidgetFromGrid(c.id));
    added.forEach(c => addWidgetToGrid(c));

    grid.batchUpdate(false);
    suppressChangeEvent = false;
}, { deep: false });

// Sync non-position properties (like gridH from pageSize changes)
function syncWidgetProperties(comps: ReportComponent[]) {
    if (!grid) return;
    suppressChangeEvent = true;

    comps.forEach(comp => {
        const el = grid!.el.querySelector(`[gs-id="${comp.id}"]`) as HTMLElement;
        if (el) {
            const node = (el as any).gridstackNode as GridStackNode | undefined;
            if (node) {
                const expectedH = pageSizeToGridH(comp.pageSize);
                if (node.h !== expectedH) {
                    grid!.update(el, { h: expectedH });
                }
            }
        }
    });

    suppressChangeEvent = false;
}

// Watch for deep property changes (e.g. pageSize changes from properties panel)
watch(() => props.components.map(c => c.pageSize), () => {
    if (!grid) return;
    syncWidgetProperties(props.components);
}, { deep: true });

onMounted(() => {
    nextTick(() => {
        initGrid();

        // Setup external drag from palette
        // This must run after grid init. The palette cards have .palette-card class
        // and data-gs-type attribute for identifying component type
        GridStack.setupDragIn('.palette-card', {
            handle: '.palette-card',
            appendTo: 'body',
        });
    });
});

onBeforeUnmount(() => {
    teleportTargets.splice(0, teleportTargets.length);
    if (grid) {
        grid.offAll();
        grid.destroy(false);
        grid = null;
    }
});
</script>

<style>
/* Gridstack overrides - NOT scoped so they apply to gridstack-managed DOM */
.page-editor {
    min-height: 100%;
    position: relative;
    border-radius: 10px;
    background-color: #FAFBFC;
    background-image: radial-gradient(circle, #D0D5DD 1px, transparent 1px);
    background-size: 20px 20px;
    box-shadow: inset 0 1px 3px rgba(0, 0, 0, 0.04);
    padding: 8px;
}

.page-editor .grid-stack {
    min-height: 400px;
}

.page-editor .grid-stack-item-content {
    border-radius: 10px;
    overflow: visible;
    inset: 4px;
}

.page-editor .gs-vue-content {
    height: 100%;
    width: 100%;
}

/* Placeholder styling during drag */
.page-editor .grid-stack-placeholder > .placeholder-content {
    border: 2px dashed #2A578D !important;
    border-radius: 10px !important;
    background: rgba(42, 87, 141, 0.06) !important;
    inset: 4px !important;
}

/* While dragging — lift effect on the dragged item */
.page-editor .grid-stack-item.ui-draggable-dragging {
    opacity: 0.9;
    z-index: 100 !important;
}

.page-editor .grid-stack-item.ui-draggable-dragging .grid-component-wrapper {
    box-shadow: 0 12px 36px rgba(0, 0, 0, 0.15), 0 0 0 1px rgba(42, 87, 141, 0.2);
    transform: scale(1.02);
}

/* Resize handle styling — east/west only */
.page-editor .grid-stack-item > .ui-resizable-e,
.page-editor .grid-stack-item > .ui-resizable-w {
    width: 8px;
    opacity: 0;
    transition: opacity 0.2s ease;
    background: transparent;
    z-index: 10;
}

.page-editor .grid-stack-item:hover > .ui-resizable-e,
.page-editor .grid-stack-item:hover > .ui-resizable-w {
    opacity: 1;
}

.page-editor .grid-stack-item > .ui-resizable-e {
    right: 0;
    cursor: e-resize;
}

.page-editor .grid-stack-item > .ui-resizable-w {
    left: 0;
    cursor: w-resize;
}

.page-editor .grid-stack-item > .ui-resizable-e::after,
.page-editor .grid-stack-item > .ui-resizable-w::after {
    content: '';
    position: absolute;
    top: 50%;
    transform: translateY(-50%);
    width: 4px;
    height: 32px;
    border-radius: 2px;
    background: #2A578D;
    opacity: 0.5;
    transition: opacity 0.15s ease;
}

.page-editor .grid-stack-item > .ui-resizable-e::after {
    right: 2px;
}

.page-editor .grid-stack-item > .ui-resizable-w::after {
    left: 2px;
}

.page-editor .grid-stack-item:hover > .ui-resizable-e::after,
.page-editor .grid-stack-item:hover > .ui-resizable-w::after {
    opacity: 0.8;
}

/* While resizing */
.page-editor .grid-stack-item.ui-resizable-resizing {
    transition: none !important;
}

.page-editor .grid-stack-item.ui-resizable-resizing .grid-component-wrapper {
    border-color: #2A578D;
    box-shadow: 0 0 0 1px #2A578D;
}

/* Drop animation — new widget entering */
@keyframes gsWidgetEnter {
    0% {
        opacity: 0;
        transform: scale(0.92);
    }
    100% {
        opacity: 1;
        transform: scale(1);
    }
}

.page-editor .grid-stack-item .grid-component-wrapper {
    animation: gsWidgetEnter 0.25s cubic-bezier(0.25, 0.1, 0.25, 1) both;
}
</style>

<style scoped>
.page-editor-empty {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 400px;
    position: absolute;
    inset: 0;
    pointer-events: none;
}

.page-editor-empty__inner {
    text-align: center;
    padding: 32px;
}

.page-editor-empty__icon {
    font-size: 40px;
    color: #D0D5DD;
    margin-bottom: 16px;
}

.page-editor-empty__title {
    font-size: 16px;
    font-weight: 600;
    color: #888;
    margin-bottom: 6px;
}

.page-editor-empty__desc {
    font-size: 13px;
    color: #B0B0B0;
    max-width: 240px;
    margin: 0 auto;
    line-height: 1.5;
}
</style>
