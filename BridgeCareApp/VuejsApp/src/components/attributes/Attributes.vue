<template>
    <v-row column class="Montserrat-font-family">
        <v-col cols="12">
            <v-col cols ="8" class="ghd-constant-header">
                <v-row>
                    <v-subheader class="ghd-md-gray ghd-control-label" style="margin-bottom:1%; margin-right:1%;">Attribute</v-subheader>
                </v-row>
                <v-row>
                    <v-row column>
                        <v-select :items='selectAttributeItems'
                            item-title="text"
                            item-value="value"
                            id="Attributes-selectAttribute-vselect"
                            variant="outlined"
                            style="margin-left:1%;"
                            menu-icon=custom:GhdDownSvg
                            append-icon="@/assets/icons/down.svg"
                            v-model='selectAttributeItemValue' class="ghd-select ghd-text-field ghd-text-field-border"
                            density="compact">
                        </v-select>                           
                    </v-row>
                    <v-btn style="margin-top: -11px;" id="Attributes-addAttribute-vbtn" @click="addAttribute" class='ghd-blue ghd-button-text ghd-outline-button-padding ghd-button' variant = "outlined">
                        Add Attribute
                    </v-btn>
                </v-row>
            </v-col>
        </v-col>
        <v-divider style="background-color: #798899 !important;" v-if="hasSelectedAttribute"/>
        <v-col cols="12" v-if="hasSelectedAttribute" class="ghd-constant-header" >
            <v-row>
                <v-col cols="2"> 
                    <v-subheader class="ghd-md-gray ghd-control-label" style="margin-left:2%;">Attribute</v-subheader>
                    <v-text-field id="Attributes-attributeName-vtextfield" variant="outlined" class="ghd-text-field-border ghd-text-field"
                         v-model='selectedAttribute.name' 
                         @update:model-value="onSetSelectedAttributeProperty('name',$event)" density="compact"/>
                </v-col>
                <v-col cols="2">
                    <v-subheader class="ghd-md-gray ghd-control-label">Data Type</v-subheader>
                    <v-select
                          item-title="text"
                            item-value="value"
                            menu-icon=custom:GhdDownSvg
                        id="Attributes-attributeDataType-vselect"
                        variant="outlined"
                        class="ghd-select ghd-text-field ghd-text-field-border"
                        :items='typeSelectValues'
                        v-model='selectedAttribute.type'
                        @update:model-value="onSetSelectedAttributeProperty('type',$event)"
                        density="compact">
                    </v-select>                           
                </v-col>
            </v-row>
        </v-col>
        <v-col cols="12" v-if="hasSelectedAttribute">
            <v-col cols="10">
                <v-row>
                    <v-col cols="2" class="pl-0">
                        <v-subheader class="ghd-md-gray ghd-control-label">Default Value</v-subheader>
                        <v-text-field id="Attributes-attributeDefaultString-vtextfield" v-if="selectedAttribute.type == 'STRING'" variant="outlined" class="ghd-text-field-border ghd-text-field"
                            v-model='selectedAttribute.defaultValue'
                            @update:model-value="onSetSelectedAttributeProperty('defaultValue',$event)" density="compact"/>
                        <v-text-field id="Attributes-attributeDefaultNumber-vtextfield" v-if="selectedAttribute.type != 'STRING'" variant="outlined" class="ghd-text-field-border ghd-text-field"
                            v-model='selectedAttribute.defaultValue'
                            @update:model-value="onSetSelectedAttributeProperty('defaultValue',$event)"
                            density="compact"/>
                    </v-col>
                    <v-col cols="2">
                        <v-subheader class="ghd-md-gray ghd-control-label">Minimum Value</v-subheader>
                        <v-text-field id="Attributes-attributeMinimumValue-vtextfield" variant="outlined" class="ghd-text-field-border ghd-text-field"                            
                            v-model='selectedAttribute.minimum'
                            @update:model-value="onSetSelectedAttributeProperty('minimum',$event)"
                            density="compact"/>
                    </v-col>
                    <v-col cols="2">
                        <v-subheader class="ghd-md-gray ghd-control-label">Maximum Value</v-subheader>
                        <v-text-field id="Attributes-attributeMaximumValue-vtextfield" variant="outlined" class="ghd-text-field-border ghd-text-field"
                            v-model='selectedAttribute.maximum'
                            @update:model-value="onSetSelectedAttributeProperty('maximum',$event)"
                            density="compact"/>
                    </v-col>
                    <v-col cols="4" style="padding-top:40px;">
                        <v-row>
                        <v-switch id="Attributes-attributeCalculated-vswitch" class='sharing header-text-content' color="#2A578D"  label='Calculated' 
                            v-model='selectedAttribute.isCalculated'
                            @update:model-value="onSetSelectedAttributeProperty('isCalculated',$event)"/>
                        <v-switch id="Attributes-attributeAscending-vswitch" class='sharing header-text-content' color="#2A578D" label='Ascending' 
                            v-model='selectedAttribute.isAscending'
                            @update:model-value="onSetSelectedAttributeProperty('isAscending',$event)"/>
                        </v-row>
                    </v-col>
                </v-row>
            </v-col>
        </v-col>
        <!-- The Buttons  -->
        <v-col cols="12" v-if="hasSelectedAttribute">        
            <v-row justify="center">
                <v-btn id="Attributes-cancel-vbtn" :disabled='!hasUnsavedChanges' @click='onDiscardChanges' variant = "outlined" class='ghd-blue ghd-button-text ghd-outline-button-padding ghd-button vertical-center'>
                    Cancel
                </v-btn>
                <v-btn id="Attributes-save-vbtn" @click='saveAttribute' :disabled='disableCrudButtons() || !hasUnsavedChanges'  variant = "outlined" class='ghd-blue ghd-button-text ghd-outline-button-padding ghd-button vertical-center'>
                    Save
                </v-btn>               
            </v-row>
        </v-col>
        <ConfirmDialog></ConfirmDialog>
    </v-row>
