<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Style</div>
        <v-btn-toggle
            v-model="localConfig.style"
            mandatory
            density="compact"
            variant="outlined"
            divided
            class="mb-3"
            @update:model-value="emitUpdate"
        >
            <v-btn value="heading" size="small" class="text-none">Heading</v-btn>
            <v-btn value="subheading" size="small" class="text-none">Subheading</v-btn>
            <v-btn value="body" size="small" class="text-none">Body</v-btn>
            <v-btn value="note" size="small" class="text-none">Note</v-btn>
        </v-btn-toggle>

        <v-divider class="my-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Content</div>
        <v-textarea
            v-model="localConfig.content"
            placeholder="Enter text content..."
            variant="outlined"
            density="compact"
            rows="4"
            auto-grow
            @update:model-value="emitUpdate"
        />

        <div class="text-block-preview mt-2 pa-3 rounded" :class="`preview-${localConfig.style}`">
            <div class="text-caption text-uppercase mb-1" style="letter-spacing: 0.08em; color: #999; font-size: 9px;">Preview</div>
            <div :class="previewClass">{{ localConfig.content || 'No content' }}</div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { clone } from 'ramda';
import { TextBlockConfig } from '@/shared/models/iAM/report-schema';

const props = defineProps<{ config: TextBlockConfig }>();
const emit = defineEmits<{ (e: 'update:config', val: TextBlockConfig): void }>();

const localConfig = ref<TextBlockConfig>(clone(props.config));

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

const previewClass = computed(() => {
    switch (localConfig.value.style) {
        case 'heading': return 'text-h6 font-weight-bold';
        case 'subheading': return 'text-subtitle-1 font-weight-medium';
        case 'body': return 'text-body-2';
        case 'note': return 'text-caption font-italic';
        default: return 'text-body-2';
    }
});

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>

<style scoped>
.text-block-preview {
    background-color: #FAFBFC;
    border: 1px dashed #D0D5DD;
    min-height: 48px;
}
.preview-heading { border-left: 3px solid #333; }
.preview-subheading { border-left: 3px solid #666; }
.preview-body { border-left: 3px solid #999; }
.preview-note { border-left: 3px solid #CCC; }
</style>
