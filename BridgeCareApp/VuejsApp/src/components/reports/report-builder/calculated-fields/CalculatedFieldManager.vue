<template>
    <v-dialog v-model="showDialog" max-width="1200" persistent>
        <v-card class="rounded-lg calculated-field-manager">
            <v-card-title class="d-flex align-center pa-4">
                <div>
                    <div class="text-subtitle-1 font-weight-medium">Calculated Fields</div>
                    <div class="text-caption text-grey">Report-only formulas and classifications. Simulation data is not changed.</div>
                </div>
                <v-spacer />
                <v-btn icon size="small" variant="text" @click="showDialog = false"><i class="fas fa-times" /></v-btn>
            </v-card-title>
            <v-divider />

            <div class="manager-body">
                <aside class="field-list pa-3">
                    <v-btn block size="small" color="#2A578D" variant="flat" class="text-none mb-3" @click="addField">
                        <i class="fas fa-plus mr-2" /> New calculated field
                    </v-btn>
                    <button
                        v-for="field in localFields"
                        :key="field.id"
                        type="button"
                        class="field-list-item"
                        :class="{ 'field-list-item--active': field.id === selectedId }"
                        @click="selectField(field.id)"
                    >
                        <span class="text-truncate">{{ field.name || 'Untitled field' }}</span>
                        <small>{{ field.mode === 'formula' ? 'Formula' : 'Classification' }} · {{ field.resultType }}</small>
                    </button>
                    <div v-if="localFields.length === 0" class="text-caption text-grey text-center pa-4">
                        No calculated fields yet.
                    </div>
                </aside>

                <main class="field-editor pa-4">
                    <template v-if="draft">
                        <div class="d-flex ga-3">
                            <v-text-field v-model="draft.name" label="Field name" variant="outlined" density="compact" class="flex-grow-1" />
                            <v-select v-model="draft.mode" :items="modeOptions" label="Mode" variant="outlined" density="compact" style="max-width: 170px;" @update:model-value="onModeChange" />
                            <v-select v-model="draft.resultType" :items="resultTypeOptions" label="Output type" variant="outlined" density="compact" style="max-width: 150px;" :disabled="draft.mode === 'formula'" />
                        </div>
                        <v-text-field v-model="draft.description" label="Description (optional)" variant="outlined" density="compact" />

                        <template v-if="draft.mode === 'formula'">
                            <div class="editor-section-title">Formula</div>
                            <ReportExpressionEditor
                                v-model="draft.formula!.expression"
                                label="Numeric expression"
                                :rows="4"
                                show-functions
                            />
                        </template>

                        <template v-else>
                            <div class="d-flex align-center mb-2">
                                <div class="editor-section-title mb-0">Classification rules</div>
                                <v-spacer />
                                <v-btn size="small" variant="outlined" class="text-none" @click="addRule"><i class="fas fa-plus mr-2" />Add rule</v-btn>
                            </div>
                            <v-alert type="info" variant="tonal" density="compact" class="mb-3">
                                Rules are checked from top to bottom. The first matching rule determines the result.
                            </v-alert>
                            <div v-for="(rule, index) in draft.rules" :key="rule.id" class="rule-card mb-3 pa-3">
                                <div class="d-flex align-center mb-2">
                                    <strong class="text-caption">Rule {{ index + 1 }}</strong>
                                    <v-spacer />
                                    <v-btn icon size="x-small" variant="text" :disabled="index === 0" @click="moveRule(index, -1)"><i class="fas fa-arrow-up" /></v-btn>
                                    <v-btn icon size="x-small" variant="text" :disabled="index === draft.rules.length - 1" @click="moveRule(index, 1)"><i class="fas fa-arrow-down" /></v-btn>
                                    <v-btn icon size="x-small" variant="text" color="error" @click="removeRule(index)"><i class="fas fa-trash" /></v-btn>
                                </div>
                                <ReportRuleBuilder
                                    :model-value="rule"
                                    :result-type="draft.resultType"
                                    @update:model-value="value => updateRule(index, value)"
                                />
                            </div>
                            <v-alert v-if="draft.rules.length === 0" type="info" variant="tonal" density="compact" class="mb-3">Add at least one rule.</v-alert>

                            <div class="otherwise-row">
                                <strong>Otherwise return</strong>
                                <v-text-field
                                    :model-value="draft.defaultResult?.value"
                                    :type="draft.resultType === 'number' ? 'number' : 'text'"
                                    :placeholder="draft.resultType === 'number' ? 'Enter a number' : 'For example, Poor'"
                                    variant="outlined"
                                    density="compact"
                                    hide-details
                                    @update:model-value="value => draft!.defaultResult = { value: normalizeResult(value) }"
                                />
                            </div>

                            <v-expansion-panels variant="accordion" class="mt-3">
                                <v-expansion-panel>
                                    <v-expansion-panel-title class="text-caption">Advanced: missing data</v-expansion-panel-title>
                                    <v-expansion-panel-text>
                                        <p class="text-caption text-grey mb-2">
                                            Choose what to return when any attribute used by these rules has no value. Leave blank to return an empty cell.
                                        </p>
                                    <v-text-field
                                        :model-value="draft.missingResult?.value"
                                        :type="draft.resultType === 'number' ? 'number' : 'text'"
                                        label="Return when data is missing"
                                        variant="outlined"
                                        density="compact"
                                        clearable
                                        hide-details
                                        @update:model-value="value => draft!.missingResult = { value: normalizeResult(value) }"
                                    />
                                    </v-expansion-panel-text>
                                </v-expansion-panel>
                            </v-expansion-panels>
                        </template>

                        <v-alert v-if="validationMessage" type="error" variant="tonal" density="compact" class="mt-3">{{ validationMessage }}</v-alert>
                        <div class="d-flex align-center mt-4">
                            <v-btn
                                size="small"
                                variant="outlined"
                                color="error"
                                class="text-none"
                                :disabled="usedIds.has(draft.id)"
                                :title="usedIds.has(draft.id) ? 'Remove this field from report components before deleting it.' : 'Delete calculated field'"
                                @click="deleteField"
                            ><i class="fas fa-trash mr-2" />Delete</v-btn>
                            <v-spacer />
                            <v-btn size="small" variant="text" class="text-none" @click="resetDraft">Discard changes</v-btn>
                            <v-btn size="small" color="#2A578D" variant="flat" class="text-none ml-2" :loading="validating" @click="saveDraft">Save field</v-btn>
                        </div>
                    </template>
                    <div v-else class="empty-editor">
                        <i class="fas fa-calculator" />
                        <span>Select a field or create a new one.</span>
                    </div>
                </main>
            </div>
        </v-card>
    </v-dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { clone } from 'ramda';
