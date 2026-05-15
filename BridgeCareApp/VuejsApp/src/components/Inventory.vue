<template> 
    <div style="width: 300px; margin: 0 auto;">
        <v-autocomplete
            v-model="inventoryReportName" 
            :items="adminDataStore.getInventoryReportNames"
            :label="`Select an Inventory Report`"
            variant="outlined"
            density="compact"
            class="ghd-select ghd-text-field ghd-text-field-border">
        </v-autocomplete>
    </div>
    <v-layout>
        <v-row>
            <v-row justify="space-between"></v-row>
            <v-row justify="space-between">
                <v-row style="display: flex; align-items: center; justify-content: center">
                    <div class="flex xs4" v-for="(key, index) in inventoryDetails">
                        <v-autocomplete
                        style="margin-top: 25px; width: 250px; margin-right: 20px"
                        class="ghd-select ghd-text-field ghd-text-field-border ghd-button-text"
                        :items="reactiveData[index]"
                        v-model="selectedInventoryIndex[index]"
                        :label="`Select by ${key}`"
                        :disabled="isDisabled(index)"
                        variant="outlined"
                        density="compact"
                        ></v-autocomplete>
                    </div>
                </v-row>
                <v-spacer></v-spacer>
                    <v-btn style="margin-right: 75px; margin-top: 40px" class='ghd-blue ghd-button-text ghd-outline-button-padding ghd-button' 
                    variant ="outlined" 
                    @click="resetDropdowns()">
                    Reset Key Fields
                    </v-btn>
           </v-row>
            <v-divider></v-divider>
        </v-row>
    </v-layout>
    <div style="margin: auto; margin-bottom: 25px; display: flex; align-items: center; justify-content: center" v-html="inventoryStore.getStaticHTMLForInventory"></div>
</template>

<script lang="ts" setup>
    import Vue from 'vue';
    import {QueryResponse, InventoryParam, emptyInventoryParam, InventoryItem, KeyProperty} from '@/shared/models/iAM/inventory';
    import {clone, empty, find, forEach, propEq} from 'ramda';
    import InventoryService from '@/services/inventory.service'
    import {inject, reactive, ref, onMounted, onBeforeUnmount, watch, Ref, computed} from 'vue';
    import { useRouter } from 'vue-router';
    import { coreAxiosInstance } from '@/shared/utils/axios-instance';
    import { useAdminDataStore, useInventoryStore } from '@/stores';

    const adminDataStore = useAdminDataStore();
    const inventoryStore = useInventoryStore();
    const emit = defineEmits(['submit'])
        
    
    const keyAttributeValues = ref<string[][]>([]);
    const reactiveData: Ref<[string[], string[]]> = ref([[], []]);
    let inventoryItem: any[][] = [];

    let selectedKeys: string[] = [];

    const selectedInventoryIndex = ref([]);

    let htmlResponse = ref();

    let queryLength: number;
    let reactValue: any[] = [];

    const inventoryDetails = ref<string[]>([]);
    let constraintDetails: string = '';
    let lastThreeLetters: string = '';
    let reportType: string = '';

    let queryValue: any;
    let querySelectedData: string[] = [];

    let inventorySelectListsWorker: any = null;

    let inventoryData: any  = null;
    let sanitizedHTML: any = null;
    let inventoryReportName = ref<string>('');

    const beforeRouteLeave = () => {
    // Reset staticHTMLForInventory when leaving the route
    inventoryStore.staticHTMLForInventory = null; // Set to null or an initial value
    }


        /**
         * Calls the setInventorySelectLists function to set both inventory type select lists
         */
        watch(() => inventoryStore.getInventoryItems,async ()=>{
            keyAttributeValues.value = await setupSelectLists();
        });

        watch(selectedInventoryIndex, (newValues: any, oldValues) => {
            let index: number;
            if(reactValue[0] != newValues[0] && newValues[0] != undefined){
                reactValue[0] = newValues[0];
                index = 0;
            }
            else
            {
                reactValue[1] = newValues[1];
                index = 1;
            }
            onSelectInventoryItem(index);
        }, { deep: true });

        watch(() => inventoryStore.getInventoryItems, ()=>{
            inventoryStore.getInventoryItems.forEach(element => {
                keyAttributeValues.value.push(element.keyProperties)
            });

            for(let i = 0; i < keyAttributeValues.value.length; i++)
            {
                let j = 0;
                reactiveData.value[0].push(keyAttributeValues.value[i][j])
            }

            for(let i = 0; i < keyAttributeValues.value.length; i++)
            {
                let j = 1;
                reactiveData.value[1].push(keyAttributeValues.value[i][j])
            }
        })

          watch(() => inventoryStore.getStaticHTMLForInventory,()=>{
           // sanitizedHTML = $sanitize(staticHTMLForInventory.value);
        })

        watch(() => adminDataStore.getInventoryReportNames,()=>{
            if(adminDataStore.getInventoryReportNames.length > 0)
                inventoryReportName.value = adminDataStore.getInventoryReportNames[0]
            
            lastThreeLetters = inventoryReportName.value.slice(-3);
            reportType = lastThreeLetters[1];
        });

        watch(inventoryReportName,()=>{
            if(selectedInventoryIndex.value.length === 3)
            {
                HandleSelectedItems(1)
            }
        })
        
