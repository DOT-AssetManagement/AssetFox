
<template>
    <v-row class="Montserrat-font-family">
        <v-col cols="12">
            <v-col cols="6" class="ghd-constant-header">
                <v-row style="margin-bottom: 20px;">
                    <v-row column>
                        <v-col>
                            <v-subheader  class="ghd-md-gray ghd-control-label">Upper Bound Date</v-subheader>
                            <v-text-field
                                variant="outlined"
                                v-model="upperBoundDate"
                                type="date"
                                :rules="[ rules['generalRules'].valueIsNotEmpty]"
                                clearable
                                single-line           
                                style="padding-left: 0px; "
                                density="compact">
                            </v-text-field>
                        </v-col>
                    </v-row>
                </v-row>
                <v-row style="margin-bottom: 20px;">
                    <v-row >
                        <v-col>
                            <v-subheader class="ghd-md-gray ghd-control-label">Lower Bound Date</v-subheader>
                            <v-text-field
                                variant="outlined"
                                v-model="lowerBoundDate"
                                type="date"
                                :rules="[ rules['generalRules'].valueIsNotEmpty]"
                                clearable
                                single-line           
                                style="padding-left: 0px;"
                                density="compact">
                            </v-text-field>                         
                        </v-col>                                                
                    </v-row>
                </v-row>          
            </v-col>                   
        </v-col>  
        <v-col cols = "12">
            <v-row justify-center>
                <v-col cols="7">
                    <v-btn style="margin-top: -30px !important;" 
                        class='ghd-blue ghd-button-text ghd-outline-button-padding ghd-button' variant = "outlined"
                        @click="onShowConfirmDeleteAlert" :disabled="!CheckIfCanDelete()">
                        Delete
                    </v-btn>
                </v-col>
            </v-row>
        </v-col>
        <v-col cols = "12">
            <v-row justify-center>
                <v-col cols="7">
                    <p v-if="CheckIfDatesAreInvalid()" style="color: red;">The lower bound date cannot be later than the upper bound date</p><br/>
                    <p v-if="!isNil(upperBoundDate) && upperBoundDate.trim() != '' && (isNil(lowerBoundDate) || lowerBoundDate.trim() == '')" style="color: red;">If the upper bound has a value and the lower bound is blank then all outputs created prior to upper bound will be deleted</p>
                </v-col>
            </v-row>
        </v-col>
        <Alert
            :dialogData="confirmDeleteAlertData"
            @submit="onConfirmDeleteAlertSubmit"
        />
    </v-row>
</template>

<script lang="ts" setup>
    import { prop } from 'ramda';
import Vue, { computed } from 'vue';
    import {inject, reactive, ref, onMounted, onBeforeUnmount, watch, Ref} from 'vue';
    import { useStore } from 'vuex';
    import {SimulationOutputDeletionParameters} from '@/shared/models/iAM/simulation-output-deletion-params'
    import Alert from '@/shared/modals/Alert.vue';
import { emptyAlertData } from '@/shared/models/modals/alert-data';
import { any, clone, isNil } from 'ramda';
import {
    InputValidationRules,
    rules as validationRules,
} from '@/shared/utils/input-validation-rules';
import ScenarioService from '@/services/scenario.service';
import { WorkType } from '@/shared/models/iAM/scenario';
import { Emitter, EventType } from 'mitt';
import { Hub } from '@/connectionHub';

    let store = useStore();
    let lowerBoundDate = ref('');
    let upperBoundDate = ref('');
    let isDeletionInProgress = false;
    let rules: InputValidationRules = validationRules;
    let confirmDeleteAlertData = ref(clone(emptyAlertData));
    const $emitter = inject('emitter') as Emitter<Record<EventType, unknown>>
    async function deleteScenarioOutputAction(payload?: any): Promise<any> {await store.dispatch('deleteScenarioOutput',payload);}
    function setAlertMessageAction(payload?: any) { store.dispatch('setAlertMessage', payload);}

    onMounted( () => {
            ScenarioService.GetQueuedWorkByWorkType(WorkType.DeleteSimulationOutput).then(response => {
                if(response.data){
                    setAlertMessageAction("Simulation output deletion is currently already in the queue")
                    isDeletionInProgress = true;
                }
            })
            
            $emitter.on(
                Hub.BroadcastEventType.BroadcastSimulationOutputDeletionCompletionEvent,
                onDeletionComplete,
        );
    });

    onBeforeUnmount(() =>  {
        $emitter.off(
            Hub.BroadcastEventType.BroadcastSimulationOutputDeletionCompletionEvent,
            onDeletionComplete,
        );
        setAlertMessageAction('');
    });

    function DeleteScenarioOutputs() {
        let dateParams: SimulationOutputDeletionParameters = {lowerBoundDate: new Date(lowerBoundDate.value), upperBoundDate: new Date(upperBoundDate.value)}
        deleteScenarioOutputAction(dateParams).then(response => {
            setAlertMessageAction("Simulation output deletion is currently already in the queue")
        })
    }

    function CheckIfDatesAreInvalid(){
        return new Date(lowerBoundDate.value) > new Date(upperBoundDate.value)
    }
    function CheckIfCanDelete(){
        if(CheckIfDatesAreInvalid() || isNil(upperBoundDate.value) || upperBoundDate.value.trim() == '' || isDeletionInProgress)
            return false;
        else
            return true;
    }

    function onShowConfirmDeleteAlert() {
        confirmDeleteAlertData.value = {
            showDialog: true,
            heading: 'Warning',
            choice: true,
            message: 'Are you sure you want to delete?',
        };
    }

    function onConfirmDeleteAlertSubmit(submit: boolean) {
        confirmDeleteAlertData.value = clone(emptyAlertData);
        if (submit ) {
            DeleteScenarioOutputs()
        }
    }

    function onDeletionComplete(){
        isDeletionInProgress = false;
        setAlertMessageAction('');
    }
</script>