import { useAttributeStore } from '@/stores';
import { getNewGuid } from '@/shared/utils/uuid-utils';
import ReportSchemaService from '@/services/report-schema.service';
import {
    ReportAttributeReference,
    ReportCalculatedField,
    ReportCalculationRule,
    ReportExpressionDefinition,
    createEmptyReportCalculatedField,
    createEmptyReportRuleCondition,
} from '@/shared/models/iAM/report-schema';
import ReportExpressionEditor from './ReportExpressionEditor.vue';
import ReportRuleBuilder from './ReportRuleBuilder.vue';

const props = defineProps<{
    modelValue: boolean;
    fields: ReportCalculatedField[];
    usedFieldIds: string[];
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
    (e: 'update:fields', value: ReportCalculatedField[]): void;
}>();

const attributeStore = useAttributeStore();
const localFields = ref<ReportCalculatedField[]>(clone(props.fields));
const selectedId = ref('');
const draft = ref<ReportCalculatedField | null>(null);
const validationMessage = ref('');
const validating = ref(false);
const usedIds = computed(() => new Set(props.usedFieldIds));
const modeOptions = [{ title: 'Classification rules', value: 'rules' }, { title: 'Numeric formula', value: 'formula' }];
const resultTypeOptions = [{ title: 'Text', value: 'text' }, { title: 'Number', value: 'number' }];

const showDialog = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value),
});

watch(() => props.fields, value => {
    localFields.value = clone(value);
    if (selectedId.value) {
        resetDraft();
    }
}, { deep: true });

function addField() {
    draft.value = createEmptyReportCalculatedField();
    selectedId.value = draft.value.id;
    validationMessage.value = '';
}

function selectField(id: string) {
    selectedId.value = id;
    resetDraft();
}

function resetDraft() {
    const field = localFields.value.find(item => item.id === selectedId.value);
    draft.value = field ? clone(field) : null;
    validationMessage.value = '';
}

function onModeChange() {
    if (!draft.value) {
        return;
    }
    if (draft.value.mode === 'formula') {
        draft.value.resultType = 'number';
        draft.value.formula ||= { expression: '', attributeReferences: [] };
    } else if (draft.value.rules.length === 0) {
        addRule();
    }
}

function addRule() {
    if (!draft.value) {
        return;
    }
    draft.value.rules.push({
        id: getNewGuid(),
        logicalOperator: 'all',
        conditions: [createEmptyReportRuleCondition()],
        result: { value: null },
    });
}

function updateRule(index: number, rule: ReportCalculationRule) {
    if (!draft.value) {
        return;
    }
    draft.value.rules.splice(index, 1, rule);
}

function removeRule(index: number) {
    draft.value?.rules.splice(index, 1);
}

function moveRule(index: number, offset: number) {
    if (!draft.value) {
        return;
    }
    const target = index + offset;
    if (target < 0 || target >= draft.value.rules.length) {
        return;
    }
    const [rule] = draft.value.rules.splice(index, 1);
    draft.value.rules.splice(target, 0, rule);
}

function normalizeResult(value: unknown): string | null {
    return value === null || value === undefined || value === '' ? null : String(value);
}

