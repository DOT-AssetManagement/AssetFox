<template>
    <div>
        <div class="text-caption mb-2" style="color: #777;">
            Choose the scenario settings to include. Sections without data are omitted.
        </div>

        <div class="d-flex mb-1">
            <v-btn size="x-small" variant="text" class="px-1 mr-2" @click="selectAll">
                Select all
            </v-btn>
        </div>

        <v-checkbox
            v-for="section in scenarioParametersSectionOptions"
            :key="section.value"
            :model-value="localConfig.sections.includes(section.value)"
            :label="section.title"
            density="compact"
            hide-details
            @update:model-value="selected => updateSection(section.value, !!selected)"
        />

    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import {
    ScenarioParametersConfig,
    ScenarioParametersSectionKey,
    scenarioParametersSectionOptions,
} from '@/shared/models/iAM/report-schema';

const props = defineProps<{ config: ScenarioParametersConfig }>();
const emit = defineEmits<{ (e: 'update:config', val: ScenarioParametersConfig): void }>();
const localConfig = ref<ScenarioParametersConfig>(normalize(props.config));

watch(() => props.config, value => {
    localConfig.value = normalize(value);
}, { deep: true });

function normalize(config: ScenarioParametersConfig): ScenarioParametersConfig {
    return {
        sections: Array.isArray(config?.sections) ? [...config.sections] : [],
    };
}

function updateSection(section: ScenarioParametersSectionKey, selected: boolean) {
    const sections = new Set(localConfig.value.sections);
    if (selected) sections.add(section);
    else if (sections.size > 1) sections.delete(section);
    localConfig.value.sections = scenarioParametersSectionOptions
        .map(option => option.value)
        .filter(value => sections.has(value));
    emitUpdate();
}

function selectAll() {
    localConfig.value.sections = scenarioParametersSectionOptions.map(section => section.value);
    emitUpdate();
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
