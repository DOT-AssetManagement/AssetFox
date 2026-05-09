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

export interface TreatmentLibrarySelectionPayload {
    libraryId: string;
}

export interface ScenarioPayload {
    scenarioId: string;
}

export interface SimulationIdPayload {
    simulationId: string;
}

export interface TreatmentImportPayload {
    file: File;
    id: string;
}

export interface DeleteTreatmentPayload {
    treatments: SimpleTreatment[];
    treatment: Treatment;
    libraryId: string;
}

export interface DeleteScenarioSelectableTreatmentPayload {
    scenarioSelectableTreatment: Treatment;
    simulationId: string;
    treatments: SimpleTreatment[];
}

export interface UpsertOrDeleteTreatmentLibraryUsersPayload {
    libraryId: string;
    proposedUsers: LibraryUser[];
}

export interface IsSharedTreatmentLibraryPayload {
    id: string;
}

export interface DeleteTreatmentLibraryPayload {
    libraryId: string;
}

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

function selectedTreatmentLibraryMutator(
    state: TreatmentStoreState,
    libraryId: string,
) {
    if (any(propEq('id', libraryId), state.treatmentLibraries)) {
        state.selectedTreatmentLibrary = find(
            propEq('id', libraryId),
            state.treatmentLibraries,
        ) as TreatmentLibrary;
    } else {
        state.selectedTreatmentLibrary = clone(emptyTreatmentLibrary);
    }
}

function addedOrUpdatedTreatmentLibraryMutator(
    state: TreatmentStoreState,
    library: TreatmentLibrary,
) {
    state.treatmentLibraries = any(
        propEq('id', library.id),
        state.treatmentLibraries,
    )
        ? (update(
              findIndex(propEq('id', library.id), state.treatmentLibraries),
              library,
              state.treatmentLibraries,
          ) as TreatmentLibrary[])
        : (append(library, state.treatmentLibraries) as TreatmentLibrary[]);
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
        selectTreatmentLibrary(payload: TreatmentLibrarySelectionPayload) {
            selectedTreatmentLibraryMutator(this, payload.libraryId);
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
        async getScenarioSelectableTreatments(payload: ScenarioPayload) {
            const response = await TreatmentService.getScenarioSelectedTreatments(
                payload.scenarioId,
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
        async getTreatmentLibraryBySimulationId(payload: string | SimulationIdPayload) {
            const simulationId = typeof payload === 'string' ? payload : payload.simulationId;

            await TreatmentService.getTreatmentLibraryBySimulationId(simulationId).then(
                (response: AxiosResponse<TreatmentLibrary>) => {
                    if (hasValue(response, 'data')) {
                        this.scenarioTreatmentLibrary = clone(response.data);
                    }
                },
            );
        },
        async deleteTreatmentLibrary(payload: DeleteTreatmentLibraryPayload) {
            const notificationStore = useNotificationStore();
            await TreatmentService.deleteTreatmentLibrary(payload.libraryId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        deletedTreatmentLibraryMutator(this, payload.libraryId);
                        notificationStore.addSuccessNotification({
                            message: 'Deleted treatment library',
                        });
                    }
                },
            );
        },
        async importScenarioTreatmentsFile(payload: TreatmentImportPayload) {
            const alertStore = useAlertStore();
            await TreatmentService.importTreatments(
                payload.file,
                payload.id,
                true,
            ).then((response: AxiosResponse) => {
                if (!isNil(response)) {
                    alertStore.setAlertMessage(
                        'A treatment import has been added to the queue',
                    );
                }
            });
        },
        async importLibraryTreatmentsFile(payload: TreatmentImportPayload) {
            const alertStore = useAlertStore();
            await TreatmentService.importTreatments(
                payload.file,
                payload.id,
                false,
            ).then((response: AxiosResponse) => {
                if (!isNil(response)) {
                    alertStore.setAlertMessage(
                        'A treatment import has been added to the queue',
                    );
                }
            });
        },
        async importScenarioTreatmentSupersedeRulesFile(
            payload: TreatmentImportPayload,
        ) {
            const alertStore = useAlertStore();
            await TreatmentService.importSupersedeRules(
                payload.file,
                payload.id,
                true,
            ).then((response: AxiosResponse) => {
                if (!isNil(response)) {
                    alertStore.setAlertMessage(
                        'A supersede import has been added to the queue',
                    );
                }
            });
        },
        async importLibraryTreatmentSupersedeRulesFile(
            payload: TreatmentImportPayload,
        ) {
            const alertStore = useAlertStore();
            await TreatmentService.importSupersedeRules(
                payload.file,
                payload.id,
                false,
            ).then((response: AxiosResponse) => {
                if (!isNil(response)) {
                    alertStore.setAlertMessage(
                        'A supersede import has been added to the queue',
                    );
                }
            });
        },
        async deleteTreatment(payload: DeleteTreatmentPayload) {
            const notificationStore = useNotificationStore();
            await TreatmentService.deleteTreatment(
                payload.treatment,
                payload.libraryId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.simpleSelectableTreatments = clone(payload.treatments);
                    selectedTreatmentLibraryMutator(this, payload.libraryId);
                    notificationStore.addSuccessNotification({
                        message: 'Deleted treatment',
                    });
                }
            });
        },
        async deleteScenarioSelectableTreatment(
            payload: DeleteScenarioSelectableTreatmentPayload,
        ) {
            const notificationStore = useNotificationStore();
            await TreatmentService.deleteScenarioSelectableTreatment(
                payload.scenarioSelectableTreatment,
                payload.simulationId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.simpleScenarioSelectableTreatments = clone(payload.treatments);
                    notificationStore.addSuccessNotification({
                        message: 'Deleted scenario treatment',
                    });
                }
            });
        },
        async upsertOrDeleteTreatmentLibraryUsers(
            payload: UpsertOrDeleteTreatmentLibraryUsersPayload,
        ) {
            await TreatmentService.upsertOrDeleteTreatmentLibraryUsers(
                payload.libraryId,
                payload.proposedUsers,
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
        async getIsSharedTreatmentLibrary(payload: IsSharedTreatmentLibraryPayload) {
            await TreatmentService.getIsSharedLibrary(payload.id).then(
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
        addedOrUpdatedTreatmentLibraryMutator(payload: { library: TreatmentLibrary }) {
            addedOrUpdatedTreatmentLibraryMutator(this, payload.library);
        },
        selectedTreatmentLibraryMutator(payload: TreatmentLibrarySelectionPayload) {
            selectedTreatmentLibraryMutator(this, payload.libraryId);
        },
    },
});