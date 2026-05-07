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

export interface CashFlowRuleLibrarySelectionPayload {
    libraryId: string;
}

export interface SharedCashFlowLibraryPayload {
    id: string;
}

export interface CashFlowStoreState {
    cashFlowRuleLibraries: CashFlowRuleLibrary[];
    selectedCashFlowRuleLibrary: CashFlowRuleLibrary;
    hasPermittedAccess: boolean;
    isSharedLibrary: boolean;
}

function selectCashFlowRuleLibraryById(
    libraries: CashFlowRuleLibrary[],
    libraryId: string,
): CashFlowRuleLibrary {
    return any(propEq('id', libraryId), libraries)
        ? (find(propEq('id', libraryId), libraries) as CashFlowRuleLibrary)
        : clone(emptyCashFlowRuleLibrary);
}

function upsertCashFlowRuleLibrary(
    libraries: CashFlowRuleLibrary[],
    library: CashFlowRuleLibrary,
): CashFlowRuleLibrary[] {
    return any(propEq('id', library.id), libraries)
        ? (update(
              findIndex(propEq('id', library.id), libraries),
              library,
              libraries,
          ) as CashFlowRuleLibrary[])
        : (append(library, libraries) as CashFlowRuleLibrary[]);
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
            this.cashFlowRuleLibraries = upsertCashFlowRuleLibrary(
                this.cashFlowRuleLibraries,
                library,
            );
        },
        selectedCashFlowRuleLibraryMutator(libraryId: string) {
            this.selectedCashFlowRuleLibrary = selectCashFlowRuleLibraryById(
                this.cashFlowRuleLibraries,
                libraryId,
            );
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
        async getIsSharedCashFlowRuleLibrary(payload: SharedCashFlowLibraryPayload) {
            await CashFlowService.getIsSharedCashFlowRuleLibrary(payload.id).then(
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
});
