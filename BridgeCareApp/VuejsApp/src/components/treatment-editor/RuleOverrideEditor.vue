<template>
    <v-card class="rule-override-card elevation-0 mb-4">
        <div class="rule-override-header">
            <div>
                <div class="rule-override-title">Rule Overrides</div>
                <div class="rule-override-summary">{{ overrideSummary }}</div>
            </div>
            <v-btn
                variant="outlined"
                class="rule-override-primary-button ghd-blue ghd-button-text"
                size="small"
                @click="addOverride"
            >
                <v-icon size="small" class="mr-1">fas fa-plus</v-icon>
                Add Override
            </v-btn>
        </div>

        <v-card-text class="rule-override-body">
            <div v-if="localOverrides.length === 0" class="rule-override-empty">
                <div class="rule-override-empty-title">No rule overrides yet</div>
                <div class="rule-override-empty-copy">Scenario toggles will appear here after an override is added.</div>
            </div>

            <v-expansion-panels
                v-else
                v-model="expandedPanel"
                variant="accordion"
                class="rule-override-panels"
            >
                <v-expansion-panel
                    v-for="(override, oIndex) in localOverrides"
                    :key="override.id || oIndex"
                    class="rule-override-panel"
                    elevation="0"
                >
                    <v-expansion-panel-title class="rule-override-panel-title">
                        <div class="rule-override-panel-heading">
                            <div class="rule-override-panel-name">{{ getOverrideName(override) }}</div>
                            <div class="rule-override-panel-meta">
                                <span class="rule-override-chip">{{ getCriteriaCountLabel(override.criteria.length) }}</span>
                                <span
                                    class="rule-override-chip"
                                    :class="{ 'rule-override-chip--muted': !override.isEnabledByDefault }"
                                >
                                    {{ override.isEnabledByDefault ? 'Default on' : 'Default off' }}
                                </span>
                            </div>
                        </div>
                        <v-btn
                            icon
                            variant="text"
                            size="small"
                            class="rule-override-icon-button rule-override-icon-button--danger"
                            @click.stop="removeOverride(oIndex)"
                            title="Delete override"
                        >
                            <v-icon size="small">fas fa-trash</v-icon>
                        </v-btn>
                    </v-expansion-panel-title>
                    <v-expansion-panel-text class="rule-override-panel-text">
                        <v-row dense class="rule-override-form-row">
                            <v-col cols="12" md="7">
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
                            <v-col cols="12" md="5">
                                <div class="rule-override-default-control">
                                    <div class="rule-override-field-label">Enabled by default</div>
                                    <v-switch
                                        aria-label="Enabled by default"
                                        color="#2A578D"
                                        density="compact"
                                        inset
                                        hide-details
                                        v-model="override.isEnabledByDefault"
                                        @update:model-value="onChanged"
                                    />
                                </div>
                            </v-col>
                        </v-row>
                        <v-row dense>
                            <v-col cols="12">
                                <div class="rule-override-field-label">Description</div>
                                <v-text-field
                                    aria-label="Description"
                                    variant="outlined"
                                    density="compact"
                                    class="ghd-control-border ghd-control-text"
                                    hide-details
                                    v-model="override.description"
                                    @update:model-value="onChanged"
                                />
                            </v-col>
                        </v-row>

                        <div class="rule-override-section-heading">
                            <span>Criteria</span>
                            <v-btn
                                variant="text"
                                size="small"
                                class="rule-override-text-button ghd-blue ghd-button-text"
                                @click="addCriterion(oIndex)"
                            >
                                <v-icon size="small" class="mr-1">fas fa-plus</v-icon>
                                Add Criterion
                            </v-btn>
                        </div>

                        <div v-if="override.criteria.length > 0" class="rule-override-table-wrap">
                            <v-table density="compact" class="rule-override-criteria-table">
                                <colgroup>
                                    <col class="rule-override-treatment-column">
                                    <col class="rule-override-operator-column">
                                    <col>
                                    <col class="rule-override-actions-column">
                                </colgroup>
                                <thead>
                                    <tr>
                                        <th>Treatments</th>
                                        <th>Operator</th>
                                        <th>Expression</th>
                                        <th class="rule-override-actions-heading">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="(criterion, cIndex) in override.criteria" :key="criterion.id || cIndex">
                                        <td class="rule-override-treatment-cell">
                                            <v-autocomplete
                                                :items="treatmentSelectItems"
                                                item-title="text"
                                                item-value="value"
                                                multiple
                                                chips
                                                closable-chips
                                                variant="outlined"
                                                density="compact"
                                                class="ghd-control-border ghd-control-text rule-override-treatment-select"
                                                v-model="criterion.treatmentIds"
                                                hide-details
                                                @update:model-value="onChanged"
                                            />
                                        </td>
                                        <td class="rule-override-operator-cell">
                                            <v-select
                                                :items="logicalOperatorItems"
                                                variant="outlined"
                                                density="compact"
                                                class="ghd-control-border ghd-control-text rule-override-operator-select"
                                                v-model="criterion.logicalOperator"
                                                hide-details
                                                @update:model-value="onChanged"
                                            />
                                        </td>
                                        <td>
                                            <div class="rule-override-expression">
                                                <span
                                                    class="rule-override-expression-text"
                                                    :class="{ 'rule-override-expression-text--empty': !criterion.criterionExpression }"
                                                >
                                                    {{ criterion.criterionExpression || 'No expression' }}
                                                </span>
                                                <v-btn
                                                    icon
                                                    variant="text"
                                                    size="small"
                                                    class="rule-override-icon-button"
                                                    @click="openCriterionEditor(oIndex, cIndex)"
                                                    title="Edit expression"
                                                >
                                                    <v-icon size="small">fas fa-edit</v-icon>
                                                </v-btn>
                                            </div>
                                        </td>
                                        <td class="rule-override-actions-cell">
                                            <v-btn
                                                icon
                                                variant="text"
                                                size="small"
                                                class="rule-override-icon-button rule-override-icon-button--danger"
                                                @click="removeCriterion(oIndex, cIndex)"
                                                title="Delete criterion"
                                            >
                                                <v-icon size="small">fas fa-trash</v-icon>
                                            </v-btn>
                                        </td>
                                    </tr>
                                </tbody>
                            </v-table>
                        </div>
                        <div v-else class="rule-override-empty rule-override-empty--compact">
                            <div class="rule-override-empty-title">No criteria for this override</div>
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
import { computed, ref, watch } from 'vue';
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