</template>

<script setup lang='ts'>
import AttributeService from '@/services/attribute.service';
import { Attribute, emptyAttribute, RuleDefinition } from '@/shared/models/iAM/attribute';
import { ValidationResult } from '@/shared/models/iAM/expression-validation';
import { SelectItem } from '@/shared/models/vue/select-item';
import { hasUnsavedChangesCore } from '@/shared/utils/has-unsaved-changes-helper';
import { hasValue } from '@/shared/utils/has-value-util';
import { InputValidationRules, rules as validationRules } from '@/shared/utils/input-validation-rules';
import { TestStringData } from '@/shared/models/iAM/test-string';
import { getBlankGuid, getNewGuid } from '@/shared/utils/uuid-utils';
import { AxiosResponse } from 'axios';
import { any, clone, find, isNil, propEq } from 'ramda';
import Vue, { computed, onBeforeMount, onBeforeUnmount, Ref, ref, shallowRef, ShallowRef, watch } from 'vue';
import { Console } from 'console';
import { useStore } from 'vuex';
import ConfirmDialog from 'primevue/confirmdialog';
import { setItemPropertyValue } from '@/shared/utils/setter-utils';

    let store = useStore();
    let hasSelectedAttribute = ref<boolean>(false);
    let selectAttributeItemValue = ref<string | null>(null);
    let selectedAttribute = ref<Attribute>(clone(emptyAttribute));
    let rules: InputValidationRules = validationRules;
    let validationErrorMessage = ref<string>('');
    let ValidationSuccessMessage = ref<string>('');
    
    let typeSelectValues = ref<SelectItem[]>([
        {text: 'STRING', value: 'STRING'},
        {text: 'NUMBER', value: 'NUMBER'}
    ]);

    let stateAttributes = computed<Attribute[]>(() => store.state.attributeModule.attributes) ;
    let stateSelectedAttribute = computed<Attribute>(() => store.state.attributeModule.selectedAttribute) ;
    let hasUnsavedChanges = computed<boolean>(() => store.state.unsavedChangesFlagModule.hasUnsavedChanges) ;
    let hasAdminAccess = computed<boolean>(() => store.state.authenticationModule.hasAdminAccess) ;
    let selectAttributeItems = ref<SelectItem[]>([]);
    
    async function logOutAction(payload?: any): Promise<any> {await store.dispatch('logOut', payload);}
    async function getAttributes(payload?: any): Promise<any> {await store.dispatch('getAttributes', payload);}
    async function getExcelSpreadsheetColumnHeadersAction(payload?: any): Promise<any> {await store.dispatch('getExcelSpreadsheetColumnHeaders', payload);}
    function selectAttributeAction(payload?: any) { store.dispatch('selectAttribute', payload);}
    async function upsertAttributeAction(payload?: any): Promise<any> {await store.dispatch('upsertAttribute', payload);}
    function setHasUnsavedChangesAction(payload?: any) { store.dispatch('setHasUnsavedChanges', payload);}
    async function getUserNameByIdGetter(payload?: any): Promise<any> {await store.dispatch('getUserNameById', payload);}
    function addErrorNotificationAction(payload?: any) { store.dispatch('addErrorNotification', payload);}

    created()
    function created() {
        getAttributes();
    }

    onBeforeUnmount(() => beforeDestroy)
    function beforeDestroy() {
        setHasUnsavedChangesAction({ value: false });
    }

    watch(stateAttributes, () =>  {
            let temp = clone(stateAttributes.value)
            temp.forEach(_ => {
                selectAttributeItems.value.push({text: _.name, value: _.id})
            })
    })

    watch(selectAttributeItemValue, () =>  {
        selectAttributeAction(selectAttributeItemValue.value);
        hasSelectedAttribute.value = true;
    })
    
    watch(selectedAttribute, () =>  {
        const hasUnsavedChanges: boolean = hasUnsavedChangesCore('', selectedAttribute.value, stateSelectedAttribute.value);
        setHasUnsavedChangesAction({ value: hasUnsavedChanges });        
    })

    watch(stateSelectedAttribute, () =>  {       
        selectedAttribute.value = clone(stateSelectedAttribute.value);      
    })

    function onSetSelectedAttributeProperty(property: string, value: any){
        selectedAttribute.value = setItemPropertyValue(
            property,
            value,
            selectedAttribute.value,
        );
    }

    function addAttribute()
    {
        selectAttributeItemValue.value = ''
    }

    function onDiscardChanges() {
        selectedAttribute.value = clone(stateSelectedAttribute.value);
    }

    async function saveAttribute() {
        let isInsert = false;
        let selectAttr = clone(selectedAttribute.value);
        if (selectedAttribute.value.id === getBlankGuid()) {
            selectAttr.id = getNewGuid();
            isInsert = true;
        }

        const createAttributeRequest = {
            attribute: selectAttr
        };

        await upsertAttributeAction(createAttributeRequest);
            
        if (!isNil(stateAttributes.value.find(_ => _.id === selectAttr.id))) {
            selectedAttribute.value.id = selectAttr.id;
            if (isInsert) selectAttributeItemValue.value = selectedAttribute.value.id;
        }
    }

    function disableCrudButtons() {
        let allValid = rules['generalRules'].valueIsNotEmpty(selectedAttribute.value.name) === true
            && rules['generalRules'].valueIsNotEmpty(selectedAttribute.value.type) === true            
            && rules['generalRules'].valueIsNotEmpty(selectedAttribute.value.defaultValue) === true
            && rules['generalRules'].valueIsNotEmpty(selectedAttribute.value.isCalculated) === true
            && rules['generalRules'].valueIsNotEmpty(selectedAttribute.value.isAscending) === true
            if(selectedAttribute.value.type === 'NUMBER'){
                if(isNaN(+selectedAttribute.value.defaultValue)){
                    allValid = false;
                    selectedAttribute.value.defaultValue = '';
                }
            }
            //when the parameter and ui sync an empty string is assigned to the parameter instead of null if the text box is empty
            if(isNil(selectedAttribute.value.maximum))  
                 selectedAttribute.value.maximum = null;
            if(isNil(selectedAttribute.value.minimum))  
                 selectedAttribute.value.minimum = null;

        return !allValid;
    }

</script>

<style>
.sharing {
    padding-top: 0 !important;
}
.sharing .v-input__slot{
    top: -10px !important;
}

.sharing .v-label{
    margin-bottom: 0;
    padding-top: 0;
}
</style>