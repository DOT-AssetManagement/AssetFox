<template>
    <v-card class="viewer-card viewer-text-block elevation-1 mb-4">
        <v-card-text :class="textClass">
            {{ textContent }}
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { ReportComponent } from '@/shared/models/iAM/report-schema';

const props = defineProps<{
    component: ReportComponent;
}>();

const textContent = computed(() => {
    if (!props.component.config) return '';
    const config = props.component.config as Record<string, any>;
    return config.content || '';
});

const textStyle = computed(() => {
    if (!props.component.config) return 'body';
    const config = props.component.config as Record<string, any>;
    return config.style || 'body';
});

const textClass = computed(() => {
    switch (textStyle.value) {
        case 'heading':
            return 'text-block-heading';
        case 'subheading':
            return 'text-block-subheading';
        case 'note':
            return 'text-block-note';
        case 'body':
        default:
            return 'text-block-body';
    }
});
</script>

<style scoped>
.viewer-text-block {
    border-left: 4px solid #2A578D;
}

.text-block-heading {
    font-size: 1.4rem;
    font-weight: 600;
    color: #002E6C;
    font-family: 'Montserrat', sans-serif;
}

.text-block-subheading {
    font-size: 1.1rem;
    font-weight: 500;
    color: #333333;
    font-family: 'Montserrat', sans-serif;
}

.text-block-body {
    font-size: 0.95rem;
    font-weight: 400;
    color: #333333;
    line-height: 1.6;
}

.text-block-note {
    font-size: 0.85rem;
    font-weight: 400;
    color: #666666;
    font-style: italic;
}
</style>
