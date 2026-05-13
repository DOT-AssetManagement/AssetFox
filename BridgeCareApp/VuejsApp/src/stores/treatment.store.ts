import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, isNil, propEq, reject, update } from 'ramda';
import TreatmentService from '@/services/treatment.service';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import { useAlertStore } from './alert.store';
import { useNotificationStore } from './notification.store';
import { LibraryUser } from '@/shared/models/iAM/user';
import {
    emptyTreatmentLibrary,
    SimpleTreatment,
    Treatment,
    TreatmentLibrary,
} from '@/shared/models/iAM/treatment';

export interface TreatmentStoreState {
    treatmentLibraries: TreatmentLibrary[];
    selectedTreatmentLibrary: TreatmentLibrary;
    scenarioSelectableTreatments: Treatment[];
    scenarioTreatmentLibrary: TreatmentLibrary;
    simpleScenarioSelectableTreatments: SimpleTreatment[];
    simpleSelectableTreatments: SimpleTreatment[];
    hasPermittedAccess: boolean;
    isSharedLibrary: boolean;
}

function deletedTreatmentLibraryMutator(
    state: TreatmentStoreState,
    deletedLibraryId: string,
) {
    if (any(propEq('id', deletedLibraryId), state.treatmentLibraries)) {
        state.treatmentLibraries = reject(
            (library: TreatmentLibrary) => deletedLibraryId === library.id,
            state.treatmentLibraries,
        ) as TreatmentLibrary[];
    }
}

