import { defineStore } from 'pinia';

export interface AlertStoreState {
    alertMessage: string;
}

export const useAlertStore = defineStore('alert', {
    state: (): AlertStoreState => ({
        alertMessage: '',
    }),
    getters: {
        getAlertMessage: (state) => state.alertMessage,
    },
    actions: {
        setAlertMessage(message: string) {
            this.alertMessage = message;
        },
    },
});
