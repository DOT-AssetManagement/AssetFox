<template>
    <v-row>
        <v-dialog content content-class="centered-dialog" persistent v-model="showDialogComputed">
            <v-container>
                <v-row   justify="center">
                    <v-col align="center">
                        <v-progress-circular :size="70" :width="7" class="assetFox-blue-pantone-281" indeterminate></v-progress-circular>
                        <h1>Please wait</h1>
                    </v-col>
                    
                </v-row>
            </v-container>
        </v-dialog>
    </v-row>
</template>

<script lang="ts" setup>
import { computed } from 'vue';
import { useBusyStore } from '@/stores';

const busyStore = useBusyStore();
const showDialogComputed = computed<boolean>(() => busyStore.getProcessCounter > 0);

</script>

<style>
    .dialog.centered-dialog, .v-dialog.centered-dialog {
        background: grey;
        box-shadow: none;
        border-radius: 6px;
        width: auto;
        color: whitesmoke;
    }
</style>
