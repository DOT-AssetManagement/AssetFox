<template>
    <v-btn
      class="ghd-white-bg ghd-blue ghd-button-text ghd-button"
      @click="onDownloadSimulationLog"
      variant="flat"
    >
      Simulation Log
    </v-btn>
  </template>
  
  <script setup lang="ts">
  import { useNotificationStore } from '@/stores';
  import { downloadSimulationLog } from '@/shared/utils/simulation-log-utils';
  
  const props = defineProps({
    networkId: {
      type: String,
      required: true,
    },
    selectedScenarioId: {
      type: String,
      required: true,
    },
    simulationName: {
      type: String,
      required: true,
    },
  });
  
  const emit = defineEmits(['success', 'error']);
  const notificationStore = useNotificationStore();
  
  async function onDownloadSimulationLog() {
    await downloadSimulationLog(
      props.networkId,
      props.selectedScenarioId,
      props.simulationName,
      notificationStore.addSuccessNotification,
      notificationStore.addErrorNotification,
    );
}
  </script>
  
  <style scoped>
  /* Include any specific styles here */
  </style>
  