function validateField(field: ReportCalculatedField): string {
    if (!field.name.trim()) {
        return 'Field name is required.';
    }
    if (localFields.value.some(item => item.id !== field.id && item.name.trim().toLowerCase() === field.name.trim().toLowerCase())) {
        return 'Calculated field names must be unique.';
    }
    if (field.mode === 'formula' && !field.formula?.expression.trim()) {
        return 'Formula is required.';
    }
    if (field.mode === 'rules' && field.rules.length === 0) {
        return 'At least one rule is required.';
    }
    if (field.mode === 'rules' && field.rules.some(rule => !rule.conditions.length)) {
        return 'Every rule needs at least one condition.';
    }
    if (field.mode === 'rules' && field.rules.some(rule => rule.conditions.some(condition => !condition.attribute.attributeId))) {
        return 'Choose an attribute for every rule condition.';
    }
    if (field.mode === 'rules' && field.rules.some(rule => rule.conditions.some(condition => !condition.operator))) {
        return 'Choose a comparison for every rule condition.';
    }
    if (field.mode === 'rules' && field.rules.some(rule => rule.conditions.some(condition => condition.value === null || condition.value === ''))) {
        return 'Enter a comparison value for every rule condition.';
    }
    if (field.resultType === 'number') {
        const values = [field.defaultResult?.value, field.missingResult?.value, ...field.rules.map(rule => rule.result.value)]
            .filter(value => value !== null && value !== undefined && value !== '');
        if (values.some(value => !Number.isFinite(Number(value)))) {
            return 'Every numeric result must be a valid number.';
        }
    }
    return '';
}

async function saveDraft() {
    if (!draft.value) {
        return;
    }
    validationMessage.value = validateField(draft.value);
    if (validationMessage.value) {
        return;
    }

    const saved = clone(draft.value);
    saved.name = saved.name.trim();
    if (saved.mode === 'formula' && saved.formula) {
        syncReferences(saved.formula);
    }

    const candidateFields = localFields.value.filter(item => item.id !== saved.id);
    candidateFields.push(saved);
    validating.value = true;
    try {
        await ReportSchemaService.validateCalculatedFields(candidateFields);
    } catch (error: any) {
        validationMessage.value = error?.response?.data?.error || 'The formula or rule expression is invalid.';
        validating.value = false;
        return;
    }
    validating.value = false;

    const index = localFields.value.findIndex(item => item.id === saved.id);
    if (index >= 0) {
        localFields.value.splice(index, 1, saved);
    } else {
        localFields.value.push(saved);
    }
    localFields.value = [...localFields.value];
    draft.value = clone(saved);
    emit('update:fields', clone(localFields.value));
}

function syncReferences(expression: ReportExpressionDefinition) {
    const attributes = attributeStore.attributesgetter ?? [];
    const byName = new Map(attributes.map((attribute: any) => [String(attribute.name).toLowerCase(), attribute]));
    const byId = new Map(attributes.map((attribute: any) => [String(attribute.id).toLowerCase(), attribute]));
    const existingByToken = new Map((expression.attributeReferences || [])
        .map(reference => [reference.token.toLowerCase(), reference]));
    const names = Array.from(expression.expression.matchAll(/\[([^\]]+)\]/g)).map(match => match[1].trim());
    const seen = new Set<string>();
    expression.attributeReferences = names.reduce<ReportAttributeReference[]>((references, name) => {
        const existing = existingByToken.get(name.toLowerCase());
        const attribute: any = byName.get(name.toLowerCase())
            || (existing ? byId.get(existing.attributeId.toLowerCase()) : undefined);
        if ((!attribute && !existing) || seen.has(name.toLowerCase())) {
            return references;
        }
        seen.add(name.toLowerCase());
        references.push({
            attributeId: attribute?.id || existing!.attributeId,
            token: name,
            name: attribute?.name || existing!.name,
        });
        return references;
    }, []);
}

function deleteField() {
    if (!draft.value || usedIds.value.has(draft.value.id)) {
        return;
    }
    localFields.value = localFields.value.filter(field => field.id !== draft.value!.id);
    selectedId.value = '';
    draft.value = null;
    emit('update:fields', clone(localFields.value));
}
</script>

<style scoped>
.calculated-field-manager { min-height: 660px; }
.manager-body { display: grid; grid-template-columns: 220px minmax(0, 1fr); min-height: 590px; }
.field-list { border-right: 1px solid #E5E7EB; background: #F8FAFC; }
.field-list-item { display: flex; flex-direction: column; width: 100%; padding: 9px 10px; margin-bottom: 4px; border-radius: 6px; text-align: left; color: #344054; }
.field-list-item:hover, .field-list-item--active { background: #E8F1FA; color: #214A78; }
.field-list-item small { color: #87909C; text-transform: capitalize; }
.field-editor { overflow-y: auto; max-height: 72vh; }
.editor-section-title { font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: .06em; color: #667085; margin: 8px 0; }
.rule-card { border: 1px solid #DDE3EA; border-radius: 8px; background: #FBFCFD; }
.otherwise-row { display: grid; grid-template-columns: 150px minmax(180px, 1fr); gap: 12px; align-items: center; padding: 14px; border: 1px solid #B9CEE4; border-radius: 8px; background: #F1F6FB; color: #344054; }
.empty-editor { height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 12px; color: #A0A7B1; }
.empty-editor i { font-size: 28px; }
</style>
