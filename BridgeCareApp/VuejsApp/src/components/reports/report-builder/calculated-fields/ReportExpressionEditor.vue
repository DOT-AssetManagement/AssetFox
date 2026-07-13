<template>
    <div class="formula-composer">
        <div class="formula-intro mb-3">
            <strong>Build a numeric formula</strong>
            <span>Insert numeric attributes and combine them using the operators or functions below.</span>
        </div>

        <div class="toolbox-section">
            <div class="toolbox-label">1. Insert a value</div>
            <div class="value-tools">
                <v-autocomplete
                    v-model="selectedAttribute"
                    :items="numericAttributeNames"
                    label="Numeric attribute"
                    placeholder="Choose an attribute"
                    variant="outlined"
                    density="compact"
                    hide-details
                    clearable
                />
                <v-btn
                    size="small"
                    color="#2A578D"
                    variant="outlined"
                    class="text-none"
                    :disabled="!selectedAttribute"
                    @click="insertAttribute"
                ><i class="fas fa-plus mr-2" />Insert attribute</v-btn>

                <v-text-field
                    v-model="constantValue"
                    type="number"
                    label="Number"
                    placeholder="For example, 100"
                    variant="outlined"
                    density="compact"
                    hide-details
                />
                <v-btn
                    size="small"
                    variant="outlined"
                    class="text-none"
                    :disabled="constantValue === null || constantValue === ''"
                    @click="insertConstant"
                ><i class="fas fa-plus mr-2" />Insert number</v-btn>
            </div>
        </div>

        <div class="toolbox-section">
            <div class="toolbox-label">2. Add an operator</div>
            <div class="operator-tools">
                <v-btn
                    v-for="operator in operators"
                    :key="operator.value"
                    size="small"
                    variant="outlined"
                    :title="operator.description"
                    @click="insert(operator.value)"
                >{{ operator.label }}</v-btn>
            </div>
            <div class="text-caption text-grey mt-1">Normal math precedence applies. Use parentheses to control the order.</div>
        </div>

        <div v-if="showFunctions" class="toolbox-section">
            <div class="toolbox-label">3. Optional functions</div>
            <div class="function-grid">
                <button
                    v-for="fn in functions"
                    :key="fn.name"
                    type="button"
                    class="function-option"
                    @click="insertFunction(fn)"
                >
                    <strong>{{ fn.signature }}</strong>
                    <span>{{ fn.description }}</span>
                </button>
            </div>
        </div>

        <div class="toolbox-label mt-3">{{ showFunctions ? '4' : '3' }}. Review the formula</div>
        <v-textarea
            ref="textareaRef"
            :model-value="modelValue"
            :label="label"
            placeholder="Example: Round([IRI] * 1.10, 1)"
            :rows="rows"
            auto-grow
            variant="outlined"
            density="compact"
            spellcheck="false"
            class="mb-2 expression-input"
            @update:model-value="value => emit('update:modelValue', value || '')"
        />

        <div class="formula-examples">
            <strong>Examples</strong>
            <span><code>[IRI] * 1.10</code> — increase IRI by 10%</span>
            <span><code>([DECK] + [SUPER]) / 2</code> — average two values</span>
            <span><code>Round([IRI], 1)</code> — round to one decimal place</span>
        </div>
        <v-alert type="info" variant="tonal" density="compact" class="mt-3">
            Division by zero returns an empty value. Formula fields always produce numbers.
        </v-alert>
    </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref } from 'vue';
import { useAttributeStore } from '@/stores';

interface FormulaFunction {
    name: string;
    signature: string;
    template: string;
    cursorBack: number;
    description: string;
}

const props = withDefaults(defineProps<{
    modelValue: string;
    label: string;
    rows?: number;
    showFunctions?: boolean;
}>(), {
    rows: 3,
    showFunctions: true,
});

const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
}>();