export const useTreatmentStore = defineStore('treatment', {
    state: (): TreatmentStoreState => ({
        treatmentLibraries: [],
        selectedTreatmentLibrary: clone(emptyTreatmentLibrary),
        scenarioSelectableTreatments: [],
        scenarioTreatmentLibrary: clone(emptyTreatmentLibrary),
        simpleScenarioSelectableTreatments: [],
        simpleSelectableTreatments: [],
        hasPermittedAccess: false,
        isSharedLibrary: false,
    }),
    actions: {
        selectTreatmentLibrary(libraryId: string) {
            this.selectedTreatmentLibraryMutator(libraryId);
        },
        async getTreatmentLibraries() {
            await TreatmentService.getTreatmentLibraries().then(
                (response: AxiosResponse<TreatmentLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.treatmentLibraries = clone(response.data);
                    }
                },
            );
        },
        async getScenarioSelectableTreatments(scenarioId: string) {
            const response = await TreatmentService.getScenarioSelectedTreatments(
                scenarioId,
            );

            if (hasValue(response, 'data')) {
                this.scenarioSelectableTreatments = clone(response.data as Treatment[]);
            }

            return response.data;
        },
        async getSimpleScenarioSelectableTreatments(scenarioId: string) {
            await TreatmentService.getSimpleTreatmentsByScenarioId(scenarioId).then(
                (response: AxiosResponse<SimpleTreatment[]>) => {
                    if (hasValue(response, 'data')) {
                        this.simpleScenarioSelectableTreatments = clone(response.data);
                    }
                },
            );
        },
        async getSimpleSelectableTreatments(libraryId: string) {
            await TreatmentService.getSimpleTreatmentsByLibraryId(libraryId).then(
                (response: AxiosResponse<SimpleTreatment[]>) => {
                    if (hasValue(response, 'data')) {
                        this.simpleSelectableTreatments = clone(response.data);
                    }
                },
            );
        },
        async getTreatmentLibraryBySimulationId(simulationId: string) {
            await TreatmentService.getTreatmentLibraryBySimulationId(simulationId).then(
                (response: AxiosResponse<TreatmentLibrary>) => {
                    if (hasValue(response, 'data')) {
                        this.scenarioTreatmentLibrary = clone(response.data);
                    }
                },
            );
        },
        async deleteTreatmentLibrary(libraryId: string) {
            const notificationStore = useNotificationStore();
            await TreatmentService.deleteTreatmentLibrary(libraryId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        deletedTreatmentLibraryMutator(this, libraryId);
                        notificationStore.addSuccessNotification({
                            message: 'Deleted treatment library',
                        });
                    }
                },
            );
        },
        async importScenarioTreatmentsFile(file: File, id: string) {
            const alertStore = useAlertStore();
            await TreatmentService.importTreatments(
                file,
                id,
                true,
            ).then((response: AxiosResponse) => {
                if (!isNil(response)) {
                    alertStore.setAlertMessage(
                        'A treatment import has been added to the queue',
                    );
                }
            });
        },
        async importLibraryTreatmentsFile(file: File, id: string) {
            const alertStore = useAlertStore();
            await TreatmentService.importTreatments(
                file,
                id,
                false,
            ).then((response: AxiosResponse) => {
                if (!isNil(response)) {
                    alertStore.setAlertMessage(
                        'A treatment import has been added to the queue',
                    );
                }
            });
        },
        async importScenarioTreatmentSupersedeRulesFile(file: File, id: string) {
            const alertStore = useAlertStore();
            await TreatmentService.importSupersedeRules(
                file,
                id,
                true,
            ).then((response: AxiosResponse) => {
                if (!isNil(response)) {
                    alertStore.setAlertMessage(
                        'A supersede import has been added to the queue',
                    );
                }
            });
        },
        async importLibraryTreatmentSupersedeRulesFile(file: File, id: string) {
            const alertStore = useAlertStore();
            await TreatmentService.importSupersedeRules(
                file,
                id,
                false,
            ).then((response: AxiosResponse) => {
                if (!isNil(response)) {
                    alertStore.setAlertMessage(
                        'A supersede import has been added to the queue',
                    );
                }
            });
        },
        async deleteTreatment(
            treatments: SimpleTreatment[],
            treatment: Treatment,
            libraryId: string,
        ) {
            const notificationStore = useNotificationStore();
            await TreatmentService.deleteTreatment(treatment, libraryId).then(
                (response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.simpleSelectableTreatments = clone(treatments);
                    this.selectedTreatmentLibraryMutator(libraryId);
                    notificationStore.addSuccessNotification({
                        message: 'Deleted treatment',
                    });
                }
                },
            );
        },
        async deleteScenarioSelectableTreatment(
            scenarioSelectableTreatment: Treatment,
            simulationId: string,
            treatments: SimpleTreatment[],
        ) {
            const notificationStore = useNotificationStore();
            await TreatmentService.deleteScenarioSelectableTreatment(
                scenarioSelectableTreatment,
                simulationId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.simpleScenarioSelectableTreatments = clone(treatments);
                    notificationStore.addSuccessNotification({
                        message: 'Deleted scenario treatment',
                    });
                }
            });
        },
        async upsertOrDeleteTreatmentLibraryUsers(
            libraryId: string,
            proposedUsers: LibraryUser[],
        ) {
            await TreatmentService.upsertOrDeleteTreatmentLibraryUsers(
                libraryId,
                proposedUsers,
            ).then((_response: AxiosResponse) => {});
        },
        async getHasPermittedAccess() {
            await TreatmentService.getHasPermittedAccess().then(
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
        async getIsSharedTreatmentLibrary(id: string) {
            await TreatmentService.getIsSharedLibrary(id).then(
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
        addedOrUpdatedTreatmentLibraryMutator(library: TreatmentLibrary) {
            this.treatmentLibraries = any(
                propEq('id', library.id),
                this.treatmentLibraries,
            )
                ? (update(
                      findIndex(propEq('id', library.id), this.treatmentLibraries),
                      library,
                      this.treatmentLibraries,
                  ) as TreatmentLibrary[])
                : (append(library, this.treatmentLibraries) as TreatmentLibrary[]);
        },
        selectedTreatmentLibraryMutator(libraryId: string) {
            if (any(propEq('id', libraryId), this.treatmentLibraries)) {
                this.selectedTreatmentLibrary = find(
                    propEq('id', libraryId),
                    this.treatmentLibraries,
                ) as TreatmentLibrary;
            } else {
                this.selectedTreatmentLibrary = clone(emptyTreatmentLibrary);
            }
        },
    },
    getters: {
        getTreatmentLibraries: (state) => state.treatmentLibraries,
        getSelectedTreatmentLibrary: (state) => state.selectedTreatmentLibrary,
        getScenarioSelectableTreatments: (state) => state.scenarioSelectableTreatments,
        getScenarioTreatmentLibrary: (state) => state.scenarioTreatmentLibrary,
        getSimpleScenarioSelectableTreatments: (state) => state.simpleScenarioSelectableTreatments,
        getSimpleSelectableTreatments: (state) => state.simpleSelectableTreatments,
        getHasPermittedAccess: (state) => state.hasPermittedAccess,
        getIsSharedLibrary: (state) => state.isSharedLibrary
    }
});