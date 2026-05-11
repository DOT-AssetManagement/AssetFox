import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, propEq, reject, update } from 'ramda';
import TargetConditionGoalService from '@/services/target-condition-goal.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import {
    emptyTargetConditionGoalLibrary,
    TargetConditionGoalLibrary,
} from '@/shared/models/iAM/target-condition-goal';

export interface TargetConditionGoalLibrarySelectionPayload {
    libraryId: string;
}

export interface TargetConditionGoalDeletePayload {
    libraryId: string;
}

export interface TargetConditionGoalSharedLibraryPayload {
    id: string;
}

export interface TargetConditionGoalStoreState {
    targetConditionGoalLibraries: TargetConditionGoalLibrary[];
    selectedTargetConditionGoalLibrary: TargetConditionGoalLibrary;
    hasPermittedAccess: boolean;
    isSharedLibrary: boolean;
}

export const useTargetConditionGoalStore = defineStore('targetConditionGoal', {
    state: (): TargetConditionGoalStoreState => ({
        targetConditionGoalLibraries: [],
        selectedTargetConditionGoalLibrary: clone(emptyTargetConditionGoalLibrary),
        hasPermittedAccess: false,
        isSharedLibrary: false,
    }),
    actions: {
        addedOrUpdatedTargetConditionGoalLibraryMutator(
            library: TargetConditionGoalLibrary,
        ) {
            this.targetConditionGoalLibraries = any(
                propEq('id', library.id),
                this.targetConditionGoalLibraries,
            )
                ? (update(
                      findIndex(propEq('id', library.id), this.targetConditionGoalLibraries),
                      library,
                      this.targetConditionGoalLibraries,
                  ) as TargetConditionGoalLibrary[])
                : (append(library, this.targetConditionGoalLibraries) as TargetConditionGoalLibrary[]);
        },
        selectedTargetConditionGoalLibraryMutator(libraryId: string) {
            this.selectedTargetConditionGoalLibrary = any(
                propEq('id', libraryId),
                this.targetConditionGoalLibraries,
            )
                ? (find(
                      propEq('id', libraryId),
                      this.targetConditionGoalLibraries,
                  ) as TargetConditionGoalLibrary)
                : clone(emptyTargetConditionGoalLibrary);
        },
        selectTargetConditionGoalLibrary(
            payload: TargetConditionGoalLibrarySelectionPayload,
        ) {
            this.selectedTargetConditionGoalLibraryMutator(payload.libraryId);
        },
        async getTargetConditionGoalLibraries() {
            await TargetConditionGoalService.getTargetConditionGoalLibraries().then(
                (response: AxiosResponse<TargetConditionGoalLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.targetConditionGoalLibraries = clone(response.data);
                    }
                },
            );
        },
        async deleteTargetConditionGoalLibrary(
            payload: TargetConditionGoalDeletePayload,
        ) {
            const notificationStore = useNotificationStore();
            await TargetConditionGoalService.deleteTargetConditionGoalLibrary(
                payload.libraryId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.targetConditionGoalLibraries = reject(
                        (library: TargetConditionGoalLibrary) =>
                            payload.libraryId === library.id,
                        this.targetConditionGoalLibraries,
                    ) as TargetConditionGoalLibrary[];
                    notificationStore.addSuccessNotification({
                        message: 'Deleted target condition goal library',
                    });
                }
            });
        },
        async getIsSharedTargetConditionGoalLibrary(
            payload: TargetConditionGoalSharedLibraryPayload,
        ) {
            await TargetConditionGoalService.getIsSharedLibrary(payload.id).then(
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
        async getHasPermittedAccess() {
            await TargetConditionGoalService.getHasPermittedAccess().then(
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
    },
});
