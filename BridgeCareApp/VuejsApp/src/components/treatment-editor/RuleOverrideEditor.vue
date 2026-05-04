<template>
    <v-card class="elevation-0 mb-4" style="border: 1px solid lightgray; padding: 10px;">
        <v-card-title class="ghd-control-label ghd-md-gray" style="padding: 0; font-size: 14px; display: flex; justify-content: space-between; align-items: center;">
            Rule Overrides
            <v-btn
                variant="flat"
                class="ghd-white-bg ghd-blue ghd-button-text ghd-blue-border"
                size="small"
                @click="addOverride"
            >
                <span class="ghd-right-padding">Add Override</span>
                <v-icon size="small">fas fa-plus</v-icon>
            </v-btn>
        </v-card-title>

        <v-card-text style="padding: 8px 0;">
            <div v-if="localOverrides.length === 0" class="ghd-md-gray" style="font-size: 13px; padding: 8px 0;">
                No rule overrides defined. Add an override to allow scenario users to toggle extra feasibility criteria.
            </div>

            <v-expansion-panels v-model="expandedPanel" variant="accordion">
                <v-expansion-panel
                    v-for="(override, oIndex) in localOverrides"
                    :key="override.id"
                >
                    <v-expansion-panel-title>
                        <div style="display: flex; align-items: center; width: 100%; justify-content: space-between;">
                            <span class="ghd-control-text">{{ override.name || '(unnamed override)' }}</span>
                            <v-btn
                                icon
                                variant="text"
                                size="small"
                                color="red"
                                @click.stop="removeOverride(oIndex)"
                                title="Delete override"
                            >
                                <v-icon size="small">fas fa-trash</v-icon>
                            </v-btn>
                        </div>
                    </v-expansion-panel-title>
                    <v-expansion-panel-text>
                        <v-row dense>
                            <v-col cols="6">
                                <div class="rule-override-field-label">Name</div>
                                <v-text-field
                                    aria-label="Name"
                                    variant="outlined"
                                    density="compact"
                                    class="ghd-control-border ghd-control-text"
                                    hide-details
                                    v-model="override.name"
                                    @update:model-value="onChanged"
                                />
                            </v-col>
                            <v-col cols="6">
                                <div class="rule-override-field-label rule-override-field-label-spacer" aria-hidden="true">Enabled by default</div>
                                <v-switch
                                    label="Enabled by default"
                                    color="#2A578D"
                                    density="compact"
                                    class="rule-override-default-switch"
                                    hide-details
                                    v-model="override.isEnabledByDefault"
                                    @update:model-value="onChanged"
                                />
                            </v-col>
                        </v-row>
                        <v-row dense>
                            <v-col cols="12">
                                <div class="rule-override-field-label">Description (optional)</div>
                                <v-text-field
                                    aria-label="Description (optional)"
                                    variant="outlined"
                                    density="compact"
                                    class="ghd-control-border ghd-control-text"
                                    hide-details
                                    v-model="override.description"
                                    @update:model-value="onChanged"
                                />
                            </v-col>
                        </v-row>

                        <div style="margin-top: 8px; margin-bottom: 4px; display: flex; justify-content: space-between; align-items: center;">
                            <span class="ghd-control-label ghd-md-gray" style="font-size: 13px;">Criteria</span>
                            <v-btn
                                variant="text"
                                size="small"
                                class="ghd-blue ghd-button-text"
                                @click="addCriterion(oIndex)"
                            >
                                <v-icon size="small" class="mr-1">fas fa-plus</v-icon>
                                Add Criterion
                            </v-btn>
                        </div>

                        <v-table v-if="override.criteria.length > 0" density="compact" style="border: 1px solid #e0e0e0;">
                            <thead>
                                <tr>
                                    <th style="width: 280px;">Treatments</th>
                                    <th style="width: 95px;">Operator</th>
                                    <th>Criterion Expression</th>
                                    <th style="width: 100px;">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-for="(criterion, cIndex) in override.criteria" :key="criterion.id">
                                    <td>
                                        <v-autocomplete
                                            :items="treatmentSelectItems"
                                            item-title="text"
                                            item-value="value"
                                            multiple
                                            chips
                                            closable-chips
                                            variant="outlined"
                                            density="compact"
                                            class="ghd-control-border ghd-control-text"
                                            style="min-width: 240px; margin-top: 4px; margin-bottom: 4px;"
                                            v-model="criterion.treatmentIds"
                                            hide-details
                                            @update:model-value="onChanged"
                                        />
                                    </td>
                                    <td>
                                        <v-select
                                            :items="logicalOperatorItems"
                                            variant="outlined"
                                            density="compact"
                                            class="ghd-control-border ghd-control-text"
                                            style="min-width: 80px; margin-top: 4px; margin-bottom: 4px;"
                                            v-model="criterion.logicalOperator"
                                            hide-details
                                            @update:model-value="onChanged"
                                        />
                                    </td>
                                    <td>
                                        <div style="display: flex; align-items: center; gap: 4px; margin-top: 4px; margin-bottom: 4px;">
                                            <span class="ghd-control-text" style="flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                                {{ criterion.criterionExpression || '(no expression)' }}
                                            </span>
                                            <v-btn
                                                icon
                                                variant="text"
                                                size="small"
                                                @click="openCriterionEditor(oIndex, cIndex)"
                                                title="Edit expression"
                                            >
                                                <v-icon size="small">fas fa-edit</v-icon>
                                            </v-btn>
                                        </div>
                                    </td>
                                    <td>
                                        <v-btn
                                            icon
                                            variant="text"
                                            size="small"
                                            color="red"
                                            @click="removeCriterion(oIndex, cIndex)"
                                            title="Delete criterion"
                                        >
                                            <v-icon size="small">fas fa-trash</v-icon>
                                        </v-btn>
                                    </td>
                                </tr>
                            </tbody>
                        </v-table>
                        <div v-else class="ghd-md-gray" style="font-size: 12px; padding: 4px 0;">
                            No criteria defined for this override.
                        </div>
                    </v-expansion-panel-text>
                </v-expansion-panel>
            </v-expansion-panels>
        </v-card-text>
    </v-card>

    <GeneralCriterionEditorDialog
        :dialogData="criterionEditorDialogData"
        @submit="onCriterionEditorSubmit"
    />
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { clone } from 'ramda';
import { TreatmentRuleOverride, TreatmentRuleOverrideCriterion, emptyTreatmentRuleOverride, emptyTreatmentRuleOverrideCriterion } from '@/shared/models/iAM/treatment-rule-override';
import { SelectItem } from '@/shared/models/vue/select-item';
import GeneralCriterionEditorDialog from '@/shared/modals/GeneralCriterionEditorDialog.vue';
import { GeneralCriterionEditorDialogData, emptyGeneralCriterionEditorDialogData } from '@/shared/models/modals/general-criterion-editor-dialog-data';
import { getNewGuid } from '@/shared/utils/uuid-utils';

