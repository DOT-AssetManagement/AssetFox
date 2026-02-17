<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Display Options</div>
        <p class="text-caption text-grey mb-3">Choose which budget metrics to show for each year.</p>

        <v-checkbox
            v-model="localConfig.showAvailableFunding"
            label="Available funding"
            density="compact"
            hide-details
            class="mb-1"
            @update:model-value="emitUpdate"
        />
        <v-checkbox
            v-model="localConfig.showSpentAmount"
            label="Amount spent"
            density="compact"
            hide-details
            class="mb-1"
            @update:model-value="emitUpdate"
        />
        <v-checkbox
            v-model="localConfig.showRemainingAmount"
            label="Remaining amount"
            density="compact"
            hide-details
            @update:model-value="emitUpdate"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import { BudgetSummaryConfig } from '@/shared/models/iAM/report-schema';

const props = defineProps<{ config: BudgetSummaryConfig }>();
const emit = defineEmits<{ (e: 'update:config', val: BudgetSummaryConfig): void }>();

const localConfig = ref<BudgetSummaryConfig>(clone(props.config));

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
