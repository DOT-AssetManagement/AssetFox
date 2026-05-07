import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, propEq, reject, update } from 'ramda';
import RemainingLifeLimitService from '@/services/remaining-life-limit.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import {
    emptyRemainingLifeLimitLibrary,
    RemainingLifeLimitLibrary,
} from '@/shared/models/iAM/remaining-life-limit';

export interface RemainingLifeLimitLibrarySelectionPayload {
    libraryId: string;
}

export interface RemainingLifeLimitDeletePayload {
    libraryId: string;
}

export interface RemainingLifeLimitSharedLibraryPayload {
    id: string;
}

export interface RemainingLifeLimitStoreState {
    remainingLifeLimitLibraries: RemainingLifeLimitLibrary[];
    selectedRemainingLifeLimitLibrary: RemainingLifeLimitLibrary;
    hasPermittedAccess: boolean;
    isSharedLibrary: boolean;
}

function selectRemainingLifeLimitLibraryById(
    libraries: RemainingLifeLimitLibrary[],
    libraryId: string,
): RemainingLifeLimitLibrary {
    return any(propEq('id', libraryId), libraries)
        ? (find(propEq('id', libraryId), libraries) as RemainingLifeLimitLibrary)
        : clone(emptyRemainingLifeLimitLibrary);
}

function upsertRemainingLifeLimitLibrary(
    libraries: RemainingLifeLimitLibrary[],
    library: RemainingLifeLimitLibrary,
): RemainingLifeLimitLibrary[] {
    return any(propEq('id', library.id), libraries)
        ? (update(
              findIndex(propEq('id', library.id), libraries),
              library,
              libraries,
          ) as RemainingLifeLimitLibrary[])
        : (append(library, libraries) as RemainingLifeLimitLibrary[]);
}

export const useRemainingLifeLimitStore = defineStore('remainingLifeLimit', {
    state: (): RemainingLifeLimitStoreState => ({
        remainingLifeLimitLibraries: [],
        selectedRemainingLifeLimitLibrary: clone(emptyRemainingLifeLimitLibrary),
        hasPermittedAccess: false,
        isSharedLibrary: false,
    }),
    actions: {
        addedOrUpdatedRemainingLifeLimitLibraryMutator(
            library: RemainingLifeLimitLibrary,
        ) {
            this.remainingLifeLimitLibraries = upsertRemainingLifeLimitLibrary(
                this.remainingLifeLimitLibraries,
                library,
            );
        },
        selectedRemainingLifeLimitLibraryMutator(libraryId: string) {
            this.selectedRemainingLifeLimitLibrary =
                selectRemainingLifeLimitLibraryById(
                    this.remainingLifeLimitLibraries,
                    libraryId,
                );
        },
        selectRemainingLifeLimitLibrary(
            payload: RemainingLifeLimitLibrarySelectionPayload,
        ) {
            this.selectedRemainingLifeLimitLibraryMutator(payload.libraryId);
        },
        async getRemainingLifeLimitLibraries() {
            await RemainingLifeLimitService.getRemainingLifeLimitLibraries().then(
                (response: AxiosResponse<RemainingLifeLimitLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.remainingLifeLimitLibraries = clone(response.data);
                    }
                },
            );
        },
        async deleteRemainingLifeLimitLibrary(
            payload: RemainingLifeLimitDeletePayload,
        ) {
            const notificationStore = useNotificationStore();
            await RemainingLifeLimitService.deleteRemainingLifeLimitLibrary(
                payload.libraryId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.remainingLifeLimitLibraries = reject(
                        (library: RemainingLifeLimitLibrary) =>
                            payload.libraryId === library.id,
                        this.remainingLifeLimitLibraries,
                    ) as RemainingLifeLimitLibrary[];
                    notificationStore.addSuccessNotification({
                        message: 'Deleted remaining life limit library',
                    });
                }
            });
        },
        async getIsSharedRemainingLifeLimitLibrary(
            payload: RemainingLifeLimitSharedLibraryPayload,
        ) {
            await RemainingLifeLimitService.getIsSharedRemainingLifeLimitLibrary(
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