const attributeStore = useAttributeStore();
const textareaRef = ref<any>(null);
const selectedAttribute = ref<string | null>(null);
const constantValue = ref<string | null>(null);
const operators = [
    { label: '+', value: ' + ', description: 'Add' },
    { label: '−', value: ' - ', description: 'Subtract' },
    { label: '×', value: ' * ', description: 'Multiply' },
    { label: '÷', value: ' / ', description: 'Divide' },
    { label: '(', value: '(', description: 'Open parentheses' },
    { label: ')', value: ')', description: 'Close parentheses' },
];
const functions: FormulaFunction[] = [
    { name: 'Abs', signature: 'Abs(value)', template: 'Abs()', cursorBack: 1, description: 'Absolute value' },
    { name: 'Ceiling', signature: 'Ceiling(value)', template: 'Ceiling()', cursorBack: 1, description: 'Round upward to a whole number' },
    { name: 'Floor', signature: 'Floor(value)', template: 'Floor()', cursorBack: 1, description: 'Round downward to a whole number' },
    { name: 'Sqrt', signature: 'Sqrt(value)', template: 'Sqrt()', cursorBack: 1, description: 'Square root' },
    { name: 'Pow', signature: 'Pow(value, power)', template: 'Pow(, )', cursorBack: 3, description: 'Raise a value to a power' },
    { name: 'Round', signature: 'Round(value, decimals)', template: 'Round(, )', cursorBack: 3, description: 'Round to a number of decimal places' },
    { name: 'Truncate', signature: 'Truncate(value)', template: 'Truncate()', cursorBack: 1, description: 'Remove the decimal portion' },
];

const numericAttributeNames = computed(() => (attributeStore.attributesgetter ?? [])
    .filter((attribute: any) => String(attribute.type).toUpperCase() === 'NUMBER')
    .map((attribute: any) => String(attribute.name))
    .sort((left: string, right: string) => left.localeCompare(right)));

function textareaElement(): HTMLTextAreaElement | null {
    return textareaRef.value?.$el?.querySelector('textarea') ?? null;
}

function insert(value: string, cursorBack = 0) {
    const textarea = textareaElement();
    const start = textarea?.selectionStart ?? props.modelValue.length;
    const end = textarea?.selectionEnd ?? start;
    const nextValue = `${props.modelValue.slice(0, start)}${value}${props.modelValue.slice(end)}`;
    const cursor = start + value.length - cursorBack;
    emit('update:modelValue', nextValue);
    nextTick(() => {
        const current = textareaElement();
        current?.focus();
        current?.setSelectionRange(cursor, cursor);
    });
}

function insertAttribute() {
    if (!selectedAttribute.value) {
        return;
    }
    insert(`[${selectedAttribute.value}]`);
    selectedAttribute.value = null;
}

function insertConstant() {
    if (constantValue.value === null || constantValue.value === '') {
        return;
    }
    insert(String(constantValue.value));
    constantValue.value = null;
}

function insertFunction(fn: FormulaFunction) {
    insert(fn.template, fn.cursorBack);
}
</script>

<style scoped>
.formula-composer { color: #344054; }
.formula-intro { display: flex; flex-direction: column; gap: 2px; padding: 12px 14px; border-radius: 8px; background: #F1F6FB; }
.formula-intro span { color: #667085; font-size: 12px; }
.toolbox-section { margin-top: 14px; }
.toolbox-label { margin-bottom: 7px; color: #475467; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: .04em; }
.value-tools { display: grid; grid-template-columns: minmax(220px, 1fr) auto; gap: 8px; align-items: center; }
.operator-tools { display: flex; flex-wrap: wrap; gap: 6px; }
.operator-tools :deep(.v-btn) { min-width: 42px; font-size: 17px; }
.function-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 7px; }
.function-option { display: flex; flex-direction: column; gap: 2px; padding: 9px 11px; border: 1px solid #D5DEE8; border-radius: 7px; background: white; text-align: left; color: #344054; }
.function-option:hover { border-color: #7CA4CC; background: #F5F9FD; }
.function-option strong { color: #214A78; font-family: Consolas, 'Courier New', monospace; font-size: 12px; }
.function-option span { color: #667085; font-size: 11px; }
.expression-input :deep(textarea) { font-family: Consolas, 'Courier New', monospace; font-size: 13px; }
.formula-examples { display: flex; flex-direction: column; gap: 4px; padding: 10px 12px; border-left: 3px solid #9BB9D5; background: #F8FAFC; color: #667085; font-size: 11px; }
.formula-examples code { color: #214A78; }
@media (max-width: 900px) {
    .value-tools { grid-template-columns: 1fr auto; }
    .function-grid { grid-template-columns: 1fr; }
}
</style>
