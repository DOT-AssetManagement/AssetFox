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
        setSelectedCriterionIsValid(isValid: boolean) {
            this.selectedCriterionIsValid = isValid;
        },
        upsertSelectedScenarioRelatedCriterion(library: CriterionLibrary) {
            this.scenarioRelatedCriteria = clone(library);
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
        async getSelectedCriterionLibrary(libraryId: string) {
            if (!isNil(libraryId) && libraryId !== getBlankGuid()) {
                await CriterionLibraryService.getSelectedCriterionLibrary(
                    libraryId,
                ).then((response: AxiosResponse<CriterionLibrary>) => {
                    if (hasValue(response, 'data')) {
                        this.selectedCriterionLibrary = clone(response.data);
                    }
                });
            } else {
                this.selectedCriterionLibrary = clone(emptyCriterionLibrary);
            }
        },
        async selectScenarioRelatedCriterion(libraryId: string) {
            if (libraryId !== getBlankGuid()) {
                await CriterionLibraryService.getSelectedCriterionLibrary(
                    libraryId,
                ).then((response: AxiosResponse<CriterionLibrary>) => {
                    if (hasValue(response, 'data')) {
                        this.scenarioRelatedCriteria = clone(response.data);
                    }
                });
            } else {
                this.scenarioRelatedCriteria = clone(emptyCriterionLibrary);
            }
        },
        selectCriterionLibrary(libraryId: string) {
            const selectedLibrary = find(
                propEq('id', libraryId),
                this.criterionLibraries,
            ) as CriterionLibrary | undefined;
            this.selectedCriterionLibrary = selectedLibrary
                ? clone(selectedLibrary)
                : clone(emptyCriterionLibrary);
        },
        async upsertCriterionLibrary(library: CriterionLibrary): Promise<string> {
            const notificationStore = useNotificationStore();
            let returningId = getBlankGuid();

            await CriterionLibraryService.upsertCriterionLibrary(library).then(
                (response: AxiosResponse<string>) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
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
        async deleteCriterionLibrary(libraryId: string) {
            const notificationStore = useNotificationStore();
            await CriterionLibraryService.deleteCriterionLibrary(libraryId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.criterionLibraries = reject(
                            propEq('id', libraryId),
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