/*          watch(stateKeyFields,()=>{
            if(reportType === 'P') {
                inventoryDetails.value = clone(stateKeyFields.value);
                inventoryDetails.value.forEach(_ => selectedKeys.push(""));

                inventoryStore.getInventory(inventoryDetails.value);
            }
        });
 */ 
        watch(() => adminDataStore.getRawDataKeyFields,()=>{
            inventoryDetails.value = clone(adminDataStore.getRawDataKeyFields);

                inventoryDetails.value.forEach(_ => selectedKeys.push(""));
            if(!adminDataStore.getInventoryReportNames.some(_ => _.includes("(R)")))
                {
                        inventoryStore.getInventory(inventoryDetails.value);
                }
                else
                {
                        inventoryStore.getInventory([inventoryDetails.value[0]]);
                }
        });

        watch(() => adminDataStore.getConstraintType,()=>{
            constraintDetails = adminDataStore.getConstraintType;
        });

        watch(() => inventoryStore.getQuerySet,()=>{
            reactiveData.value = [[],[]];
                for(let i = 0; i < inventoryDetails.value.length; i++) 
                {
                    queryValue = inventoryStore.getQuerySet[i].values;
                    queryValue.sort((a: string | number, b: string | number) => 
                    {
                        if (typeof a === "number" && typeof b === "number") {
                            return a - b; // Sort numbers in descending order
                        } else if (typeof a === "string" && typeof b === "string") {
                            return a.localeCompare(b); // Sort strings in alphabetical order
                        } else {
                            return 0; // Preserve the original order if the data types are different
                        }
                    });
                    reactiveData.value[i] = queryValue;
                }
        });
        /**
         * Vue component has been mounted
         */
        onMounted(() => {
            (async () => { 
                await adminDataStore.getConstraintType();
                await adminDataStore.getInventoryReports();
                await adminDataStore.getKeyFields(); 
                await adminDataStore.getRawDataKeyFields();
                await adminDataStore.getConstraintType();
            })();
        })

        onBeforeUnmount(() => {
            const router = useRouter();
            router.beforeEach(beforeRouteLeave);
        });

        created();
        function created() {
            initializeLists();
        }

        async function setupSelectLists() {
            querySelectedData = [];
            const data: any = {
                inventoryItems: inventoryStore.getInventoryItems,
                inventoryDetails: inventoryDetails.value
            };
            let toReturn: string[][] = [];
            var keyProperties = data.inventoryDetails;
             const response = await InventoryService.getInventory(keyProperties);
               keyAttributeValues.value = response.data;
               response.data.forEach((element: string[]) => {
                    toReturn.push(element);
                });

            return toReturn; 
                  
        }
        
        async function initializeLists() {
        }

        async function resetDropdowns() {
            selectedInventoryIndex.value = [];
            reactiveData.value = [[],[]];
            //Reset selected key fields
            selectedInventoryIndex.value.forEach((value: string, index: number, array: string[]) => {
                selectedKeys[index] = '';
            })
            selectedInventoryIndex.value = [];
            inventoryStore.staticHTMLForInventory = null;
            selectedKeys[0] = "";
            keyAttributeValues.value = await setupSelectLists();

        }

        function onSelectInventoryItem(index: number){
            constraintDetails = adminDataStore.getConstraintType;
            if(constraintDetails == 'OR')
            {
                HandleSelectedItems(index);
            }
            else if(constraintDetails == 'AND') {
                let selectedCounter = 0;
                //Get the first user selected key field and it's selection and put it in a dictionary
                QueryAccess();

                //Check if any dropdowns are empty
                for(let i = 0; i < inventoryDetails.value.length; i++) {
                    if(selectedInventoryIndex.value[i]) {
                        selectedCounter++;
                    }
                }
                
                if(selectedCounter === inventoryDetails.value.length)
                {
                    HandleSelectedItems(index);
                 }
            }       
        }

         function HandleSelectedItems(index: number) {
            selectedKeys = selectedInventoryIndex.value;
            let key = selectedInventoryIndex.value[index];
                let data: InventoryParam = {
                keyProperties: {},
                values: function (values: any): unknown {
                throw new Error('Function not implemented.');
                }
                };

            if(constraintDetails == "OR")
            {
                for(let i = 0; i < inventoryDetails.value.length; i++){
                    if(i === index){
                        data.keyProperties[i] = key;
                        continue;
                    }
                    let inventoryItem = inventoryStore.getInventoryItems.filter(function(item: { keyProperties: string | any[]; }){if(item.keyProperties.indexOf(key) !== -1) return item;})[0]; 
                    let otherKeyValue = inventoryItem.keyProperties[i]; 
                    selectedKeys[i] = otherKeyValue;
                    data.keyProperties[i] = otherKeyValue;
                }
            }

                 //Create a dictionary of the selected key fields
                let dictionary: Record<string, string> = {};

                for(let i = 0; i < inventoryDetails.value.length; i++) {
                    let dictNames: any = inventoryDetails.value[i];
                    let dictValues: any = selectedKeys[i];
                    dictionary[dictNames] = dictValues;                     
                }

                //Set the data equal to the dictionary
                data.keyProperties = dictionary;

                if(selectedKeys.length === inventoryDetails.value.length)
                {
                    inventoryStore.getStaticInventoryHTML(inventoryReportName.value, data.keyProperties as any); 
                }
        }

        function QueryAccess() {
             //Get the first user selected key field and it's selection and put it in a dictionary

            if(querySelectedData.length === 0 || querySelectedData.length === 2) 
            {
                for(let i = 0; i < selectedInventoryIndex.value.length; i++) 
                    {
                        if(selectedInventoryIndex.value[i] !== '') 
                        {
                            if(!querySelectedData.includes(inventoryDetails.value[i]) && !querySelectedData.includes(selectedInventoryIndex.value[i]))
                            {
                                querySelectedData.push(inventoryDetails.value[i]);
                                querySelectedData.push(selectedInventoryIndex.value[i]);
                            }
                        }
                    }
                    //Send to back end to recieve dropdown lists for the other key fields                     
                    inventoryStore.getQuery(querySelectedData as any);          
            }
        }

        function isDisabled(index: number) {
            if(querySelectedData.length < index * 2 && constraintDetails === "AND") {
                return true;
            }
            return false;
        }
</script>

<style>
    @import "../assets/css/inventory.css"
</style>
