<template>
    <v-dialog width="60%" persistent v-model="showDialog">
        <v-card>
            <v-card-title class="ghd-dialog-padding-top-title">
                <v-row justify="space-between">
                    <v-col>
                        <div class="ghd-control-dialog-header"><h5>Select User Defined Report intputs</h5></div>
                        <XButton @click="onSubmit(false)"/>
                    </v-col>
                </v-row>
            </v-card-title>
            <v-card-text class="ghd-dialog-box-padding-center">
                <v-row style="height: 60px;">
                    <v-col>
                        <v-subheader class="ghd-md-gray ghd-control-label">Attributes</v-subheader>                        
                    </v-col>
                    <v-col cols="10">
                        <div> 
                            <!-- <span class="sm-txt" > {{ newUserDefinedReportRequestModel.Attributes.join(', ') || 'None' }}
                            </span> -->
                            <v-btn
                                icon="mdi-pencil"
                                size="x-small"
                                variant="tonal"
                                class="ml-2"
                                @click="openAttributeDialog(newUserDefinedReportRequestModel.Attributes)"
                            ></v-btn>
                        </div>
                    </v-col>
                </v-row>
                <v-row style="height: 60px;">      
                    <v-col>
                        <v-subheader class="ghd-md-gray ghd-control-label">Years</v-subheader>                        
                    </v-col>
                    <v-col cols="10">
                        <div> 
                            <v-btn
                                icon="mdi-pencil"
                                size="x-small"
                                variant="tonal"
                                class="ml-2"
                                @click="openYearDialog(newUserDefinedReportRequestModel.Years)"
                            ></v-btn>
                        </div>
                    </v-col>
                </v-row>
                <v-row style="height: 50px;">
                    <v-col cols = "4">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayAssets-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Display Assets"
                        v-model="newUserDefinedReportRequestModel.DisplayAssets"
                        @update:model-value='onSetDisplayAssets("displayAssets",$event)'/>
                    </v-col>
                </v-row>                
                <v-row style="height: 50px;">
                    <v-col cols = "4">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayBudgets-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Display Budgets"
                        v-model="newUserDefinedReportRequestModel.DisplayBudgets"
                        @update:model-value='onSetDisplayBudgets("DisplayBudgets",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 50px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayDeficientConditionGoals-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Display Deficient Condition Goals"
                        v-model="newUserDefinedReportRequestModel.DisplayDeficientConditionGoals"
                        @update:model-value='onSetDisplayAssets("DisplayDeficientConditionGoals",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 60px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayTargetConditionGoals-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Display Target Condition Goals"
                        v-model="newUserDefinedReportRequestModel.DisplayTargetConditionGoals"
                        @update:model-value='onSetDisplayTargetConditionGoals("DisplayTargetConditionGoals",$event)'/>
                    </v-col>
                </v-row>
            </v-card-text>

            <v-card-actions class="ghd-dialog-box-padding-bottom">
                <v-row justify="center">
                <CancelButton @cancel="onSubmit(false)"/>
                <SaveButton 
                    @save="onSubmit(true)"
                    :disabled="disableSubmitButton()"
                />        
                </v-row>
            </v-card-actions>

            <AttributeSelectionPopup
                v-model="showAttributeDialog"
                :available-attributes="attributeSelectItems"
                :initial-selected-treatments="currentEditingAttributes"
                @save="handleSaveAttributes"
            />
        </v-card>        
    </v-dialog>    
</template>

<script setup lang="ts">
    import Vue, { toRefs, ref } from 'vue';
    import { emptyUserDefinedReportRequestModel, UserDefinedReportRequestModel } from '@/shared/models/iAM/reports';
    import {InputValidationRules, rules as validationRules} from '@/shared/utils/input-validation-rules';
    import SaveButton from '@/shared/components/buttons/SaveButton.vue';
    import CancelButton from '@/shared/components/buttons/CancelButton.vue';
    import XButton from '@/shared/components/buttons/XButton.vue';

    const props = defineProps({
        showDialog: Boolean
    })
    const { showDialog } = toRefs(props);

    const emit = defineEmits(['submit'])

    let newUserDefinedReportRequestModel = ref<UserDefinedReportRequestModel>({...emptyUserDefinedReportRequestModel});
    let rules: InputValidationRules = validationRules;

    function disableSubmitButton() {
        return false; // any rules to check?
    }

    function onSubmit(submit: boolean) {
        if (submit) {
        emit('submit', newUserDefinedReportRequestModel.value);
        } else {
        emit('submit', null);
        }

        newUserDefinedReportRequestModel.value = {...emptyUserDefinedReportRequestModel};
    }
</script>