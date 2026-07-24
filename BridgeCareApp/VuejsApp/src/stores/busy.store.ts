import { defineStore } from 'pinia';

export interface BusyStoreState {
    processCounter: number;
}

export const useBusyStore = defineStore('busy', {
    state: (): BusyStoreState => ({
        processCounter: 0,
    }),
    getters: {
        getProcessCounter: (state) => state.processCounter,
    },
    actions: {
        incrementProcessCounter() {
            this.processCounter += 1;
        },
        decrementProcessCounter() {
            this.processCounter = Math.max(0, this.processCounter - 1);
        },
        setProcessCounter(count: number) {
            this.processCounter = count;
        },
    },
});
