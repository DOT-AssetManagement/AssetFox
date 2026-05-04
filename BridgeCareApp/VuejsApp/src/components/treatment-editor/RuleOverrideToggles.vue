<template>
    <v-card class="rule-override-toggle-card elevation-0 mb-4">
        <div class="rule-override-toggle-header">
            <div>
                <div class="rule-override-toggle-title">Rule Overrides</div>
                <div class="rule-override-toggle-summary">{{ toggleSummary }}</div>
            </div>
        </div>
        <v-card-text class="rule-override-toggle-body">
            <div v-if="ruleOverrideItems.length === 0" class="rule-override-toggle-empty">
                No rule overrides on this library
            </div>
            <div v-else class="rule-override-toggle-list">
                <div
                    v-for="ruleOverride in ruleOverrideItems"
                    :key="ruleOverride.id || ruleOverride.name"
                    class="rule-override-toggle-item"
                    :class="{ 'rule-override-toggle-item--enabled': isOverrideEnabled(ruleOverride) }"
                >
                    <div class="rule-override-toggle-copy">
                        <div class="rule-override-toggle-name-row">
                            <span class="rule-override-toggle-name">{{ getOverrideName(ruleOverride) }}</span>
                            <v-icon
                                v-if="hasUnmatchedTreatments(ruleOverride)"
                                color="#B26A00"
                                size="small"
                                class="rule-override-warning-icon"
                                :title="getUnmatchedWarning(ruleOverride)"
                            >
                                fas fa-exclamation-triangle
                            </v-icon>
                            <span
                                class="rule-override-toggle-state"
                                :class="{ 'rule-override-toggle-state--off': !isOverrideEnabled(ruleOverride) }"
                            >
                                {{ isOverrideEnabled(ruleOverride) ? 'Enabled' : 'Disabled' }}
                            </span>
                        </div>
                        <div v-if="ruleOverride.description" class="rule-override-toggle-description">
                            {{ ruleOverride.description }}
                        </div>
                    </div>
                    <v-switch
                        color="#2A578D"
                        density="compact"
                        inset
                        hide-details
                        :aria-label="getOverrideName(ruleOverride)"
                        :model-value="isOverrideEnabled(ruleOverride)"
                        @update:model-value="onToggle(ruleOverride, $event)"
                    />
                </div>
            </div>
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch } from 'vue';
import { TreatmentRuleOverride, ScenarioOverrideSetting } from '@/shared/models/iAM/treatment-rule-override';
import { SimpleTreatment } from '@/shared/models/iAM/treatment';
import TreatmentRuleOverrideService from '@/services/treatment-rule-override.service';

const props = defineProps<{
    ruleOverrides: TreatmentRuleOverride[];
    scenarioId: string;
    treatments: SimpleTreatment[];
}>();

const emit = defineEmits(['changed']);

const scenarioSettings = ref<ScenarioOverrideSetting[]>([]);

const ruleOverrideItems = computed(() => props.ruleOverrides || []);
const enabledOverrideCount = computed(() =>
    ruleOverrideItems.value.filter(ruleOverride => isOverrideEnabled(ruleOverride)).length,
);
const toggleSummary = computed(() => {
    const totalCount = ruleOverrideItems.value.length;
    if (totalCount === 0) {
        return '0 overrides';
    }

    const overrideLabel = totalCount === 1 ? 'override' : 'overrides';
    return `${enabledOverrideCount.value} of ${totalCount} ${overrideLabel} enabled`;
});

onMounted(async () => {
    if (props.scenarioId) {
        await loadSettings();
    }
});

watch(() => props.scenarioId, async (newVal) => {
    if (newVal) {
        await loadSettings();
    }
});

async function loadSettings() {
    try {
        const response = await TreatmentRuleOverrideService.getScenarioOverrideSettings(props.scenarioId);
        scenarioSettings.value = response.data || [];
    } catch {
        scenarioSettings.value = [];
    }
}

function isOverrideEnabled(ruleOverride: TreatmentRuleOverride): boolean {
    const setting = scenarioSettings.value.find(s => s.overrideId === ruleOverride.id || s.overrideName === ruleOverride.name);
    return setting ? setting.isEnabled : ruleOverride.isEnabled ?? ruleOverride.isEnabledByDefault;
}

