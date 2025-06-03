<template>
    <v-row>
      <v-dialog max-width="450px" persistent v-model="showDialogComputed">
        <v-card class="ghd-padding">
          <v-card-title>
            <v-row justify="space-between">
              <h3 class="ghd-title">Clone Treatment</h3>
              <XButton @click="onSubmit(false)"/> 
            </v-row>                    
          </v-card-title>
          <v-card-text>
            <v-row column>
              <v-subheader class="ghd-control-label ghd-md-gray">Name</v-subheader>
              <v-text-field variant="outlined" id="CreateTreatmentDialog-name-textField" class="ghd-control-border ghd-control-text ghd-control-width-lg" outline v-model="treatmentName"></v-text-field>
            </v-row>
          </v-card-text>
          <v-card-actions>
            <v-row justify="center">
              <CancelButton @cancel="onSubmit(false)"/>
              <SaveButton 
                @save="onSubmit(true)"
                :disabled="treatmentName === ''" 
              />           
            </v-row>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-row>
  </template>
  
  <script lang="ts" setup>
  import Vue, { computed } from 'vue';
  import { inject, reactive, ref, onMounted, onBeforeUnmount, watch, Ref} from 'vue';
  import { useStore } from 'vuex';
  import {emptyTreatment, Treatment} from '@/shared/models/iAM/treatment';
  import {getNewGuid} from '@/shared/utils/uuid-utils';
  import XButton from '@/shared/components/buttons/XButton.vue';
  import CancelButton from '@/shared/components/buttons/CancelButton.vue';
  import SaveButton from '@/shared/components/buttons/SaveButton.vue';
import { clone } from 'ramda';
  
  const props = defineProps<{showDialog:boolean}>()
  let showDialogComputed = computed(() => props.showDialog);
  let treatmentName = ref<string>('')
  const emit = defineEmits(['submit'])
  
    function onSubmit(submit: boolean) {
      if (submit) {        
        emit('submit', treatmentName.value);
      } else {
        emit('submit', null);
      }
      treatmentName.value = '';
    }
  
  </script>
  