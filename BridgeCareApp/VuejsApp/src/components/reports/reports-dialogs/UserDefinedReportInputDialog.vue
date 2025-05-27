<template>
    <v-dialog width="60%" height="80%" persistent v-model="showDialog">
        <v-card style="height: 98%;">
            <v-card-title class="ghd-dialog-padding-top-title">
                <v-row justify="space-between">
                    <v-col cols="12">
                        <div class="ghd-control-dialog-header">
                            <h5>Select User Defined Report inputs <XButton class="float-right" @click="onSubmit(false)"/></h5>
                        </div>
                    </v-col>
                </v-row>
            </v-card-title>
            <v-card-text class="ghd-dialog-box-padding-center">
                <v-row style="height: 70px; width: 98%;" class="p-2 mt-n4">
                    <v-col>
                        <div class="row">
                            <v-subheader class="ghd-md-gray ghd-control-label pl-1">Attributes</v-subheader>                            
                            <div style="max-width: 85%; height: 70px; max-height: 95%; overflow-y: auto; overflow-wrap: normal;" class="mt-n1">
                                <span class="sm-txt p-2"> {{ newUserDefinedReportRequestModel.attributes.join(', ') || 'None' }}
                                </span>
                            </div>
                            <v-btn
                                class="ghd-blue mt-n3"
                                @click="openAttributeDialog(newUserDefinedReportRequestModel.attributes)"
                                flat
                                icon
                            >
                                <img class='img-general' :src="getUrl('assets/icons/edit.svg')"/> 
                            </v-btn>                            
                        </div>
                    </v-col>
                </v-row>
                <v-row style="height: 70px; width: 98%;" class="p-2 mt-n2">
                    <v-col>
                        <div class="row">
                            <v-subheader class="ghd-md-gray ghd-control-label pl-1">Years</v-subheader>                            
                            <div style="max-width: 85%; height: 70px; max-height: 95%; overflow-y: auto; overflow-wrap: normal;" class="mt-n1">
                                <span class="sm-txt p-2"> {{ newUserDefinedReportRequestModel.years.join(', ') || 'None' }}
                                </span>
                            </div>
                            <v-btn
                                class="ghd-blue mt-n3"
                                @click="openYearDialog(newUserDefinedReportRequestModel.years)"
                                flat
                                icon
                            >
                                <img class='img-general' :src="getUrl('assets/icons/edit.svg')"/> 
                            </v-btn>                            
                        </div>
                    </v-col>
                </v-row>
                <v-row>                 
                    <v-col class="mb-n3">
                        <v-subheader class="ghd-control-text">Select below options to view in the report:</v-subheader>                    
                    </v-col>   
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "4">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayConditionOfNetwork-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="ConditionOfNetwork"
                        v-model="newUserDefinedReportRequestModel.displayConditionOfNetwork"
                        @update:model-value='onSetDisplayProperty("displayConditionOfNetwork",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "4">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayInitialAssetSummaries-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Initial Assets"
                        v-model="newUserDefinedReportRequestModel.displayInitialAssets"
                        @update:model-value='onSetDisplayProperty("displayInitialAssetSummaries",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "4">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayAssets-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Year Assets"
                        v-model="newUserDefinedReportRequestModel.displayYearAssets"
                        @update:model-value='onSetDisplayProperty("displayAssets",$event)'/>
                    </v-col>
                </v-row>                
                <v-row style="height: 40px;">
                    <v-col cols = "4">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayBudgets-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Budgets"
                        v-model="newUserDefinedReportRequestModel.displayBudgets"
                        @update:model-value='onSetDisplayProperty("displayBudgets",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayDeficientConditionGoals-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Deficient Condition Goals"
                        v-model="newUserDefinedReportRequestModel.displayDeficientConditionGoals"
                        @update:model-value='onSetDisplayProperty("displayDeficientConditionGoals",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayTargetConditionGoals-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Target Condition Goals"
                        v-model="newUserDefinedReportRequestModel.displayTargetConditionGoals"
                        @update:model-value='onSetDisplayProperty("displayTargetConditionGoals",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayTreatmentOptions-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Treatment Options"
                        v-model="newUserDefinedReportRequestModel.displayTreatmentOptions"
                        @update:model-value='onSetDisplayProperty("displayTreatmentOptions",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayTreatmentSchedulingCollisions-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Treatment Scheduling Collisions"
                        v-model="newUserDefinedReportRequestModel.displayTreatmentSchedulingCollisions"
                        @update:model-value='onSetDisplayProperty("displayTreatmentSchedulingCollisions",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayTreatmentRejections-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Treatment Rejections"
                        v-model="newUserDefinedReportRequestModel.displayTreatmentRejections"
                        @update:model-value='onSetDisplayProperty("displayTreatmentRejections",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayTreatmentCashflowConsiderations-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Treatment Cashflow Considerations"
                        v-model="newUserDefinedReportRequestModel.displayTreatmentCashflowConsiderations"
                        @update:model-value='onSetDisplayProperty("displayTreatmentCashflowConsiderations",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "6">
                        <v-switch
                        id="UserDefinedReportInputDialog-displyTreatmentCurrentBudgetsToSpend-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Treatment Current Budgets To Spend"
                        v-model="newUserDefinedReportRequestModel.displyTreatmentCurrentBudgetsToSpend"
                        @update:model-value='onSetDisplayProperty("displyTreatmentCurrentBudgetsToSpend",$event)'/>
                    </v-col>
                </v-row>
                <v-row style="height: 40px;">
                    <v-col cols = "5">
                        <v-switch
                        id="UserDefinedReportInputDialog-displayTreatmentAllocations-switch"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Treatment Allocations"
                        v-model="newUserDefinedReportRequestModel.displayTreatmentAllocations"
                        @update:model-value='onSetDisplayProperty("displayTreatmentAllocations",$event)'/>
                    </v-col>
                </v-row>
            </v-card-text>

            <v-card-actions class="ghd-dialog-box-padding-bottom">
                <v-row justify="center">
                <CancelButton @cancel="onSubmit(false)"/>
                <SubmitButton 
                    @submit="onSubmit(true)"
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

            <YearSelectionPopup
                v-model="showYearDialog"
                :available-years="yearSelectItems"
                :initial-selected-years="currentEditingYears"
                @save="handleSaveYears"
            />
        </v-card>        
    </v-dialog>    