function onToggle(ruleOverride: TreatmentRuleOverride, enabled: boolean) {
    const changedSetting = { overrideId: ruleOverride.id, overrideName: ruleOverride.name, isEnabled: enabled };
    const existingIndex = scenarioSettings.value.findIndex(s => s.overrideId === ruleOverride.id || s.overrideName === ruleOverride.name);
    if (existingIndex >= 0) {
        scenarioSettings.value[existingIndex] = {
            ...scenarioSettings.value[existingIndex],
            ...changedSetting,
        };
    } else {
        scenarioSettings.value.push(changedSetting);
    }

    emit('changed', changedSetting);
}

function getOverrideName(ruleOverride: TreatmentRuleOverride): string {
    return ruleOverride.name?.trim() || 'Untitled override';
}

function hasUnmatchedTreatments(ruleOverride: TreatmentRuleOverride): boolean {
    const treatmentIds = new Set((props.treatments || []).map(treatment => treatment.id.toLowerCase()));
    return (ruleOverride.criteria || []).some(
        c => getCriterionTreatmentIds(c).some(treatmentId => !treatmentIds.has(treatmentId.toLowerCase()))
    );
}

function getUnmatchedWarning(ruleOverride: TreatmentRuleOverride): string {
    const treatmentIds = new Set((props.treatments || []).map(treatment => treatment.id.toLowerCase()));
    const unmatched = (ruleOverride.criteria || [])
        .flatMap(c => getCriterionTreatmentIds(c)
            .filter(treatmentId => !treatmentIds.has(treatmentId.toLowerCase())));
    return `This override references treatment(s) not found in the current treatment list: ${unmatched.join(', ')}. It may have no effect.`;
}

function getCriterionTreatmentIds(criterion: TreatmentRuleOverride['criteria'][number]): string[] {
    if (criterion.treatmentIds?.length) {
        return criterion.treatmentIds;
    }

    if (criterion.treatments?.length) {
        return criterion.treatments.map(treatment => treatment.id);
    }

    if (criterion.treatmentName) {
        const treatment = props.treatments.find(item => item.name.toLowerCase() === criterion.treatmentName?.toLowerCase());
        return treatment ? [treatment.id] : [];
    }

    return [];
}
</script>

<style scoped>
.rule-override-toggle-card {
    border: 1px solid #d8dee7;
    border-radius: 8px;
    overflow: hidden;
}

.rule-override-toggle-header {
    align-items: center;
    background: #f7f9fc;
    border-bottom: 1px solid #e8edf3;
    display: flex;
    justify-content: space-between;
    padding: 12px 16px;
}

.rule-override-toggle-title {
    color: #333333;
    font-family: 'Montserrat', sans-serif;
    font-size: 14px;
    font-weight: 600;
    line-height: 20px;
}

.rule-override-toggle-summary,
.rule-override-toggle-description,
.rule-override-toggle-empty {
    color: #666666;
    font-family: 'Montserrat', sans-serif;
    font-size: 12px;
    line-height: 18px;
}

.rule-override-toggle-body {
    padding: 10px 16px 14px;
}

.rule-override-toggle-empty {
    background: #fbfcfe;
    border: 1px dashed #cfd8e3;
    border-radius: 8px;
    padding: 12px 14px;
}

.rule-override-toggle-list {
    display: grid;
    gap: 8px;
}

.rule-override-toggle-item {
    align-items: center;
    background: #ffffff;
    border: 1px solid #e1e7ef;
    border-radius: 8px;
    display: flex;
    gap: 12px;
    justify-content: space-between;
    min-height: 56px;
    padding: 8px 10px 8px 12px;
}

.rule-override-toggle-item--enabled {
    border-color: #b8cfe6;
    box-shadow: inset 3px 0 0 #2A578D;
}

.rule-override-toggle-copy {
    min-width: 0;
}

.rule-override-toggle-name-row {
    align-items: center;
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.rule-override-toggle-name {
    color: #333333;
    font-family: 'Montserrat', sans-serif;
    font-size: 14px;
    font-weight: 600;
    line-height: 20px;
}

.rule-override-warning-icon {
    margin-left: 2px;
}

.rule-override-toggle-state {
    background: #edf4fb;
    border: 1px solid #c8d8e8;
    border-radius: 999px;
    color: #2A578D;
    font-family: 'Montserrat', sans-serif;
    font-size: 11px;
    font-weight: 600;
    line-height: 18px;
    padding: 0 8px;
}

.rule-override-toggle-state--off {
    background: #f4f5f7;
    border-color: #d8dde3;
    color: #666666;
}

.rule-override-toggle-description {
    margin-top: 2px;
}

.rule-override-toggle-item :deep(.v-input) {
    flex: 0 0 auto;
}

@media (max-width: 720px) {
    .rule-override-toggle-item {
        align-items: flex-start;
        flex-direction: column;
    }
}
</style>