const props = defineProps<{
    ruleOverrides: TreatmentRuleOverride[];
    treatmentSelectItems: SelectItem[];
}>();

const emit = defineEmits(['update:ruleOverrides']);

const localOverrides = ref<TreatmentRuleOverride[]>([]);
const expandedPanel = ref<number | undefined>(undefined);
const criterionEditorDialogData = ref<GeneralCriterionEditorDialogData>(clone(emptyGeneralCriterionEditorDialogData));
const logicalOperatorItems = ['AND', 'OR'];
let editingOverrideIndex = -1;
let editingCriterionIndex = -1;

watch(() => props.ruleOverrides, (newVal) => {
    localOverrides.value = normalizeOverrides(clone(newVal));
}, { immediate: true, deep: true });

function onChanged() {
    localOverrides.value = normalizeOverrides(localOverrides.value);
    emit('update:ruleOverrides', clone(localOverrides.value));
}

function addOverride() {
    const newOverride: TreatmentRuleOverride = {
        ...clone(emptyTreatmentRuleOverride),
        id: getNewGuid(),
        isEnabled: true,
    };
    localOverrides.value.push(newOverride);
    expandedPanel.value = localOverrides.value.length - 1;
    onChanged();
}

function removeOverride(index: number) {
    localOverrides.value.splice(index, 1);
    onChanged();
}

function addCriterion(overrideIndex: number) {
    const newCriterion: TreatmentRuleOverrideCriterion = {
        ...clone(emptyTreatmentRuleOverrideCriterion),
        id: getNewGuid(),
        sortOrder: localOverrides.value[overrideIndex].criteria.length,
    };
    localOverrides.value[overrideIndex].criteria.push(newCriterion);
    onChanged();
}

function removeCriterion(overrideIndex: number, criterionIndex: number) {
    localOverrides.value[overrideIndex].criteria.splice(criterionIndex, 1);
    onChanged();
}

function openCriterionEditor(overrideIndex: number, criterionIndex: number) {
    editingOverrideIndex = overrideIndex;
    editingCriterionIndex = criterionIndex;
    criterionEditorDialogData.value = {
        showDialog: true,
        CriteriaExpression: localOverrides.value[overrideIndex].criteria[criterionIndex].criterionExpression,
    };
}

function onCriterionEditorSubmit(criterionExpression: string) {
    criterionEditorDialogData.value = clone(emptyGeneralCriterionEditorDialogData);

    if (criterionExpression !== null && editingOverrideIndex >= 0 && editingCriterionIndex >= 0) {
        localOverrides.value[editingOverrideIndex].criteria[editingCriterionIndex].criterionExpression = criterionExpression;
        onChanged();
    }

    editingOverrideIndex = -1;
    editingCriterionIndex = -1;
}

function normalizeOverrides(overrides: TreatmentRuleOverride[]): TreatmentRuleOverride[] {
    const treatmentIdByName = new Map(
        props.treatmentSelectItems.map(item => [item.text.toLowerCase(), item.value.toString()]),
    );

    return (overrides || []).map(ruleOverride => ({
        ...ruleOverride,
        isEnabled: ruleOverride.isEnabled ?? ruleOverride.isEnabledByDefault,
        criteria: (ruleOverride.criteria || []).map((criterion, index) => ({
            ...criterion,
            logicalOperator: criterion.logicalOperator === 'OR' ? 'OR' : 'AND',
            sortOrder: index,
            treatmentIds: normalizeTreatmentIds(criterion, treatmentIdByName),
            treatments: criterion.treatments || [],
        })),
    }));
}

function normalizeTreatmentIds(
    criterion: TreatmentRuleOverrideCriterion,
    treatmentIdByName: Map<string, string>,
): string[] {
    if (criterion.treatmentIds?.length) {
        return criterion.treatmentIds.map(id => id.toString());
    }

    if (criterion.treatments?.length) {
        return criterion.treatments.map(treatment => treatment.id);
    }

    if (criterion.treatmentName) {
        const treatmentId = treatmentIdByName.get(criterion.treatmentName.toLowerCase());
        return treatmentId ? [treatmentId] : [];
    }

    return [];
}
</script>

<style scoped>
.rule-override-field-label {
    color: #666666;
    font-family: 'Montserrat', sans-serif;
    font-size: 12px;
    font-weight: 500;
    line-height: 18px;
    margin-bottom: 2px;
}

.rule-override-field-label-spacer {
    visibility: hidden;
}

.rule-override-default-switch {
    margin-top: 0;
}
</style>
