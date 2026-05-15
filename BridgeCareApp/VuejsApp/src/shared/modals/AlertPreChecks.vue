<template>
    <v-row>
        <v-dialog style="overflow-y: auto" max-width='800px' persistent scrollable v-model="showDialogComputed">
            <v-card>
                <v-card-title class="ghd-dialog-box-padding-top">
                    <v-row justify-space-between align-center>
                        <div style="margin-left: 250px" class="ghd-control-dialog-header">Interactive pre-checks</div>
                        <v-btn style='margin-left:241px; font-size: 17px' @click="onSubmit(false)" variant = "flat">
                            X
                        </v-btn>
                    </v-row>
                </v-card-title>
                <div style='height: 100%; max-width:100%' class="ghd-dialog-box-padding-center">                    
                    <div style='max-height: 450px; border:1px solid black; overflow-y:auto;'>
                        <div v-if=dialogDataPreChecks.ErrorHeading != ''>                            
                            <v-card-text class="px-4" style="margin-top: -10px;">
                                <div style="font-weight: 500; margin-top: 0px; margin-bottom: 5px;">
                                    {{dialogDataPreChecks.ErrorHeading}}
                                </div>
                                <ul>
                                    <li class="text--primary" v-for="(key, index) in filteredErrorMessages" :key="index">
                                        {{ key }}                                
                                    </li>
                                </ul>
                            </v-card-text>
                        </div>
                        <div v-if=dialogDataPreChecks.WarningHeading != ''>                            
                            <v-card-text class="px-4" style="margin-top: -10px;">
                                <div style="font-weight: 500; margin-top: 0px; margin-bottom: 5px;">
                                    {{dialogDataPreChecks.WarningHeading}}
                                </div>
                                <ul>
                                    <li class="text--primary" v-for="(key, index) in filteredWarningMessages" :key="index">
                                        {{ key }}                                
                                    </li>
                                </ul>
                            </v-card-text>
                        </div>
                        <div v-if=dialogDataPreChecks.SuccessHeading != ''>                            
                            <v-card-text class="px-4" style="margin-top: -10px;">
                                <div style="font-weight: 500; margin-top: 0px; margin-bottom: 5px;">
                                    {{dialogDataPreChecks.SuccessHeading}}
                                </div>
                                <ul>
                                    <li class="text--primary" v-for="(key, index) in filteredSuccessMessages" :key="index">
                                        {{ key }}                                
                                    </li>
                                </ul>
                            </v-card-text>
                        </div>
                    </div>
                </div>
                <v-card-actions>
                    <v-row style="margin-left:20px" justify-center row v-if="dialogDataPreChecks.choice">
                        <v-btn 
                        id="Alert-Cancel-vbtn"
                        @click="onSubmit(false)" 
                        class="ghd-blue ghd-button" variant = "outlined">
                            Cancel
                        </v-btn>
                        <v-btn 
                        id="Alert-Proceed-vbtn"
                        @click="onSubmit(true)" 
                        class="ghd-blue-bg ghd-white ghd-button">
                            Proceed
                        </v-btn>
                    </v-row>
                    <v-row v-if="!dialogDataPreChecks.choice" style="padding-left: 35px">                        
                        <v-btn @click="onSubmit(false)" class="ghd-blue-bg ghd-white ghd-button">
                            OK
                        </v-btn>
                        <v-btn @click="onDownloadPreChecksLog" class="ghd-blue-bg ghd-white ghd-button">
                            Download Log
                        </v-btn>
                    </v-row>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-row>
</template>

<script lang="ts" setup>
    import Vue, { computed } from 'vue';
    import {AlertPreChecksData, PreChecksValidationLog, emptyPreChecksValidationLog } from '../models/modals/alert-data';
    import {inject, reactive, ref, onMounted, onBeforeUnmount, watch, Ref} from 'vue';
    import { useStore } from 'vuex';
    import { useRouter } from 'vue-router';
    import ScenarioService from '@/services/scenario.service';
    import FileDownload from 'js-file-download';
    import { AxiosResponse } from 'axios';
    import { hasValue } from '@/shared/utils/has-value-util';
    import { prop, clone } from 'ramda';
    import { useNotificationStore } from '@/stores';
    
    const notificationStore = useNotificationStore();

    const props = defineProps<{
        dialogDataPreChecks: AlertPreChecksData,
        scenario: string
    }>()
    let showDialogComputed = computed(() => props.dialogDataPreChecks.showDialog);
    const emit = defineEmits(['submit'])
    let preChecksValidationLog : PreChecksValidationLog = clone(emptyPreChecksValidationLog);
    ;

    const filteredErrorMessages = computed(() => {
        const message = props.dialogDataPreChecks.ErrorMessage;

        if (props.dialogDataPreChecks.ErrorMessage != '') {
            // Split the message by periods and filter out empty strings
            const sentences = message.split('.');
            
            var filtered = sentences.filter(function (el) {
                return el != null && el != '';
            });

            return filtered;
        }
        return [];
    });

    const filteredWarningMessages = computed(() => {
        const message = props.dialogDataPreChecks.WarningMessage;

        if (props.dialogDataPreChecks.WarningMessage != '') {
            // Split the message by periods and filter out empty strings
            const sentences = message.split('.');
            
            var filtered = sentences.filter(function (el) {
                return el != null && el != '';
            });

            return filtered;
        }
        return [];
    });

    const filteredSuccessMessages = computed(() => {
        const message = props.dialogDataPreChecks.SuccessMessage;

        if (props.dialogDataPreChecks.SuccessMessage != '') {
            // Split the message by periods and filter out empty strings
            const sentences = message.split('.');
            
            var filtered = sentences.filter(function (el) {
                return el != null && el != '';
            });

            return filtered;
        }
        return [];
    });

    const filteredLog = computed(()=> {
            preChecksValidationLog.ErrorMessages = filteredErrorMessages.value;
            preChecksValidationLog.WarningMessages = filteredWarningMessages.value;
            return preChecksValidationLog;
        }
    );

    /**
     * Emits a boolean result to the parent component
     * @param submit
     */
    function onSubmit(submit: boolean) {
        emit('submit', submit);
    }

    async function onDownloadPreChecksLog() {
        try {            
            const response: AxiosResponse<any> = await ScenarioService.getPreChecksDownloadLog(filteredLog.value);
        
            if (hasValue(response, 'data')) {                
                notificationStore.addSuccessNotification({ message: 'Pre-Checks Log downloaded' });
                FileDownload(response.data, `Pre-Checks Log ${props.scenario}.txt`);
            } else {
                throw new Error('No data received');
            }
            onSubmit(false);
        } catch (error) {
            console.error('Error downloading Pre-Checks Log:', error);
            notificationStore.addErrorNotification({
                message: 'Failed to download Pre-Checks Log',
                longMessage:
                    'Failed to download Pre-Checks Log. Please try again',
            });
        }
    }
</script>
