import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, propEq, reject, update } from 'ramda';
import BudgetPriorityService from '@/services/budget-priority.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import {
    BudgetPriority,
    BudgetPriorityLibrary,
    emptyBudgetPriorityLibrary,
} from '@/shared/models/iAM/budget-priority';
import { LibraryUser } from '@/shared/models/iAM/user';

export interface BudgetPriorityLibrarySelectionPayload {
    libraryId: string;
}

export interface SharedLibraryPayload {
    id: string;
}

export interface BudgetPriorityStoreState {
    budgetPriorityLibraries: BudgetPriorityLibrary[];
    selectedBudgetPriorityLibrary: BudgetPriorityLibrary;
    hasPermittedAccess: boolean;
    isSharedLibrary: boolean;
}

function upsertBudgetPriorityLibrary(
    libraries: BudgetPriorityLibrary[],
    library: BudgetPriorityLibrary,
): BudgetPriorityLibrary[] {
    return any(propEq('id', library.id), libraries)
        ? (update(
              findIndex(propEq('id', library.id), libraries),
              library,
              libraries,
          ) as BudgetPriorityLibrary[])
        : (append(library, libraries) as BudgetPriorityLibrary[]);
}

function selectBudgetPriorityLibraryById(
    libraries: BudgetPriorityLibrary[],
    libraryId: string,
): BudgetPriorityLibrary {
    return any(propEq('id', libraryId), libraries)
        ? (find(propEq('id', libraryId), libraries) as BudgetPriorityLibrary)
        : clone(emptyBudgetPriorityLibrary);
}

export const useBudgetPriorityStore = defineStore('budgetPriority', {
    state: (): BudgetPriorityStoreState => ({
        budgetPriorityLibraries: [],
        selectedBudgetPriorityLibrary: clone(emptyBudgetPriorityLibrary),
        hasPermittedAccess: false,
        isSharedLibrary: false,
    }),
    actions: {
        budgetPriorityLibraryMutator(library: BudgetPriorityLibrary) {
            this.budgetPriorityLibraries = upsertBudgetPriorityLibrary(
                this.budgetPriorityLibraries,
                library,
            );
        },
        selectedBudgetPriorityLibraryMutator(libraryId: string) {
            this.selectedBudgetPriorityLibrary = selectBudgetPriorityLibraryById(
                this.budgetPriorityLibraries,
                libraryId,
            );
        },
        selectBudgetPriorityLibrary(payload: BudgetPriorityLibrarySelectionPayload) {
            this.selectedBudgetPriorityLibraryMutator(payload.libraryId);
        },
        async getBudgetPriorityLibraries() {
            await BudgetPriorityService.getBudgetPriorityLibraries().then(
                (response: AxiosResponse<BudgetPriorityLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.budgetPriorityLibraries = clone(response.data);
                    }
                },
            );
        },
        async deleteBudgetPriorityLibrary(libraryId: string) {
            const notificationStore = useNotificationStore();
            await BudgetPriorityService.deleteBudgetPriorityLibrary(libraryId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.budgetPriorityLibraries = reject(
                            propEq('id', libraryId),
                            this.budgetPriorityLibraries,
                        ) as BudgetPriorityLibrary[];
                        notificationStore.addSuccessNotification({
                            message: 'Deleted budget priority library',
                        });
                    }
                },
            );
        },
        async getHasPermittedAccess() {
            await BudgetPriorityService.getHasPermittedAccess().then(
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
        async getIsSharedBudgetPriorityLibrary(payload: SharedLibraryPayload) {
            await BudgetPriorityService.getIsSharedBudgetPriorityLibrary(payload.id).then(
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
