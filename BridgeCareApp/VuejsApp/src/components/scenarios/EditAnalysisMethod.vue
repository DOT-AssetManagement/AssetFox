<template>
    <v-card fluid  class="elevation-0 vcard-main-layout">
        <v-row >
            <v-col cols="12" lg="4" id="EditAnalysisMethod-weightingParent-vflex">
                <v-subheader class="ghd-control-label ghd-md-gray">Weighting</v-subheader>
                <v-select
                    id="EditAnalysisMethod-weighting-vselect"
                    class="ghd-select ghd-control-border ghd-control-text"
                    :items="weightingAttributes"
                    variant="outlined"
                    clearable
                    item-title="text"
                    item-value="value"
                    density="compact"
                    menu-icon=custom:GhdDownSvg
                    v-model="analysisMethod.attribute"
                    :disabled="!hasAdminAccess"
                    @update:model-value="onSetAnalysisMethodProperty('attribute',$event)">
                </v-select>
            </v-col>
            <v-col cols="12" lg="4" id="EditAnalysisMethod-optimizationStrategyParent-vflex">
                <v-subheader class="ghd-control-label ghd-md-gray">Optimization Strategy</v-subheader>
                <v-select 
                    id="EditAnalysisMethod-optimizationStrategy-select"
                    class="ghd-select ghd-control-border ghd-control-text"
                    :items="optimizationStrategy"
                    item-title="text"
                    item-value="value"
                    variant="outlined"
                    density="compact"
                    menu-icon=custom:GhdDownSvg
                    v-model="analysisMethod.optimizationStrategy"
                    @update:model-value="onSetAnalysisMethodProperty('optimizationStrategy',$event)"
                    :disabled="!hasAdminAccess">
                </v-select>
            </v-col>    
            <v-col cols="12" lg="4" id="EditAnalysisMethod-spendingStrategyParent-vflex">
                <v-subheader class="ghd-control-label ghd-md-gray">Spending Strategy</v-subheader>
                <v-select
                    id="EditAnalysisMethod-spendingStrategy-select"
                    class="ghd-select ghd-control-border ghd-control-text"
                    :items="spendingStrategy"
                    variant="outlined"
                    item-title="text"
                    item-value="value"
                    density="compact"
                    menu-icon=custom:GhdDownSvg
                    v-model="analysisMethod.spendingStrategy"
                    @update:model-value="onSetAnalysisMethodProperty('spendingStrategy',$event)">
                </v-select>
            </v-col>                      
        </v-row>
        <v-row>
            <v-col cols="12" md="4">
                <v-subheader class="ghd-control-label ghd-md-gray">Benefit Attribute</v-subheader>
                <v-select
                    id="EditAnalysisMethod-benefitAttribute-select"
                    class="ghd-select ghd-control-text ghd-control-border"
                    :items="benefitAttributes"
                    item-title="text"
                    item-value="value"
                    variant="outlined"
                    density="compact"
                    v-model="benefit.attribute"
                    menu-icon=custom:GhdDownSvg
                    @update:model-value="onSetBenefitProperty('attribute', $event)"
                    :disabled="!hasAdminAccess"
                >
                </v-select>
            </v-col>
            <v-col cols="12" md="4">
                <v-subheader class="ghd-control-label ghd-md-gray">Benefit Limit</v-subheader>
                <v-text-field
                    id="EditAnalysisMethod-benefitLimit-textField"
                    style="margin:0px"
                    class="ghd-control-text ghd-control-border"
                    @update:model-value="onSetBenefitProperty('limit',$event)"
                    variant="outlined"
                    density="compact"
                    type="number"
                    
                    :value.number="benefit.limit"
                    :rules="[
                        rules['generalRules'].valueIsNotEmpty,
                        rules['generalRules'].valueIsNotNegative(
                            analysisMethod.benefit.limit,
                        ),
                    ]"
                    :disabled="!hasAdminAccess">
                </v-text-field>
            </v-col>
        </v-row>
        <v-row>
            <v-col cols="12" lg="5">
                <v-switch 
                id="EditAnalysisMethod-allowMultiBudgetFunding-switch"
                class="ghd-checkbox"
                color="#2A578D"
                label="Allow Multi Budget Funding"
                :disabled="!hasAdminAccess"
                v-model="analysisMethod.shouldUseExtraFundsAcrossBudgets"
                @update:model-value='onSetAnalysisMethodProperty("shouldUseExtraFundsAcrossBudgets",$event)'/>
                
            </v-col>
            <v-col cols="12" lg="5">
                <v-switch 
                id="EditAnalysisMethod-allowMultipleTreatments-switch"
                class="ghd-checkbox"
                color="#2A578D"
                label="Allow Multiple Treatments"
                v-model="analysisMethod.shouldAllowMultipleTreatments"
                @update:model-value='onSetAnalysisMethodProperty("shouldAllowMultipleTreatments",$event)'/>
            </v-col>
        </v-row>
        <v-row>
            <v-col cols="12" lg="5">
                <v-switch 
                id="EditAnalysisMethod-useSuperAssets-switch"
                class="ghd-checkbox"
                color="#2A578D"
                label="Enable Super Assets"
                v-model="analysisMethod.shouldUseSuperAssets"
                @update:model-value='onSetAnalysisMethodProperty("shouldUseSuperAssets",$event)'/>
            </v-col>
            <template v-if="analysisMethod.shouldUseSuperAssets">
                <v-col cols = "12" lg="5">
                    <v-subheader class="ghd-control-label ghd-md-gray">Super Asset Attribute</v-subheader>
                    <v-select
                        id="EditAnalysisMethod-superAssetAttribute-select"
                        class="ghd-select ghd-control-border ghd-control-text"
                        :items="superAssetAttributes"
                        variant="outlined"
                        item-title="text"
                        item-value="value"
                        density="compact"
                        menu-icon=custom:GhdDownSvg
                        v-model="superAssetAttributeModel"
                        clearable
                        @update:model-value="onSetAnalysisMethodProperty('superAssetAttribute',$event)"
                        :disabled="!hasAdminAccess">
                    </v-select>
                </v-col>
            </template>
        </v-row>
        <v-row>
            <v-row justify="space-between" style="padding-left: 10px;">
                <v-col cols="12" lg="6">
                    <v-subheader class="ghd-control-label ghd-md-gray">Description</v-subheader>
                    <v-textarea
                        id="EditAnalysisMethod-description-textArea"
                        class="ghd-control-text ghd-control-border"
                        @update:model-value="onSetAnalysisMethodProperty('description', $event)"
                        no-resize
                        variant="outlined"
                        rows="6"
                        :model-value="analysisMethod.description"
                    >
                    </v-textarea>
                </v-col>
                <v-col cols="12" lg="6">
                    <v-row justify="space-between" style="padding: 10px;">
                    <v-subheader class="ghd-control-label ghd-md-gray">                             
                        Criteria ({{ assetCountString }})
                    </v-subheader>
                    <v-btn
                        id="EditAnalysisMethod-criteriaEditor-btn"
                        style="padding-right:30px; padding-bottom: 5px; margin-bottom: 5px !important;"
                        @click="
                            onShowCriterionEditorDialog
                        "
                        class="edit-icon ghd-control-label ghd-blue"
                        icon
                        flat
                        title="Edit"
                    >
                        <img class='img-general' :src="getUrl('assets/icons/edit.svg')"/> 
                    </v-btn>
                    </v-row>
                    <v-textarea
                        id="EditAnalysisMethod-criteria-textArea"
                        class="ghd-control-text ghd-control-border"
                        style="padding-bottom: 0px; height: 250px;"
                        no-resize
                        variant="outlined"
                        readonly
                        :rows="criteriaRows()"
                        v-model="analysisMethod.criterionLibrary.mergedCriteriaExpression"
                    >
                    </v-textarea>
                </v-col>
            </v-row>
        </v-row>
        <v-row justify="center" style="margin: 0;">
                <CancelButton 
                    :disabled="!hasUnsavedChanges"
                    @cancel="onDiscardChanges"
                />
                <SaveButton 
                    :disabled="!valid || !hasUnsavedChanges"
                    @save="onUpsertAnalysisMethod"
                />
        </v-row>
    </v-card>
    <GeneralCriterionEditorDialog
        :dialogData="criterionEditorDialogData"
        @submit="onCriterionEditorDialogSubmit"/>
    <Alert
    :dialogData="ConfirmEmptyCriteria"
    @submit="onConfirmEmptyCriteriaAlertSubmit"/>
    <ConfirmDialog></ConfirmDialog>
