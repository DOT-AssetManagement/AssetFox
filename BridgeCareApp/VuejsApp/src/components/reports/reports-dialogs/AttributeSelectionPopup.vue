<template>
    <v-dialog v-model="dialog" persistent max-width="600px">
      <v-card>
        <v-card-title>
          <span class="text-h5">Select Attributes</span>
        </v-card-title>
        <v-card-text>
          <v-container>
            <v-row>
              <v-col cols="12">
                <v-text-field
                  v-model="searchTerm"
                  label="Search Attributes"
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
                  v-for="Attribute in filteredAttributes"
                  :key="Attribute"
                  v-model="selectedAttributes"
                  :label="Attribute"
                  :value="Attribute"
                  hide-details
                  density="compact"
                ></v-checkbox>
                 <p v-if="filteredAttributes.length === 0">No Attributes found.</p>
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
    availableAttributes: string[]; // List of all possible Attributes
    initialSelectedAttributes: string[]; // Attributes currently selected for the project
  }>();
  
  const emit = defineEmits(['update:modelValue', 'save']);
  
  // Use toRefs to keep reactivity with props
  const { modelValue, availableAttributes, initialSelectedAttributes } = toRefs(props);
  
  const dialog = ref(false);
  const selectedAttributes = ref<string[]>([]);
  const searchTerm = ref('');
  
  // Watch the modelValue prop to control the dialog visibility internally
  watch(modelValue, (newValue) => {
    dialog.value = newValue;
    if (newValue) {
      // Reset selected Attributes when dialog opens
      selectedAttributes.value = [...initialSelectedAttributes.value];
      searchTerm.value = ''; // Clear search on open
    }
  });
  
  // Filter Attributes based on search term
  const filteredAttributes = computed(() => {
    if (!searchTerm.value) {
      return availableAttributes.value;
    }
    return availableAttributes.value.filter(Attribute =>
      Attribute.toLowerCase().includes(searchTerm.value.toLowerCase())
    );
  });
  
  function cancel() {
    dialog.value = false;
    emit('update:modelValue', false);
  }
  
  function save() {
    emit('save', selectedAttributes.value);
    dialog.value = false;
    emit('update:modelValue', false);
  }
  </script>