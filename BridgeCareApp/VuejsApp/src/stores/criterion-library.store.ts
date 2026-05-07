import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, isNil, propEq, reject, update } from 'ramda';
import CriterionLibraryService from '@/services/criterion-library.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import { getBlankGuid } from '@/shared/utils/uuid-utils';
import {
    CriterionLibrary,
    emptyCriterionLibrary,
} from '@/shared/models/iAM/criteria';

export interface CriterionLibrarySelectionPayload {
    libraryId: string;
}

export interface CriterionLibraryDeletePayload {
    libraryId: string;
}

export interface CriterionValidityPayload {
    isValid: boolean;
}

export interface CriterionLibraryUpsertPayload {
    library: CriterionLibrary;
}

export interface CriterionLibraryStoreState {
    criterionLibraries: CriterionLibrary[];
    selectedCriterionLibrary: CriterionLibrary;
    selectedCriterionIsValid: boolean;
    scenarioRelatedCriteria: CriterionLibrary;
    hasPermittedAccess: boolean;
}

function upsertCriterionLibraryState(
    libraries: CriterionLibrary[],
    library: CriterionLibrary,
): CriterionLibrary[] {
    return any(propEq('id', library.id), libraries)
        ? (update(
              findIndex(propEq('id', library.id), libraries),
              library,
              libraries,
          ) as CriterionLibrary[])
        : (append(library, libraries) as CriterionLibrary[]);
}

export const useCriterionLibraryStore = defineStore('criterionLibrary', {
    state: (): CriterionLibraryStoreState => ({
        criterionLibraries: [],
        selectedCriterionLibrary: clone(emptyCriterionLibrary),
        selectedCriterionIsValid: false,
        scenarioRelatedCriteria: clone(emptyCriterionLibrary),
        hasPermittedAccess: false,
    }),
    actions: {
        setSelectedCriterionIsValid(payload: CriterionValidityPayload) {
            this.selectedCriterionIsValid = payload.isValid;
        },
        upsertSelectedScenarioRelatedCriterion(payload: CriterionLibraryUpsertPayload) {
            this.scenarioRelatedCriteria = clone(payload.library);
        },
        async getCriterionLibraries() {
            await CriterionLibraryService.getCriterionLibraries().then(
                (response: AxiosResponse<CriterionLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.criterionLibraries = clone(response.data);
                    }
                },
            );
        },
        async getSelectedCriterionLibrary(payload: CriterionLibrarySelectionPayload) {
            if (!isNil(payload.libraryId) && payload.libraryId !== getBlankGuid()) {
                await CriterionLibraryService.getSelectedCriterionLibrary(
                    payload.libraryId,
                ).then((response: AxiosResponse<CriterionLibrary>) => {
                    if (hasValue(response, 'data')) {
                        this.selectedCriterionLibrary = clone(response.data);
                    }
                });
            } else {
                this.selectedCriterionLibrary = clone(emptyCriterionLibrary);
            }
        },
        async selectScenarioRelatedCriterion(payload: CriterionLibrarySelectionPayload) {
            if (payload.libraryId !== getBlankGuid()) {
                await CriterionLibraryService.getSelectedCriterionLibrary(
                    payload.libraryId,
                ).then((response: AxiosResponse<CriterionLibrary>) => {
                    if (hasValue(response, 'data')) {
                        this.scenarioRelatedCriteria = clone(response.data);
                    }
                });
            } else {
                this.scenarioRelatedCriteria = clone(emptyCriterionLibrary);
            }
        },
        selectCriterionLibrary(payload: CriterionLibrarySelectionPayload) {
            const selectedLibrary = find(
                propEq('id', payload.libraryId),
                this.criterionLibraries,
            ) as CriterionLibrary | undefined;
            this.selectedCriterionLibrary = selectedLibrary
                ? clone(selectedLibrary)
                : clone(emptyCriterionLibrary);
        },
        async upsertCriterionLibrary(
            payload: CriterionLibraryUpsertPayload,
        ): Promise<string> {
            const notificationStore = useNotificationStore();
            let returningId = getBlankGuid();

            await CriterionLibraryService.upsertCriterionLibrary(payload.library).then(
                (response: AxiosResponse<string>) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        const library = payload.library;
                        const message = any(
                            propEq('id', library.id),
                            this.criterionLibraries,
                        )
                            ? 'Updated criterion library'
                            : 'Added criterion library';

                        this.criterionLibraries = upsertCriterionLibraryState(
                            this.criterionLibraries,
                            library,
                        );

                        if (!library.isSingleUse) {
                            this.selectedCriterionLibrary = clone(library);
                        } else {
                            this.scenarioRelatedCriteria = clone(library);
                        }

                        notificationStore.addSuccessNotification({ message });
                        returningId = response.data;
                    }
                },
            );

            return returningId;
        },
        async deleteCriterionLibrary(payload: CriterionLibraryDeletePayload) {
            const notificationStore = useNotificationStore();
            await CriterionLibraryService.deleteCriterionLibrary(payload.libraryId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.criterionLibraries = reject(
                            propEq('id', payload.libraryId),
                            this.criterionLibraries,
                        ) as CriterionLibrary[];
                        this.selectedCriterionIsValid = false;
                        notificationStore.addSuccessNotification({
                            message: 'Deleted criterion library',
                        });
                    }
                },
            );
        },
        async getHasPermittedAccess() {
            await CriterionLibraryService.getHasPermittedAccess().then(
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