</template>

<script lang="ts" setup>
import Vue, { computed, ref, shallowReactive, shallowRef, watch, onMounted, onBeforeUnmount, inject } from 'vue'; 
import { clone, equals, isNil } from 'ramda';
import { hasValue } from '@/shared/utils/has-value-util';
import { Attribute } from '@/shared/models/iAM/attribute';
import { setItemPropertyValue } from '@/shared/utils/setter-utils';
import { getBlankGuid, getNewGuid } from '@/shared/utils/uuid-utils';
import {
    AnalysisMethod,
    Benefit,
    emptyAnalysisMethod,
    OptimizationStrategy,
    SpendingStrategy,
} from '@/shared/models/iAM/analysis-method';
import { SelectItem } from '@/shared/models/vue/select-item';
import { CriterionLibrary } from '@/shared/models/iAM/criteria';
import {
    InputValidationRules,
    rules as validationRules,
} from '@/shared/utils/input-validation-rules';
import {emptyAlertData} from '@/shared/models/modals/alert-data';
import Alert from '@/shared/modals/Alert.vue';
import GeneralCriterionEditorDialog from '@/shared/modals/GeneralCriterionEditorDialog.vue';
import { emptyGeneralCriterionEditorDialogData, GeneralCriterionEditorDialogData } from '@/shared/models/modals/general-criterion-editor-dialog-data';
import { useRouter } from 'vue-router'; 
import { getUrl } from '@/shared/utils/get-url';
import ConfirmDialog from 'primevue/confirmdialog';
import EditSvg from '@/shared/icons/EditSvg.vue';
import CancelButton from '@/shared/components/buttons/CancelButton.vue';
import SaveButton from '@/shared/components/buttons/SaveButton.vue';
import mitt, { Emitter, EventType } from 'mitt';
import { useAnalysisMethodStore, useAttributeStore, useAuthenticationStore, useNotificationStore, useScenarioStore, useUnsavedChangesStore } from '@/stores';
    const authenticationStore = useAuthenticationStore();
    const analysisMethodStore = useAnalysisMethodStore();
    const notificationStore = useNotificationStore();
    const scenarioStore = useScenarioStore();
    const router = useRouter(); 
    // ToDo - verify if below is correct. Its used in onUpsertAnalysisMethod()
    const $refs = inject('$refs') as any

    const $emitter = inject('emitter') as Emitter<Record<EventType, unknown>>

    const attributeStore = useAttributeStore();

    const hasAdminAccess = computed<boolean>(() => authenticationStore.hasAdminAccess) ; 
    const unsavedChangesStore = useUnsavedChangesStore();

    const selectedScenarioId = ref<string>(getBlankGuid());
    const analysisMethod = ref<AnalysisMethod>(clone(emptyAnalysisMethod));
    let benefit = computed<Benefit>(() => analysisMethod.value.benefit);
    let assetCountString = ref<string>("unknown asset count");
    const optimizationStrategy: SelectItem[] = [
        { text: 'Benefit', value: OptimizationStrategy.Benefit },
        {
            text: 'Lowest-Lifecycle-Cost (LLC)',
            value: OptimizationStrategy.BenefitToCostRatio,
        },
        { text: 'Remaining Life', value: OptimizationStrategy.RemainingLife },
        {
            text: 'Remaining life-to-Cost Ratio',
            value: OptimizationStrategy.RemainingLifeToCostRatio,
        },
    ];
    const spendingStrategy: SelectItem[] = [
        { text: 'No Spending', value: SpendingStrategy.NoSpending },
        {
            text: 'Unlimited Spending',
            value: SpendingStrategy.UnlimitedSpending,
        },
        {
            text: 'Until Target & Deficient Condition Goals Met',
            value: SpendingStrategy.UntilTargetAndDeficientConditionGoalsMet,
        },
        {
            text: 'Until Target Condition Goals Met',
            value: SpendingStrategy.UntilTargetConditionGoalsMet,
        },
        {
            text: 'Until Deficient Condition Goals Met',
            value: SpendingStrategy.UntilDeficientConditionGoalsMet,
        },
        { text: 'As Budget Permits', value: SpendingStrategy.AsBudgetPermits },
    ];
    const benefitAttributes = ref<SelectItem[]>([]);
    const weightingAttributes = ref<SelectItem[]>([{ text: '', value: '' }]);
    const superAssetAttributes = ref<SelectItem[]>([]);
    const simulationName = ref<string>();
    const networkName = ref<string>();
    const criterionEditorDialogData = ref<GeneralCriterionEditorDialogData>(clone(emptyGeneralCriterionEditorDialogData));
    const rules = ref<InputValidationRules>(validationRules);
    const valid = ref<boolean>(true);
    const criteriaIsIntentionallyEmpty = ref<boolean>(false);
    let hasUnsavedChanges = ref<boolean>(false);
    let previousSuperAssetAttribute = ref<string>('');
    let ConfirmEmptyCriteria = ref(clone(emptyAlertData));

    const superAssetAttributeModel = computed<string | null>({
        get() {
            const val = analysisMethod.value.superAssetAttribute;
            return !val ? null : val;
        },
        set(newVal) {
            analysisMethod.value = setItemPropertyValue(
            'superAssetAttribute',
            newVal ?? '',
            analysisMethod.value,
            );
        },
    });

    //beforeRouteEnter(to: any, from: any, next: any) {
       //next((vm: any) => {
    created(); 
    function created() { 
    }

    onMounted(() => {
        selectedScenarioId.value = router.currentRoute.value.query.scenarioId as string;
            simulationName.value = router.currentRoute.value.query.simulationName as string;
            networkName.value = router.currentRoute.value.query.networkName as string;
            if (selectedScenarioId.value === getBlankGuid()) {
                // set 'no selected scenario' error message, then redirect user to Scenarios UI
                notificationStore.addErrorNotification({
                    message: 'Found no selected scenario for edit',
                });
                router.push('/Scenarios/');
            }

// Get the selected scenario's analysis method data
analysisMethodStore.getAnalysisMethod(selectedScenarioId.value)
    .then(() => {
        // Get simulation analysis setting
        return analysisMethodStore.getSimulationAnalysisSetting(selectedScenarioId.value);
    })
    .then(() => {
        // Get current user or shared scenario info
        return scenarioStore.getCurrentUserOrSharedScenario(selectedScenarioId.value);
    })
    .then(() => {
        // Select scenario
        scenarioStore.selectScenario(selectedScenarioId.value);
    })
    .catch(error => {
        console.error('Error in action chain:', error);
    });

        if (hasValue(attributeStore.getNumericAttributes)) {
            setBenefitAndWeightingAttributes();
            setSuperAssetAttributes();
        }
    });

    onBeforeUnmount(() => {
        unsavedChangesStore.setHasUnsavedChanges(false);
    });

    watch(() =>analysisMethodStore.analysisMethodGetter, () => {
        analysisMethod.value = {
            ...analysisMethodStore.analysisMethodGetter,
            benefit: {
                ...analysisMethodStore.analysisMethodGetter.benefit,
            },
        };
    });

    // watch(benefit, onAnalysisChanged)
    watch(analysisMethod, onAnalysisChanged)
     async function onAnalysisChanged() {
        hasUnsavedChanges.value = !equals(analysisMethod.value, analysisMethodStore.analysisMethodGetter) || !analysisMethodStore.simulationAnalysisSettingGetter;

        unsavedChangesStore.setHasUnsavedChanges(hasUnsavedChanges.value);

         setBenefitAttributeIfEmpty();
         updateAssetCountString();
    }

    watch(() => analysisMethodStore.simulationAnalysisSettingGetter, (newVal) => {
        if(newVal === false)
        {
            hasUnsavedChanges.value = true;
        }
    });

    watch(() =>attributeStore.getNumericAttributes, () => {
        if (hasValue(attributeStore.getNumericAttributes)) {
            setBenefitAndWeightingAttributes();
            setSuperAssetAttributes();
            setBenefitAttributeIfEmpty();
        }
    });

     function setBenefitAttributeIfEmpty() {
        if (
            !hasValue(analysisMethod.value.benefit.attribute) &&
            hasValue(benefitAttributes.value)
        ) {
            analysisMethod.value.benefit.attribute = benefitAttributes.value[0].value.toString();
        }
    }

    function updateAssetCountString() {
        console.log("updating asset count");
        console.log(analysisMethod);
        if (hasValue(analysisMethod.value.lastKnownAssetCount) && analysisMethod.value.lastKnownAssetCount!=-1) {
            assetCountString.value = "Last known asset count: " + analysisMethod.value.lastKnownAssetCount;
        } else {
            assetCountString.value = "unknown asset count";
        }
    }
 
    function onSetAnalysisMethodProperty(property: string, value: any) {
        analysisMethod.value = setItemPropertyValue(
            property,
            value,
            analysisMethod.value,
        );

        if (property === 'shouldUseSuperAssets') {
            if (!value) {
                previousSuperAssetAttribute.value = analysisMethod.value.superAssetAttribute || '';
                analysisMethod.value = setItemPropertyValue(
                    'superAssetAttribute',
                    '',
                    analysisMethod.value,
                );
            } else if (previousSuperAssetAttribute.value) {
                analysisMethod.value = setItemPropertyValue(
                    'superAssetAttribute',
                    previousSuperAssetAttribute.value,
                    analysisMethod.value,
                );
            }
        }
    }

    function onSetBenefitProperty(property: string, value: any) {
        analysisMethod.value.benefit = setItemPropertyValue(
            property,
            value,
            analysisMethod.value.benefit,
        );
        onAnalysisChanged();
    }

    function setBenefitAndWeightingAttributes() {
        const numericAttributeSelectItems: SelectItem[] = attributeStore.getNumericAttributes.map(
            (attribute: Attribute) => ({
                text: attribute.name,
                value: attribute.name,
            }),
        );
        benefitAttributes.value = [...numericAttributeSelectItems];
        weightingAttributes.value = [
            weightingAttributes.value[0],
            ...numericAttributeSelectItems,
        ];
    }

    function setSuperAssetAttributes() {
        const superAssetAttributeSelectItems: SelectItem[] = attributeStore.attributesgetter.map(
            (attribute: Attribute) => ({
                text: attribute.name,
                value: attribute.name,
            }),
        );
        superAssetAttributes.value = [...superAssetAttributeSelectItems];
    }

    function onShowCriterionEditorDialog() {
        criterionEditorDialogData.value = {
            showDialog: true,
            CriteriaExpression: analysisMethod.value.criterionLibrary.mergedCriteriaExpression,
        };
    }

    function onCriterionEditorDialogSubmit(criterionexpression: string, resultsCount: number | null) {
        criterionEditorDialogData.value = clone(
            emptyGeneralCriterionEditorDialogData,
        );

        if (!isNil(criterionexpression)) {
            if(analysisMethod.value.criterionLibrary.id == getBlankGuid())
                analysisMethod.value.criterionLibrary.id = getNewGuid();
            let assetCount = resultsCount == -1 ? analysisMethod.value.lastKnownAssetCount : resultsCount;
            analysisMethod.value = {
                ...analysisMethod.value,
                lastKnownAssetCount: assetCount ?? -1,
                criterionLibrary: {...analysisMethod.value.criterionLibrary, mergedCriteriaExpression: criterionexpression} as CriterionLibrary,
            };
        }
    }

    function criteriaIsEmpty()
    {
        return (isNil(analysisMethod.value.criterionLibrary) ||
                isNil(analysisMethod.value.criterionLibrary.mergedCriteriaExpression) ||
                analysisMethod.value.criterionLibrary.mergedCriteriaExpression == ""
                );
    }

    function criteriaRows()
    {
        return criteriaIsEmpty() ? 4 : 6;
    }

    function criteriaIsInvalid() {
        return criteriaIsEmpty() && !criteriaIsIntentionallyEmpty.value;
    }    

    async function onUpsertAnalysisMethod() {
        if(criteriaIsEmpty())
        {
            onConfirmEmptyCriteriaAlert();
        }
        else
        {
            if(analysisMethod.value.benefit.id === getBlankGuid())
            analysisMethod.value.benefit.id = getNewGuid();
            const responseData = await analysisMethodStore.upsertAnalysisMethod(analysisMethod.value, selectedScenarioId.value);
            
            if(responseData === "Analysis Method successfully updated")
            {
                $emitter.emit('AnalysisMethodUpdated');              
            }
        }
    }

    function onConfirmEmptyCriteriaAlert() {
        ConfirmEmptyCriteria.value = {
            showDialog: true,
            heading: 'Warning',
            choice: true,
            message:
                'Warning: Criteria field is blank. A blank Criteria field will default to your user assigned assets.',
        };
    }

    async function onConfirmEmptyCriteriaAlertSubmit(submit: boolean) {
        ConfirmEmptyCriteria.value = clone(emptyAlertData);

        if (submit) {
            if(analysisMethod.value.benefit.id === getBlankGuid())
            analysisMethod.value.benefit.id = getNewGuid();

            const responseData = await analysisMethodStore.upsertAnalysisMethod(analysisMethod.value, selectedScenarioId.value);
            
            if(responseData === "Analysis Method successfully updated")
            {
                $emitter.emit('AnalysisMethodUpdated');              
            }
        } 
    }

    function onDiscardChanges() {
        analysisMethod.value = clone(analysisMethodStore.analysisMethodGetter);
    }



</script>