const overrideSummary = computed(() => getOverrideCountLabel(localOverrides.value.length));

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

function getOverrideName(ruleOverride: TreatmentRuleOverride): string {
    return ruleOverride.name?.trim() || 'Untitled override';
}

function getOverrideCountLabel(count: number): string {
    return `${count} ${count === 1 ? 'override' : 'overrides'}`;
}

function getCriteriaCountLabel(count: number): string {
    return `${count} ${count === 1 ? 'criterion' : 'criteria'}`;
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
.rule-override-card {
    border: 1px solid #d8dee7;
    border-radius: 8px;
    overflow: hidden;
}

.rule-override-header {
    align-items: center;
    background: #f7f9fc;
    border-bottom: 1px solid #e8edf3;
    display: flex;
    justify-content: space-between;
    padding: 10px 12px;
}

.rule-override-title,
.rule-override-panel-name,
.rule-override-empty-title {
    color: #333333;
    font-family: 'Montserrat', sans-serif;
    font-size: 14px;
    font-weight: 600;
    line-height: 20px;
}

.rule-override-summary,
.rule-override-empty-copy {
    color: #666666;
    font-family: 'Montserrat', sans-serif;
    font-size: 12px;
    line-height: 18px;
}

.rule-override-primary-button {
    border-color: #2A578D;
    border-radius: 6px;
    min-height: 32px;
    padding: 0 10px !important;
}

.rule-override-body {
    padding: 10px 12px 12px;
}

.rule-override-empty {
    background: #fbfcfe;
    border: 1px dashed #cfd8e3;
    border-radius: 8px;
    padding: 14px 16px;
}

.rule-override-empty--compact {
    margin-top: 4px;
    padding: 10px 12px;
}

.rule-override-panels {
    display: flex;
    flex-direction: column;
    gap: 8px;
    width: 100%;
}

.rule-override-panel {
    border: 1px solid #dfe5ec;
    border-radius: 8px !important;
    flex: 0 0 auto;
    max-width: none;
    overflow: hidden;
    width: 100%;
}

.rule-override-panel-title {
    min-height: 50px;
    padding: 8px 10px 8px 12px;
}

.rule-override-panel-heading {
    align-items: center;
    display: flex;
    gap: 12px;
    justify-content: space-between;
    min-width: 0;
    width: 100%;
}

.rule-override-panel-name {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.rule-override-panel-meta {
    display: flex;
    flex-shrink: 0;
    gap: 6px;
}

.rule-override-chip {
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

.rule-override-chip--muted {
    background: #f4f5f7;
    border-color: #d8dde3;
    color: #666666;
}

.rule-override-panel-text :deep(.v-expansion-panel-text__wrapper) {
    background: #ffffff;
    border-top: 1px solid #edf1f5;
    padding: 10px 12px 12px;
}

.rule-override-form-row {
    align-items: end;
}

.rule-override-field-label {
    color: #666666;
    font-family: 'Montserrat', sans-serif;
    font-size: 12px;
    font-weight: 500;
    line-height: 18px;
    margin-bottom: 4px;
}

.rule-override-default-control {
    align-items: center;
    border: 1px solid #e1e7ef;
    border-radius: 8px;
    display: inline-flex;
    gap: 10px;
    justify-content: flex-start;
    min-height: 40px;
    padding: 0 10px;
    width: fit-content;
}

.rule-override-default-control :deep(.v-input) {
    flex: 0 0 auto;
}

.rule-override-section-heading {
    align-items: center;
    color: #666666;
    display: flex;
    font-family: 'Montserrat', sans-serif;
    font-size: 13px;
    font-weight: 600;
    justify-content: space-between;
    margin: 8px 0 4px;
}

.rule-override-text-button {
    border-radius: 6px;
    min-height: 28px;
    padding: 0 8px !important;
}

.rule-override-table-wrap {
    border: 1px solid #dfe5ec;
    border-radius: 8px;
    overflow: hidden;
}

.rule-override-criteria-table {
    background: #ffffff;
}

.rule-override-criteria-table :deep(th) {
    background: #f7f9fc;
    color: #666666;
    font-family: 'Montserrat', sans-serif;
    font-size: 12px;
    font-weight: 600;
    height: 34px;
}

.rule-override-criteria-table :deep(td) {
    border-bottom: 1px solid #edf1f5;
    padding: 6px;
    vertical-align: middle;
}

.rule-override-criteria-table :deep(tbody tr:last-child td) {
    border-bottom: 0;
}

.rule-override-treatment-column,
.rule-override-treatment-cell {
    width: 36%;
}

.rule-override-operator-column,
.rule-override-operator-cell {
    width: 140px;
}

.rule-override-actions-column,
.rule-override-actions-heading,
.rule-override-actions-cell {
    text-align: center;
    width: 88px;
}

.rule-override-treatment-select {
    min-width: 240px;
}

.rule-override-operator-select {
    min-width: 118px;
}

.rule-override-expression {
    align-items: center;
    background: #fbfcfe;
    border: 1px solid #e2e8f0;
    border-radius: 6px;
    display: flex;
    gap: 6px;
    min-height: 36px;
    padding: 2px 4px 2px 10px;
}

.rule-override-expression-text {
    color: #333333;
    flex: 1;
    font-family: 'Montserrat', sans-serif;
    font-size: 13px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.rule-override-expression-text--empty {
    color: #888888;
    font-style: italic;
}

.rule-override-icon-button {
    color: #2A578D;
    flex: 0 0 auto;
}

.rule-override-icon-button--danger {
    color: #b64343;
}

@media (max-width: 960px) {
    .rule-override-header,
    .rule-override-panel-heading {
        align-items: flex-start;
        flex-direction: column;
    }

    .rule-override-primary-button {
        align-self: flex-start;
    }

    .rule-override-table-wrap {
        overflow-x: auto;
    }
}
</style>
