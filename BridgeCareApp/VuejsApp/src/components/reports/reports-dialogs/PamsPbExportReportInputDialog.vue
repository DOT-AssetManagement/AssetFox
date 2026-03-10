<template>
    <v-dialog width="40%" height="30%" persistent v-model="showDialog">
        <v-card style="height: 98%;">
            <v-card-title class="ghd-dialog-padding-top-title">
                <v-row justify="space-between">
                    <v-col cols="12">
                        <div class="ghd-control-dialog-header">
                            <h5>Select PAMS PBExport Report inputs <XButton class="float-right" @click="onSubmit(false)"/></h5>
                        </div>
                    </v-col>
                </v-row>
            </v-card-title>
            <v-card-text class="ghd-dialog-box-padding-center">    
                <v-row>                 
                    <v-col class="mb-n3">
                        <v-subheader class="ghd-control-text">Select below option to view in the report:</v-subheader>                    
                    </v-col>   
                </v-row>            
                <v-row style="height: 40px;">
                    <v-col cols = "5">
                        <v-switch
                        id="PamsPbExportReportInputDialog-displayMas-cb"
                        class="ghd-checkbox"
                        color="#2A578D"
                        label="Maintainable Assets tab"
                        v-model="newPamsPbExportReportRequestModel.displayMasTab"
                        @update:model-value='onSetDisplayProperty("displayMasTab",$event)'/>
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
        </v-card>        
    </v-dialog>    
</template>

<script setup lang="ts">    
    import { toRefs, ref, computed, onMounted, watch } from 'vue';
    import { setItemPropertyValue } from '@/shared/utils/setter-utils';
    import { emptyPamsPbExportReportRequestModel, PamsPbExportReportRequestModel } from '@/shared/models/iAM/reports';
    import SubmitButton from '@/shared/components/buttons/SubmitButton.vue';
    import CancelButton from '@/shared/components/buttons/CancelButton.vue';
    import XButton from '@/shared/components/buttons/XButton.vue';
    import { useRouter } from 'vue-router'; 
    const router = useRouter();
    const props = defineProps({
        showDialog: Boolean
    })
    const { showDialog } = toRefs(props);
    const emit = defineEmits(['submit']);
    const selectedScenarioId = ref('');
    
    let newPamsPbExportReportRequestModel = ref<PamsPbExportReportRequestModel>({...emptyPamsPbExportReportRequestModel});            
    
    watch(showDialog, async (isNowVisible) => {
        if (isNowVisible) {
            selectedScenarioId.value = router.currentRoute.value.query.scenarioId as string;        
        }
    });

    function disableSubmitButton() {
        return false;
    }

    function onSubmit(submit: boolean) {
        if (submit) {
        emit('submit', newPamsPbExportReportRequestModel.value);
        } else {
        emit('submit', null);
        }

        newPamsPbExportReportRequestModel.value = {
            ...emptyPamsPbExportReportRequestModel
        };
    }

    function onSetDisplayProperty(property: string, value: any) {
        newPamsPbExportReportRequestModel.value = setItemPropertyValue(
            property,
            value,
            newPamsPbExportReportRequestModel.value,
        );
    }    
</script>