<template>
    <v-card class="elevation-0 mb-4" style="border: 1px solid lightgray; padding: 10px;">
        <v-card-title class="ghd-control-label ghd-md-gray" style="padding: 0; font-size: 14px;">
            Rule Overrides
        </v-card-title>
        <v-card-text style="padding: 4px 0;">
            <div v-if="ruleOverrides.length === 0" class="ghd-md-gray" style="font-size: 13px;">
                No rule overrides defined on this library.
            </div>
            <div v-for="ruleOverride in ruleOverrides" :key="ruleOverride.id" style="margin-bottom: 4px;">
                <v-switch
                    color="#2A578D"
                    density="compact"
                    hide-details
                    :model-value="isOverrideEnabled(ruleOverride)"
                    @update:model-value="onToggle(ruleOverride, $event)"
                >
                    <template v-slot:label>
                        <div>
                            <span class="ghd-control-text">{{ ruleOverride.name }}</span>
                            <v-icon
                                v-if="hasUnmatchedTreatments(ruleOverride)"
                                color="orange"
                                size="small"
                                style="margin-left: 6px;"
                                :title="getUnmatchedWarning(ruleOverride)"
                            >
                                fas fa-exclamation-triangle
                            </v-icon>
                            <div v-if="ruleOverride.description" class="ghd-md-gray" style="font-size: 12px;">
                                {{ ruleOverride.description }}
                            </div>
                        </div>
                    </template>
                </v-switch>
            </div>
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
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

function hasUnmatchedTreatments(ruleOverride: TreatmentRuleOverride): boolean {
    const treatmentIds = new Set(props.treatments.map(treatment => treatment.id.toLowerCase()));
    return ruleOverride.criteria.some(
        c => getCriterionTreatmentIds(c).some(treatmentId => !treatmentIds.has(treatmentId.toLowerCase()))
    );
}

function getUnmatchedWarning(ruleOverride: TreatmentRuleOverride): string {
    const treatmentIds = new Set(props.treatments.map(treatment => treatment.id.toLowerCase()));
    const unmatched = ruleOverride.criteria
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
