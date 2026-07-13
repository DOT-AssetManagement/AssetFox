<template>
    <div class="rule-builder">
        <div v-if="modelValue.conditions.length > 1" class="match-mode mb-3">
            <span class="text-caption text-grey-darken-1">Match</span>
            <v-btn-toggle
                :model-value="modelValue.logicalOperator"
                mandatory
                density="compact"
                color="#2A578D"
                variant="outlined"
                @update:model-value="updateLogicalOperator"
            >
                <v-btn value="all" size="small" class="text-none">All conditions (AND)</v-btn>
                <v-btn value="any" size="small" class="text-none">Any condition (OR)</v-btn>
            </v-btn-toggle>
        </div>

        <div
            v-for="(condition, index) in modelValue.conditions"
            :key="condition.id"
            class="condition-row mb-2"
        >
            <div class="condition-prefix">
                {{ index === 0 ? 'When' : modelValue.logicalOperator === 'any' ? 'Or' : 'And' }}
            </div>
            <v-autocomplete
                :model-value="condition.attribute.attributeId"
                :items="attributeOptions"
                item-title="name"
                item-value="id"
                label="Attribute"
                placeholder="Choose an attribute"
                variant="outlined"
                density="compact"
                hide-details
                @update:model-value="value => updateAttribute(index, value)"
            />
            <v-select
                :model-value="condition.operator"
                :items="operatorsFor(condition)"
                item-title="title"
                item-value="value"
                label="Comparison"
                placeholder="Choose a comparison"
                variant="outlined"
                density="compact"
                hide-details
                :disabled="!condition.attribute.attributeId"
                @update:model-value="value => updateCondition(index, { operator: value })"
            />
            <v-text-field
                :model-value="condition.value"
                :type="isNumeric(condition) ? 'number' : 'text'"
                label="Value"
                :placeholder="isNumeric(condition) ? 'Enter a number' : 'Enter text'"
                variant="outlined"
                density="compact"
                hide-details
                :disabled="!condition.attribute.attributeId"
                @update:model-value="value => updateCondition(index, { value: normalizeValue(value) })"
            />
            <v-btn
                icon
                size="x-small"
                variant="text"
                color="error"
                :disabled="modelValue.conditions.length === 1"
                title="Remove condition"
                @click="removeCondition(index)"
            ><i class="fas fa-times" /></v-btn>
        </div>

        <div class="d-flex justify-end mb-3">
            <v-btn size="x-small" variant="text" color="#2A578D" class="text-none" @click="addCondition">
                <i class="fas fa-plus mr-2" />Add condition
            </v-btn>
        </div>

        <div class="result-row">
            <div class="condition-prefix">Then return</div>
            <v-text-field
                :model-value="modelValue.result.value"
                :type="resultType === 'number' ? 'number' : 'text'"
                :placeholder="resultType === 'number' ? 'Enter a number' : 'For example, Good'"
                variant="outlined"
                density="compact"
                hide-details
                @update:model-value="updateResult"
            />
        </div>

        <div v-if="summary" class="rule-summary mt-3">
            <i class="fas fa-check-circle mr-2" />{{ summary }}
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { clone } from 'ramda';
import { useAttributeStore } from '@/stores';
import {
    ReportCalculatedFieldResultType,
    ReportCalculationRule,
    ReportRuleComparisonOperator,
    ReportRuleCondition,
    ReportRuleLogicalOperator,
    createEmptyReportRuleCondition,
} from '@/shared/models/iAM/report-schema';

interface AttributeOption {
    id: string;
    name: string;
    type: string;
}

interface OperatorOption {
    title: string;
    value: ReportRuleComparisonOperator;
}

const props = defineProps<{
    modelValue: ReportCalculationRule;
    resultType: ReportCalculatedFieldResultType;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: ReportCalculationRule): void;
}>();

const attributeStore = useAttributeStore();
const numericOperators: OperatorOption[] = [
    { title: 'is equal to', value: 'equals' },
    { title: 'is not equal to', value: 'notEquals' },
    { title: 'is less than', value: 'lessThan' },
    { title: 'is less than or equal to', value: 'lessThanOrEqual' },
    { title: 'is greater than', value: 'greaterThan' },
    { title: 'is greater than or equal to', value: 'greaterThanOrEqual' },
];
const textOperators: OperatorOption[] = [
    { title: 'is', value: 'equals' },
    { title: 'is not', value: 'notEquals' },
    { title: 'contains', value: 'contains' },
    { title: 'does not contain', value: 'notContains' },
    { title: 'starts with', value: 'startsWith' },
    { title: 'ends with', value: 'endsWith' },
];

