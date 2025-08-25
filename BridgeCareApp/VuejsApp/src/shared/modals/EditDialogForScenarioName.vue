<template>
    <div class="text-left">
      <div
        style="cursor:pointer;"
        color="primary"
      >
        <slot></slot>
        
        <v-dialog
          v-model="dialog"
          activator="parent"
          max-width="290"
        >
          <v-card>
            <v-card-text>
              <v-col>
                <slot style="cursor:pointer;" name="input"></slot>     
                <v-text-field
                  label="Edit"
                  single-line
                  v-model="scenarioName"
                  :rules="[
                      rules[
                          'generalRules'
                      ].valueIsNotEmpty,
                      rules[
                          'generalRules'
                      ].valueContainsNoCertainSpecialCharacters,
                  ]"
                  :error-messages="scenarioNameErrors"
                  
              />     
                <v-row>
                  <v-col align="center">
                    <v-btn
                      variant="outlined"
                      class="pa-2 ma-2 ghd-blue ghd-button-text ghd-outline-button-padding ghd-button"
                      @click="onCancel"
                    >
                      Cancel
                    </v-btn>
                    <v-btn
                      variant="outlined"
                      class="pa-2 ma-2 ghd-blue ghd-button-text ghd-outline-button-padding ghd-button"
                      :disabled="hasValidationErrors"
                      @click="onSave"
                    >
                      Save
                    </v-btn>
                  </v-col>
                </v-row>
              </v-col>
            </v-card-text>
          </v-card>
        </v-dialog>
      </div>
    </div>
  </template>
<script setup lang="ts">
import { clone } from 'ramda';
import { computed, reactive, ref, watch } from 'vue';
import { InputValidationRules, rules as validationRules } from '@/shared/utils/input-validation-rules';
import { useStore } from 'vuex';
import { Scenario } from '../models/iAM/scenario';

let rules: InputValidationRules = validationRules;
const emit = defineEmits(['save', 'open', 'update:returnValue']);
let dialog = ref(false);
const scenarioNameErrors = ref<string[]>([]);
let store = useStore(); 

let stateScenarioNames = computed<string[]>(() => store.state.scenarioModule.scenarioNames)
const props = defineProps<{
  selectedScenario: Scenario
}>(); 
let scenarioName = ref("");

watch(dialog, (newVal) => {
  if (newVal === true) {
    emit('open');
  }
  scenarioName.value = props.selectedScenario.name;
});

const hasValidationErrors = computed(() => {
  // Validate that there are no special characters in the name
  scenarioNameErrors.value = [];

  const specialCharError = rules.generalRules.valueContainsNoCertainSpecialCharacters(props.returnValue);
  if (specialCharError !== true) {
    scenarioNameErrors.value.push(specialCharError as string);
  }
  if(stateScenarioNames.value.find(_ => _ == scenarioName.value) != undefined)
  {
    scenarioNameErrors.value.push("There is already a scenario with that name")
  }
  return scenarioNameErrors.value.length > 0;
});

function onSave() {
  emit('save', props.selectedScenario, scenarioName.value);
  dialog.value = false;
}

function onCancel() {
    dialog.value = false;
}
</script>