</template>

<script setup lang="ts">    
    import { toRefs, ref, computed, onMounted } from 'vue';
    import { getUrl } from '@/shared/utils/get-url';
    import { setItemPropertyValue } from '@/shared/utils/setter-utils';
    import { emptyUserDefinedReportRequestModel, UserDefinedReportRequestModel } from '@/shared/models/iAM/reports';
    import SubmitButton from '@/shared/components/buttons/SubmitButton.vue';
    import CancelButton from '@/shared/components/buttons/CancelButton.vue';
    import XButton from '@/shared/components/buttons/XButton.vue';
    import AttributeSelectionPopup from '@/components/reports/reports-dialogs/AttributeSelectionPopup.vue';
    import YearSelectionPopup from '@/components/reports/reports-dialogs/YearSelectionPopup.vue';
    import { useStore } from 'vuex';
    import { Attribute } from '@/shared/models/iAM/attribute';
    import { InvestmentPlan } from '@/shared/models/iAM/investment';
    const props = defineProps({
        showDialog: Boolean
    })
    const { showDialog } = toRefs(props);
    const emit = defineEmits(['submit']);
    const selectedScenarioId = ref('');

    let store = useStore();
    import { useRouter } from 'vue-router'; 
    const router = useRouter();    
    let newUserDefinedReportRequestModel = ref<UserDefinedReportRequestModel>({...emptyUserDefinedReportRequestModel});    
        
    const showAttributeDialog = ref(false);
    const currentEditingAttributes = ref<string[]>([]);
    let stateAttributes = computed<Attribute[]>(() => store.state.attributeModule.attributes);
    let attributeSelectItems = stateAttributes.value.map((attribute: Attribute) => (attribute.name));
    
    const showYearDialog = ref(false);
    const currentEditingYears = ref<number[]>([]);
    async function getInvestmentAction(payload?: any): Promise<any> { await store.dispatch('getInvestment', payload); }
    let stateInvestmentPlan = computed<InvestmentPlan>(() => store.state.investmentModule.investmentPlan);
    let yearSelectItems: number[];

    onMounted(async () => {       
        selectedScenarioId.value = router.currentRoute.value.query.scenarioId as string; 
        await getInvestmentAction(selectedScenarioId.value);        
        yearSelectItems = Array.from(Array(stateInvestmentPlan.value?.numberOfYearsInAnalysisPeriod), (_, index) => 
        index + stateInvestmentPlan.value?.firstYearOfAnalysisPeriod) ?? [];
    });

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
        currentEditingAttributes.value = attributes; // current values
        showAttributeDialog.value = true;
    }

    function handleSaveAttributes(newAttrbutes: string[]) {        
        newUserDefinedReportRequestModel.value.attributes = newAttrbutes;

        // Reset editing state        
        currentEditingAttributes.value = [];
    }

    function openYearDialog(years: number[]) {                        
        currentEditingYears.value = years; // current values
        showYearDialog.value = true;
    }

    function handleSaveYears(newYears: number[]) {        
        newUserDefinedReportRequestModel.value.years = newYears;

        // Reset editing state        
        currentEditingYears.value = [];
    }
</script>