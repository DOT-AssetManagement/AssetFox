import { defineStore } from 'pinia';

export interface UnsavedChangesStoreState {
    hasUnsavedChanges: boolean;
}

export const useUnsavedChangesStore = defineStore('unsavedChanges', {
    state: (): UnsavedChangesStoreState => ({
        hasUnsavedChanges: false,
    }),
    actions: {
        setHasUnsavedChanges(value: boolean) {
            this.hasUnsavedChanges = value;
        },
    },
    getters: {
        getHasUnsavedChanges: (state) => state.hasUnsavedChanges
    }
});
