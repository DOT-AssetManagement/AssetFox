<template>
    <v-dialog v-model="dialog" persistent max-width="600px">
      <v-card>
        <v-card-title>
          <span class="text-h5">Select Treatments</span>
        </v-card-title>
        <v-card-text>
          <v-container>
            <v-row>
              <v-col cols="12">
                <v-text-field
                  v-model="searchTerm"
                  label="Search Treatments"
                  prepend-inner-icon="mdi-magnify"
                  variant="outlined"
                  hide-details
                  clearable
                  density="compact"
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row style="max-height: 400px; overflow-y: auto;">
              <v-col cols="12">
                <v-checkbox
                  v-for="treatment in filteredTreatments"
                  :key="treatment"
                  v-model="selectedTreatments"
                  :label="treatment"
                  :value="treatment"
                  hide-details
                  density="compact"
                ></v-checkbox>
                 <p v-if="filteredTreatments.length === 0">No treatments found.</p>
              </v-col>
            </v-row>
          </v-container>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="blue-darken-1" variant="text" @click="cancel">
            Cancel
          </v-btn>
          <v-btn color="blue-darken-1" variant="text" @click="save">
            Save
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </template>
  
  <script setup lang="ts">
  import { ref, watch, computed, toRefs } from 'vue';
  
  const props = defineProps<{
    modelValue: boolean; // Controls dialog visibility
    availableTreatments: string[]; // List of all possible treatments
    initialSelectedTreatments: string[]; // Treatments currently selected for the project
  }>();
  
  const emit = defineEmits(['update:modelValue', 'save']);
  
  // Use toRefs to keep reactivity with props
  const { modelValue, availableTreatments, initialSelectedTreatments } = toRefs(props);
  
  const dialog = ref(false);
  const selectedTreatments = ref<string[]>([]);
  const searchTerm = ref('');
  
  // Watch the modelValue prop to control the dialog visibility internally
  watch(modelValue, (newValue) => {
    dialog.value = newValue;
    if (newValue) {
      // Reset selected treatments when dialog opens
      selectedTreatments.value = [...initialSelectedTreatments.value];
      searchTerm.value = ''; // Clear search on open
    }
  });
  
  // Filter treatments based on search term
  const filteredTreatments = computed(() => {
    if (!searchTerm.value) {
      return availableTreatments.value;
    }
    return availableTreatments.value.filter(treatment =>
      treatment.toLowerCase().includes(searchTerm.value.toLowerCase())
    );
  });
  
  function cancel() {
    dialog.value = false;
    emit('update:modelValue', false);
  }
  
  function save() {
    emit('save', selectedTreatments.value);
    dialog.value = false;
    emit('update:modelValue', false);
  }
  </script>