const attributeOptions = computed<AttributeOption[]>(() => (attributeStore.attributesgetter ?? [])
    .map((attribute: any) => ({ id: String(attribute.id), name: String(attribute.name), type: String(attribute.type) }))
    .sort((left: AttributeOption, right: AttributeOption) => left.name.localeCompare(right.name)));

function attributeFor(condition: ReportRuleCondition): AttributeOption | undefined {
    return attributeOptions.value.find(attribute => attribute.id === condition.attribute.attributeId);
}

function isNumeric(condition: ReportRuleCondition): boolean {
    return attributeFor(condition)?.type.toUpperCase() === 'NUMBER';
}

function operatorsFor(condition: ReportRuleCondition): OperatorOption[] {
    if (!condition.attribute.attributeId) {
        return [];
    }
    return isNumeric(condition) ? numericOperators : textOperators;
}

function updateRule(mutator: (rule: ReportCalculationRule) => void) {
    const next = clone(props.modelValue);
    mutator(next);
    emit('update:modelValue', next);
}

function updateLogicalOperator(value: ReportRuleLogicalOperator) {
    updateRule(rule => rule.logicalOperator = value);
}

function updateAttribute(index: number, attributeId: string | null) {
    updateRule(rule => {
        const condition = rule.conditions[index];
        const attribute = attributeOptions.value.find(item => item.id === attributeId);
        condition.attribute = attribute
            ? { attributeId: attribute.id, token: attribute.name, name: attribute.name }
            : { attributeId: '', token: '', name: '' };
        const availableOperators = attribute?.type.toUpperCase() === 'NUMBER' ? numericOperators : textOperators;
        if (!availableOperators.some(operator => operator.value === condition.operator)) {
            condition.operator = attribute ? 'equals' : '';
        }
    });
}

function updateCondition(index: number, patch: Partial<Pick<ReportRuleCondition, 'operator' | 'value'>>) {
    updateRule(rule => Object.assign(rule.conditions[index], patch));
}

function addCondition() {
    updateRule(rule => rule.conditions.push(createEmptyReportRuleCondition()));
}

function removeCondition(index: number) {
    if (props.modelValue.conditions.length === 1) {
        return;
    }
    updateRule(rule => rule.conditions.splice(index, 1));
}

function updateResult(value: unknown) {
    updateRule(rule => rule.result.value = normalizeValue(value));
}

function normalizeValue(value: unknown): string | null {
    return value === null || value === undefined || value === '' ? null : String(value);
}

const summary = computed(() => {
    const conditions = props.modelValue.conditions.map(condition => {
        const attribute = attributeFor(condition);
        const operator = operatorsFor(condition).find(item => item.value === condition.operator);
        if (!attribute || !operator || condition.value === null || condition.value === '') {
            return '';
        }
        return `${attribute.name} ${operator.title} ${condition.value}`;
    });
    if (conditions.some(condition => !condition)) {
        return '';
    }
    const join = props.modelValue.logicalOperator === 'any' ? ' or ' : ' and ';
    const result = props.modelValue.result.value ?? 'empty';
    return `When ${conditions.join(join)}, return ${result}.`;
});
</script>

<style scoped>
.rule-builder { color: #344054; }
.match-mode { display: flex; align-items: center; gap: 12px; }
.condition-row { display: grid; grid-template-columns: 70px minmax(120px, 1.2fr) minmax(145px, 1.2fr) minmax(90px, .8fr) 28px; gap: 8px; align-items: center; }
.condition-prefix { font-size: 12px; font-weight: 700; color: #475467; text-transform: uppercase; letter-spacing: .04em; }
.result-row { display: grid; grid-template-columns: 110px minmax(180px, 1fr); gap: 10px; align-items: center; padding: 12px; border-radius: 7px; background: #F1F6FB; }
.rule-summary { padding: 8px 10px; border-radius: 6px; background: #ECFDF3; color: #326B49; font-size: 12px; }
@media (max-width: 900px) {
    .condition-row { grid-template-columns: 70px 1fr 1fr; }
    .condition-row > :nth-child(4) { grid-column: 2 / 4; }
}
</style>
