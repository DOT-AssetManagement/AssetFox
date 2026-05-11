import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, propEq, reject, update } from 'ramda';
import DeficientConditionGoalService from '@/services/deficient-condition-goal.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import {
    DeficientConditionGoalLibrary,
    emptyDeficientConditionGoalLibrary,
} from '@/shared/models/iAM/deficient-condition-goal';

export interface DeficientConditionGoalLibrarySelectionPayload {
    libraryId: string;
}

export interface DeficientConditionGoalSharedLibraryPayload {
    id: string;
}

export interface DeficientConditionGoalDeletePayload {
    libraryId: string;
}

export interface DeficientConditionGoalStoreState {
    deficientConditionGoalLibraries: DeficientConditionGoalLibrary[];
    selectedDeficientConditionGoalLibrary: DeficientConditionGoalLibrary;
    hasPermittedAccess: boolean;
    isSharedLibrary: boolean;
}

export const useDeficientConditionGoalStore = defineStore('deficientConditionGoal', {
    state: (): DeficientConditionGoalStoreState => ({
        deficientConditionGoalLibraries: [],
        selectedDeficientConditionGoalLibrary: clone(
            emptyDeficientConditionGoalLibrary,
        ),
        hasPermittedAccess: false,
        isSharedLibrary: false,
    }),
    actions: {
        addedOrUpdatedDeficientConditionGoalLibraryMutator(
            library: DeficientConditionGoalLibrary,
        ) {
            this.deficientConditionGoalLibraries = any(
                propEq('id', library.id),
                this.deficientConditionGoalLibraries,
            )
                ? (update(
                      findIndex(propEq('id', library.id), this.deficientConditionGoalLibraries),
                      library,
                      this.deficientConditionGoalLibraries,
                  ) as DeficientConditionGoalLibrary[])
                : (append(library, this.deficientConditionGoalLibraries) as DeficientConditionGoalLibrary[]);
        },
        selectedDeficientConditionGoalLibraryMutator(libraryId: string) {
            this.selectedDeficientConditionGoalLibrary = any(
                propEq('id', libraryId),
                this.deficientConditionGoalLibraries,
            )
                ? (find(
                      propEq('id', libraryId),
                      this.deficientConditionGoalLibraries,
                  ) as DeficientConditionGoalLibrary)
                : clone(emptyDeficientConditionGoalLibrary);
        },
        selectDeficientConditionGoalLibrary(
            payload: DeficientConditionGoalLibrarySelectionPayload,
        ) {
            this.selectedDeficientConditionGoalLibraryMutator(payload.libraryId);
        },
        async getDeficientConditionGoalLibraries() {
            await DeficientConditionGoalService.getDeficientConditionGoalLibraries().then(
                (response: AxiosResponse<DeficientConditionGoalLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.deficientConditionGoalLibraries = clone(response.data);
                    }
                },
            );
        },
        async deleteDeficientConditionGoalLibrary(
            payload: DeficientConditionGoalDeletePayload,
        ) {
            const notificationStore = useNotificationStore();
            await DeficientConditionGoalService.deleteDeficientConditionGoalLibrary(
                payload.libraryId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.deficientConditionGoalLibraries = reject(
                        (library: DeficientConditionGoalLibrary) =>
                            payload.libraryId === library.id,
                        this.deficientConditionGoalLibraries,
                    ) as DeficientConditionGoalLibrary[];
                    notificationStore.addSuccessNotification({
                        message: 'Deleted target condition goal library',
                    });
                }
            });
        },
        async getHasPermittedAccess() {
            await DeficientConditionGoalService.getHasPermittedAccess().then(
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
        async getIsSharedDeficientConditionGoalLibrary(
            payload: DeficientConditionGoalSharedLibraryPayload,
        ) {
            await DeficientConditionGoalService.getIsSharedDeficientConditionGoalLibrary(
                payload.id,
            ).then((response: AxiosResponse<boolean>) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.isSharedLibrary = response.data as boolean;
                }
            });
        },
    },
});
