<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Goal Type</div>
        <v-btn-toggle
            v-model="localConfig.goalType"
            mandatory
            density="compact"
            variant="outlined"
            divided
            class="mb-3"
            @update:model-value="emitUpdate"
        >
            <v-btn value="target" size="small" class="text-none">
                <i class="fas fa-bullseye mr-2" style="font-size: 10px;" />
                Target Goals
            </v-btn>
            <v-btn value="deficient" size="small" class="text-none">
                <i class="fas fa-exclamation-triangle mr-2" style="font-size: 10px;" />
                Deficient Goals
            </v-btn>
        </v-btn-toggle>

        <v-divider class="my-3" />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Display Options</div>
        <v-checkbox
            v-model="localConfig.showGoalName"
            label="Show goal name"
            density="compact"
            hide-details
            class="mb-1"
            @update:model-value="emitUpdate"
        />
        <v-checkbox
            v-model="localConfig.showActualVsTarget"
            label="Show actual vs. target values"
            density="compact"
            hide-details
            class="mb-1"
            @update:model-value="emitUpdate"
        />
        <v-checkbox
            v-model="localConfig.showMetStatus"
            label="Show met / unmet status"
            density="compact"
            hide-details
            @update:model-value="emitUpdate"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import { GoalTrackingConfig } from '@/shared/models/iAM/report-schema';

const props = defineProps<{ config: GoalTrackingConfig }>();
const emit = defineEmits<{ (e: 'update:config', val: GoalTrackingConfig): void }>();

const localConfig = ref<GoalTrackingConfig>(clone(props.config));

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
