<template>
  <v-row
    column
    class="Montserrat-font-family ma-0"
    style="width: 25%; padding-left: 50px"
  >
  <v-col></v-col>
    <v-row align-center class="vl-style">
      <v-col cols="11">
        <v-row column>
          <v-subheader
            class="ghd-control-label ghd-md-gray Montserrat-font-family"
            >Implementation Name</v-subheader
          >
        </v-row>
      </v-col>
      <v-col cols="8">
        <v-row column style="padding-right: 100px">
          <v-text-field
            variant="outlined"
            id="AdminSiteSettingsEditor-EditImplementationName-textfield"
            v-model="ImplementationID"
            type="text"
            hide-details
            clearable
            single-line           
            style="padding-left: 0px; width: 250%"
            density="compact"
          >
          </v-text-field>
          <v-col >
        <v-row column style="padding-left: 300px; margin-top: -46px; margin-left: 25px;">
          <v-btn
            id="AdminSiteSettingsEditor-SaveImplementationName-btn"
            class="ghd-blue ghd-button-text ghd-outline-button-padding ghd-button"
            style="padding-left: 10px"
            @click="onSaveImplementationName"
            variant = "outlined"
            >Save</v-btn
          >
        </v-row>
      </v-col>
        </v-row>
      </v-col>
      
    </v-row>
    <v-row align-center class="vl-style" style="margin-top: 5%">
    <v-col cols="12">
      <v-row column>
        <v-subheader
          class="ghd-control-label ghd-md-gray Montserrat-font-family"
          >Administrator Contact Email
        </v-subheader>
      </v-row>
    </v-col>
    <v-col cols="10">
      <v-row column style="padding-right: 100px">
        <v-text-field
          variant="outlined"
          id="AdminSiteSettingsEditor-AdminContactEmail-textfield"
          v-model="adminContactEmail"
          type="email"
          hide-details
          clearable
          single-line
          style="padding-left: 0px; width: 250%"
          density="compact"
        ></v-text-field>
      </v-row>
      <v-row column style="padding-left: 300px; margin-top: -40px; margin-left: 25px;">
        <v-btn
          id="AdminSiteSettingsEditor-SaveAdminContactEmail-btn"
          class="ghd-blue ghd-button-text ghd-outline-button-padding ghd-button"
          @click="onSaveAdminContactEmail"
          variant="outlined"
        >Save</v-btn>
      </v-row>
    </v-col>
  </v-row>
</v-row>
</template>
<script lang="ts" setup>
import { ref } from 'vue';
import { useAdminSiteSettingsStore } from '@/stores';

const adminSiteSettingsStore = useAdminSiteSettingsStore() ;
let ImplementationID = ref('');
let adminContactEmail = ref('');

function onSaveAdminContactEmail() {
  adminSiteSettingsStore.setAdminContactEmail(adminContactEmail.value);
}
  
function onSaveImplementationName() {
  adminSiteSettingsStore.importImplementationName(ImplementationID.value);
}

</script>
