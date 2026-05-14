import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, propEq, reject, update } from 'ramda';
import CashFlowService from '@/services/cash-flow.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import {
    CashFlowRuleLibrary,
    emptyCashFlowRuleLibrary,
} from '@/shared/models/iAM/cash-flow';

export interface CashFlowStoreState {
    cashFlowRuleLibraries: CashFlowRuleLibrary[];
    selectedCashFlowRuleLibrary: CashFlowRuleLibrary;
    hasPermittedAccess: boolean;
    isSharedLibrary: boolean;
}

export const useCashFlowStore = defineStore('cashFlow', {
    state: (): CashFlowStoreState => ({
        cashFlowRuleLibraries: [],
        selectedCashFlowRuleLibrary: clone(emptyCashFlowRuleLibrary),
        hasPermittedAccess: false,
        isSharedLibrary: false,
    }),
    actions: {
        cashFlowRuleLibraryMutator(library: CashFlowRuleLibrary) {
            this.cashFlowRuleLibraries = any(
                propEq('id', library.id),
                this.cashFlowRuleLibraries,
            )
                ? (update(
                      findIndex(propEq('id', library.id), this.cashFlowRuleLibraries),
                      library,
                      this.cashFlowRuleLibraries,
                  ) as CashFlowRuleLibrary[])
                : (append(library, this.cashFlowRuleLibraries) as CashFlowRuleLibrary[]);
        },
        selectedCashFlowRuleLibraryMutator(libraryId: string) {
            this.selectedCashFlowRuleLibrary = any(
                propEq('id', libraryId),
                this.cashFlowRuleLibraries,
            )
                ? (find(
                      propEq('id', libraryId),
                      this.cashFlowRuleLibraries,
                  ) as CashFlowRuleLibrary)
                : clone(emptyCashFlowRuleLibrary);
        },
        selectCashFlowRuleLibrary(libraryId: string) {
            this.selectedCashFlowRuleLibraryMutator(libraryId);
        },
        async getCashFlowRuleLibraries() {
            await CashFlowService.getCashFlowRuleLibraries().then(
                (response: AxiosResponse<CashFlowRuleLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.cashFlowRuleLibraries = clone(response.data);
                    }
                },
            );
        },
        async deleteCashFlowRuleLibrary(libraryId: string) {
            const notificationStore = useNotificationStore();
            await CashFlowService.deleteCashFlowRuleLibrary(libraryId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.cashFlowRuleLibraries = reject(
                            propEq('id', libraryId),
                            this.cashFlowRuleLibraries,
                        ) as CashFlowRuleLibrary[];
                        notificationStore.addSuccessNotification({
                            message: 'Deleted cash flow rule library',
                        });
                    }
                },
            );
        },
        async getHasPermittedAccess() {
            await CashFlowService.getHasPermittedAccess().then(
                (response: AxiosResponse<boolean>) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.hasPermittedAccess = response.data as boolean;
                    }
                },
            );
        },
        async getIsSharedCashFlowRuleLibrary(id: string) {
            await CashFlowService.getIsSharedCashFlowRuleLibrary(id).then(
                (response: AxiosResponse<boolean>) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.isSharedLibrary = response.data as boolean;
                    }
                },
            );
        },
    },
    getters: {
        cashFlowRuleLibrariesGetter: (state) => state.cashFlowRuleLibraries,
        getSelectedCashFlowRuleLibrary: (state) => state.selectedCashFlowRuleLibrary,
        hasPermittedAccessGetter: (state) => state.hasPermittedAccess,
        getIsSharedLibrary: (state) => state.isSharedLibrary
    }
});
