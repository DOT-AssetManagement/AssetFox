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
    <v-row align-center class="vl-style">
      <v-col cols="12">
        <v-row column>
          <v-subheader
            class="ghd-control-label ghd-md-gray Montserrat-font-family"
            >Agency Logo
          </v-subheader>
        </v-row>
      </v-col>
      <v-col cols="12">
        <v-row column style="padding-left: 330px; margin-top: -35px;">
          <input
            id="agencyImageUpload"
            type="file"
            ref="agencyFileInput"
            :accept="agencyLogoAccept"
            @change="handleAgencyLogoUpload"
            hidden
          />
          <v-btn
            class="ghd-blue ghd-button-text ghd-outline-button-padding ghd-button"
            style="margin-right: 40%"
            @click="onUploadAgencyLogo"
            variant="outlined"
            >Upload</v-btn
          >
        </v-row>
      </v-col>
    </v-row>
    <v-row align-center class="vl-style">
      <v-col cols="12">
        <v-row column>
          <v-subheader
            class="ghd-control-label ghd-md-gray Montserrat-font-family"
            >Implementation Logo
          </v-subheader>
        </v-row>
      </v-col>
      <v-col cols="12">
        <v-row column style="padding-left: 330px; margin-top: -35px;">
          <input
            id="implementationImageUpload"
            type="file"
            ref="implementationFileInput"
            :accept="implementationLogoAccept"
            @change="handleImplementationLogoUpload"
            hidden
          />
          <v-btn
            class="ghd-blue ghd-button-text ghd-outline-button-padding ghd-button"
            style="margin-right: 40%"
            @click="onUploadImplementationLogo"
            variant = "outlined"
            >Upload</v-btn
          >
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
import { computed, ref } from 'vue';
import { useAdminSiteSettingsStore, useConfigStore } from '@/stores';

const adminSiteSettingsStore = useAdminSiteSettingsStore() ;
const configStore = useConfigStore();
const agencyFileInput = ref<HTMLInputElement | null>(null);
const implementationFileInput = ref<HTMLInputElement | null>(null);
let ImplementationID = ref('');
let adminContactEmail = ref('');

const agencyLogoAccept = computed(() => getConfiguredExtension(configStore.config?.logos?.agencyFilename));
const implementationLogoAccept = computed(() => getConfiguredExtension(configStore.config?.logos?.implementationFilename));

function getConfiguredExtension(filename?: string): string {
  if (!filename || !filename.includes('.')) {
    return 'image/*';
  }

  return `.${filename.split('.').pop()}`;
}

function onSaveAdminContactEmail() {
  adminSiteSettingsStore.setAdminContactEmail(adminContactEmail.value);
}
  
function onSaveImplementationName() {
  adminSiteSettingsStore.importImplementationName(ImplementationID.value);
}

function onUploadImplementationLogo() {
  implementationFileInput.value?.click();
}

async function handleImplementationLogoUpload(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) {
    return;
  }

  try {
    await adminSiteSettingsStore.importProductLogo(file);
  } finally {
    input.value = '';
  }
}

function onUploadAgencyLogo() {
  agencyFileInput.value?.click();
}

async function handleAgencyLogoUpload(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) {
    return;
  }

  try {
    await adminSiteSettingsStore.importAgencyLogo(file);
  } finally {
    input.value = '';
  }
}

</script>
