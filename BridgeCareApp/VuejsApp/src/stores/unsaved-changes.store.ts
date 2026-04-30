import { defineStore } from 'pinia';

export interface UnsavedChangesPayload {
    value: boolean;
}

export interface UnsavedChangesStoreState {
    hasUnsavedChanges: boolean;
}

export const useUnsavedChangesStore = defineStore('unsavedChanges', {
    state: (): UnsavedChangesStoreState => ({
        hasUnsavedChanges: false,
    }),
    actions: {
        setHasUnsavedChanges(payload: UnsavedChangesPayload | boolean) {
            this.hasUnsavedChanges =
                typeof payload === 'boolean' ? payload : payload.value;
        },
    },
});
