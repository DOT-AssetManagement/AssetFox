<template>
    <v-dialog v-model="dialog" persistent max-width="600px">
      <v-card>
        <v-card-title>
          <span class="text-h5">Select Years</span>
        </v-card-title>
        <v-card-text>
          <v-container>
            <v-row>
              <v-col cols="12">
                <v-text-field
                  v-model="searchTerm"
                  label="Search Years"
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
                  v-for="Year in filteredYears"
                  :key="Year"
                  v-model="selectedYears"
                  :label="Year"
                  :value="Year"
                  hide-details
                  density="compact"
                ></v-checkbox>
                 <p v-if="filteredYears == undefined || filteredYears.length === 0">No Years found.</p>
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
    availableYears: number[]; // List of all possible Years
    initialSelectedYears: number[]; // Years currently selected for the project
  }>();
  
  const emit = defineEmits(['update:modelValue', 'save']);
  
  // Use toRefs to keep reactivity with props
  const { modelValue, availableYears, initialSelectedYears } = toRefs(props);
  
  const dialog = ref(false);
  const selectedYears = ref<number[]>([]);
  const searchTerm = ref('');

  // Watch the modelValue prop to control the dialog visibility internally
  watch(modelValue, (newValue) => {
    dialog.value = newValue;
    if (newValue) {
      // Reset selected Years when dialog opens
      selectedYears.value = [...initialSelectedYears.value];
    }
  });
  
  // Filter Years based on search term
  const filteredYears = computed(() => {
      if (!searchTerm.value) {
      return availableYears.value;
    }
    return availableYears.value.filter(Year =>
      Year.toString().includes(searchTerm.value)
    );
  });
  
  function cancel() {
    dialog.value = false;
    emit('update:modelValue', false);
  }
  
  function save() {
    emit('save', selectedYears.value);
    dialog.value = false;
    emit('update:modelValue', false);
  }
  </script>