<template>
    <div>
        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Group By</div>
        <v-autocomplete
            v-model="localConfig.groupByAttribute"
            :items="attributeNames"
            label="Group rows by attribute"
            variant="outlined"
            density="compact"
            class="mb-3"
            clearable
            @update:model-value="emitUpdate"
        />

        <div class="text-caption text-uppercase mb-2" style="letter-spacing: 0.08em; color: #666;">Aggregation</div>
        <v-row dense class="mb-3">
            <v-col :cols="showFunctionPicker ? 6 : 12">
                <v-select
                    v-model="localConfig.aggregateField"
                    :items="aggregateFieldOptions"
                    label="Value to aggregate"
                    variant="outlined"
                    density="compact"
                    @update:model-value="onAggregateFieldChange"
                />
            </v-col>
            <v-col v-if="showFunctionPicker" cols="6">
                <v-select
                    v-model="localConfig.aggregateFunction"
                    :items="availableFunctionOptions"
                    label="Function"
                    variant="outlined"
                    density="compact"
                    @update:model-value="emitUpdate"
                />
            </v-col>
        </v-row>

        <v-checkbox
            v-model="localConfig.includePercentages"
            label="Include percentage columns"
            density="compact"
            hide-details
            @update:model-value="emitUpdate"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { clone } from 'ramda';
import { useStore } from 'vuex';
import { WorkSummaryConfig, AggregateFunction, aggregateFunctionOptions } from '@/shared/models/iAM/report-schema';

const props = defineProps<{ config: WorkSummaryConfig }>();
const emit = defineEmits<{ (e: 'update:config', val: WorkSummaryConfig): void }>();

const store = useStore();
const localConfig = ref<WorkSummaryConfig>(clone(props.config));

watch(() => props.config, (val) => { localConfig.value = clone(val); }, { deep: true });

const attributeNames = computed(() => {
    const attrs = store.state.attributeModule?.attributes ?? [];
    return attrs.map((a: any) => a.name).sort();
});

const aggregateFieldOptions = [
    { title: 'Treatment Cost', value: 'cost' },
    { title: 'Asset Count', value: 'count' },
];

// Which aggregation functions are valid for each field. Asset Count is always
// COUNT(*), so the function picker is hidden for that field entirely.
const validFunctionsByField: Record<string, AggregateFunction[]> = {
    cost: ['sum', 'avg', 'min', 'max'],
    count: ['count'],
};

const showFunctionPicker = computed(() => localConfig.value.aggregateField !== 'count');

const availableFunctionOptions = computed(() => {
    const allowed = validFunctionsByField[localConfig.value.aggregateField] ?? [];
    return aggregateFunctionOptions.filter((opt) => allowed.includes(opt.value));
});

function onAggregateFieldChange() {
    const allowed = validFunctionsByField[localConfig.value.aggregateField] ?? [];
    if (!allowed.includes(localConfig.value.aggregateFunction)) {
        localConfig.value.aggregateFunction = allowed[0];
    }
    emitUpdate();
}

function emitUpdate() {
    emit('update:config', clone(localConfig.value));
}
</script>
