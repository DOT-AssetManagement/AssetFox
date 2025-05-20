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
                        <!-- class="treatment-cell-content" -->
                        <div> 
                            <!-- <span class="sm-txt" :style="getTreatmentStyle(newUserDefinedReportRequestModel.Attributes)"></span> -->
                            <span class="sm-txt"> {{ newUserDefinedReportRequestModel.attributes.join(', ') || 'None' }}
                            </span>
                            <v-btn
                                class="ghd-blue"
                                @click="openAttributeDialog(newUserDefinedReportRequestModel.attributes)"
                                flat
                                icon
                            >
                                <img class='img-general' :src="getUrl('assets/icons/edit.svg')"/> 
                            </v-btn>
                        </div>
                    </v-col>
                </v-row>
                <v-row style="height: 60px;">      
                    <v-col>
                        <v-subheader class="ghd-md-gray ghd-control-label">Years</v-subheader>                        
                    </v-col>
                    <v-col cols="10">
                        <!-- <div> 
                            <v-btn
                                icon="mdi-pencil"
                                size="x-small"
                                variant="tonal"
                                class="ml-2"
                                @click="openYearDialog(newUserDefinedReportRequestModel.Years)"
                            ></v-btn>
                        </div> -->
                    </v-col>
                </v-row>
                <v-row style="height: 50px;">
                    <v-col cols = "4">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayAssets-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Display Assets"
                        v-model="newUserDefinedReportRequestModel.displayAssets"
                        @update:model-value='onSetDisplayProperty("displayAssets",$event)'/>
                    </v-col>
                </v-row>                
                <v-row style="height: 50px;">
                    <v-col cols = "4">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayBudgets-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Display Budgets"
                        v-model="newUserDefinedReportRequestModel.displayBudgets"
                        @update:model-value='onSetDisplayProperty("DisplayBudgets",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 50px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayDeficientConditionGoals-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Display Deficient Condition Goals"
                        v-model="newUserDefinedReportRequestModel.displayDeficientConditionGoals"
                        @update:model-value='onSetDisplayProperty("DisplayDeficientConditionGoals",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 60px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayTargetConditionGoals-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Display Target Condition Goals"
                        v-model="newUserDefinedReportRequestModel.displayTargetConditionGoals"
                        @update:model-value='onSetDisplayProperty("DisplayTargetConditionGoals",$event)'/>
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
                :initial-selected-attributes="currentEditingAttributes"
                @save="handleSaveAttributes"
            />
        </v-card>        
    </v-dialog>    
</template>

<script setup lang="ts">    
    import Vue, { toRefs, ref, computed } from 'vue';
    import { getUrl } from '@/shared/utils/get-url';
    import { setItemPropertyValue } from '@/shared/utils/setter-utils';
    import { emptyUserDefinedReportRequestModel, UserDefinedReportRequestModel } from '@/shared/models/iAM/reports';
    import { InputValidationRules, rules as validationRules } from '@/shared/utils/input-validation-rules';
    import SaveButton from '@/shared/components/buttons/SaveButton.vue';
    import CancelButton from '@/shared/components/buttons/CancelButton.vue';
    import XButton from '@/shared/components/buttons/XButton.vue';
    import AttributeSelectionPopup from '@/components/reports/reports-dialogs/AttributeSelectionPopup.vue';
    import { useStore } from 'vuex';
    import { Attribute } from '@/shared/models/iAM/attribute';

    const props = defineProps({
        showDialog: Boolean
    })
    const { showDialog } = toRefs(props);

    const emit = defineEmits(['submit'])

    let store = useStore();
    let stateAttributes = computed<Attribute[]>(() => store.state.attributeModule.attributes) ;
    let newUserDefinedReportRequestModel = ref<UserDefinedReportRequestModel>({...emptyUserDefinedReportRequestModel});
    let rules: InputValidationRules = validationRules;
    const showAttributeDialog = ref(false);
    const currentEditingAttributes = ref<string[]>([]);
    // const attributeSelectItems = ref<string[]>([]);
    let attributeSelectItems = stateAttributes.value.map((attribute: Attribute) => (attribute.name));

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

    function onSetDisplayProperty(property: string, value: any) {
        newUserDefinedReportRequestModel.value = setItemPropertyValue(
            property,
            value,
            newUserDefinedReportRequestModel.value,
        );
    }

    function openAttributeDialog(attributes: string[]) {                        
        currentEditingAttributes.value = attributes; // Pass current attributes
        showAttributeDialog.value = true;
    }

    function handleSaveAttributes(newAttrbutes: string[]) {
        alert('in handleSaveAttributes');
        newUserDefinedReportRequestModel.value.attributes = newAttrbutes;

        // Reset editing state        
        currentEditingAttributes.value = []; // do we need this?
    }
</script>