import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, clone, find, findIndex, propEq, update } from 'ramda';
import ScenarioService from '@/services/scenario.service';
import { CloneScenarioData, emptyScenario, QueuedWork, Scenario } from '@/shared/models/iAM/scenario';
import { SimulationAnalysisDetail } from '@/shared/models/iAM/simulation-analysis-detail';
import { SimulationReportDetail } from '@/shared/models/iAM/simulation-report-detail';
import { PagingPage, PagingRequest } from '@/shared/models/iAM/paging';
import { SimulationOutputDeletionParameters } from '@/shared/models/iAM/simulation-output-deletion-params';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import { useNotificationStore } from './notification.store';

export interface ScenarioSelectionPayload {
    scenarioId: string;
}

export interface SimulationAnalysisDetailPayload {
    simulationAnalysisDetail: SimulationAnalysisDetail;
}

export interface QueuedWorkStatusPayload {
    workQueueStatusUpdate: QueuedWork;
}

export interface SimulationReportDetailPayload {
    simulationReportDetail: SimulationReportDetail;
}

export interface RunSimulationPayload {
    networkId: string;
    scenarioId?: string;
}

export interface MigrateLegacyScenarioPayload {
    simulationId: number;
    networkId: string;
}

export interface CreateScenarioPayload {
    scenario: Scenario;
    networkId: string;
}

export interface CloneScenarioPayload {
    scenarioId: string;
    destinationNetworkId: string;
    networkId: string;
    scenarioName: string;
}

export interface CloneScenarioWithDestinationNetworkPayload {
    id: string;
    destinationNetworkId: string;
    networkId: string;
    name: string;
}

export interface UpdateScenarioPayload {
    scenario: Scenario;
}

export interface DeleteScenarioPayload {
    scenarioId: string | number;
    scenarioName: string;
}

export interface ScenarioBySimulationIdPayload {
    simulationId: string;
}

export interface CancelWorkQueueItemPayload {
    simulationId: string;
}

export interface ScenarioStoreState {
    scenarios: Scenario[];
    currentSharedScenariosPage: Scenario[];
    currentUserScenarioPage: Scenario[];
    totalSharedScenarios: number;
    totalUserScenarios: number;
    selectedScenario: Scenario;
    currentUserOrSharedScenario: Scenario;
    queuedSimulations: QueuedWork[];
    currentWorkQueuePage: QueuedWork[];
    totalQueuedSimulations: number;
    fastQueueitems: QueuedWork[];
    currentFastWorkQueuePage: QueuedWork[];
    totalFastQueuedItems: number;
    simulationRunSettingId: string | number;
    simulationRunSettingName: string;
    scenarioNames: string[];
}

function selectedScenarioMutator(state: ScenarioStoreState, id: string) {
    if (any(propEq('id', id), state.currentSharedScenariosPage)) {
        state.selectedScenario = find(propEq('id', id), state.currentSharedScenariosPage) as Scenario;
    }
    else if(any(propEq('id', id), state.currentUserScenarioPage)) {
        state.selectedScenario = find(propEq('id', id), state.currentUserScenarioPage) as Scenario;
    }
    else {
        state.selectedScenario = id == emptyScenario.id ? clone(emptyScenario) : state.currentUserOrSharedScenario;
    }
}

function simulationAnalysisDetailMutator(state: ScenarioStoreState, simulationAnalysisDetail: SimulationAnalysisDetail) {
    if (any(propEq('id', simulationAnalysisDetail.simulationId), state.currentSharedScenariosPage)) {
        const updatedScenario: Scenario = find(propEq('id', simulationAnalysisDetail.simulationId), state.currentSharedScenariosPage) as Scenario;
        updatedScenario.lastRun = simulationAnalysisDetail.lastRun;
        updatedScenario.status = simulationAnalysisDetail.status;
        updatedScenario.runTime = simulationAnalysisDetail.runTime;

        state.currentSharedScenariosPage = update(
            findIndex(propEq('id', updatedScenario.id), state.currentSharedScenariosPage),
            updatedScenario,
            state.currentSharedScenariosPage,
        ) as Scenario[];
    }
    if(any(propEq('id', simulationAnalysisDetail.simulationId), state.currentUserScenarioPage)) {
        const updatedScenario: Scenario = find(propEq('id', simulationAnalysisDetail.simulationId), state.currentUserScenarioPage) as Scenario;
        updatedScenario.lastRun = simulationAnalysisDetail.lastRun;
        updatedScenario.status = simulationAnalysisDetail.status;
        updatedScenario.runTime = simulationAnalysisDetail.runTime;

        state.currentUserScenarioPage = update(
            findIndex(propEq('id', updatedScenario.id), state.currentUserScenarioPage),
            updatedScenario,
            state.currentUserScenarioPage,
        ) as Scenario[];
    }
    if(any(propEq('id', simulationAnalysisDetail.simulationId), state.currentWorkQueuePage)) {
        const updatedSimulation: QueuedWork = find(propEq('id', simulationAnalysisDetail.simulationId), state.currentWorkQueuePage) as QueuedWork;
        updatedSimulation.status = simulationAnalysisDetail.status;

        state.currentWorkQueuePage = update(
            findIndex(propEq('id', updatedSimulation.id), state.currentWorkQueuePage),
            updatedSimulation,
            state.currentWorkQueuePage,
        ) as QueuedWork[];
    }
}

function simulationReportDetailMutator(state: ScenarioStoreState, simulationReportDetail: SimulationReportDetail) {
    if (any(propEq('id', simulationReportDetail.simulationId), state.currentSharedScenariosPage)) {
        const updatedScenario: Scenario = find(propEq('id', simulationReportDetail.simulationId), state.currentSharedScenariosPage) as Scenario;
        updatedScenario.reportStatus = simulationReportDetail.status;

        state.currentSharedScenariosPage = update(
            findIndex(propEq('id', updatedScenario.id), state.currentSharedScenariosPage),
            updatedScenario,
            state.currentSharedScenariosPage,
        ) as Scenario[];
    }
    if (any(propEq('id', simulationReportDetail.simulationId), state.currentUserScenarioPage)) {
        const updatedScenario: Scenario = find(propEq('id', simulationReportDetail.simulationId), state.currentUserScenarioPage) as Scenario;
        updatedScenario.reportStatus = simulationReportDetail.status;

        state.currentUserScenarioPage = update(
            findIndex(propEq('id', updatedScenario.id), state.currentUserScenarioPage),
            updatedScenario,
            state.currentUserScenarioPage,
        ) as Scenario[];
    }
}

function workQueStatusUpdateMutator(state: ScenarioStoreState, queuedWorkUpdated: QueuedWork) {
    if (any(propEq('id', queuedWorkUpdated.id), state.currentWorkQueuePage)) {
        const updatedQueuedWork: QueuedWork = find(propEq('id', queuedWorkUpdated.id), state.currentWorkQueuePage) as QueuedWork;
        updatedQueuedWork.status = queuedWorkUpdated.status;

        state.currentWorkQueuePage = update(
            findIndex(propEq('id', updatedQueuedWork.id), state.currentWorkQueuePage),
            updatedQueuedWork,
            state.currentWorkQueuePage,
        ) as QueuedWork[];
    }
}

function fastWorkQueStatusUpdateMutator(state: ScenarioStoreState, queuedWorkUpdated: QueuedWork) {
    if (any(propEq('id', queuedWorkUpdated.id), state.currentFastWorkQueuePage)) {
        const updatedQueuedWork: QueuedWork = find(propEq('id', queuedWorkUpdated.id), state.currentFastWorkQueuePage) as QueuedWork;
        updatedQueuedWork.status = queuedWorkUpdated.status;

        state.currentFastWorkQueuePage = update(
            findIndex(propEq('id', updatedQueuedWork.id), state.currentFastWorkQueuePage),
            updatedQueuedWork,
            state.currentFastWorkQueuePage,
        ) as QueuedWork[];
    }
}

async function getScenarios(state: ScenarioStoreState) {
    await ScenarioService.getScenarios().then(
        (response: AxiosResponse<Scenario[]>) => {
            if (hasValue(response, 'data')) {
                state.scenarios = clone(response.data);
            }
        },
    );
}

export const useScenarioStore = defineStore('scenario', {
    state: (): ScenarioStoreState => ({
        scenarios: [],
        currentSharedScenariosPage: [],
        currentUserScenarioPage: [],
        totalSharedScenarios: 0,
        totalUserScenarios: 0,
        selectedScenario: clone(emptyScenario),
        currentUserOrSharedScenario: clone(emptyScenario),
        queuedSimulations: [],
        currentWorkQueuePage: [],
        totalQueuedSimulations: 0,
        fastQueueitems: [],
        currentFastWorkQueuePage: [],
        totalFastQueuedItems: 0,
        simulationRunSettingId: 0,
        simulationRunSettingName: '',
        scenarioNames: [],
    }),
    actions: {
        selectScenario(payload: ScenarioSelectionPayload) {
            selectedScenarioMutator(this, payload.scenarioId);
        },
        updateSimulationAnalysisDetail(payload: SimulationAnalysisDetailPayload) {
            simulationAnalysisDetailMutator(this, payload.simulationAnalysisDetail);
        },
        updateQueuedWorkStatus(payload: QueuedWorkStatusPayload) {
            workQueStatusUpdateMutator(this, payload.workQueueStatusUpdate);
        },
        updateFastQueuedWorkStatus(payload: QueuedWorkStatusPayload) {
            fastWorkQueStatusUpdateMutator(this, payload.workQueueStatusUpdate);
        },
        updateSimulationReportDetail(payload: SimulationReportDetailPayload) {
            simulationReportDetailMutator(this, payload.simulationReportDetail);
        },
        updateSimulationRunSettingId(payload: string | number) {
            this.simulationRunSettingId = payload;
        },
        updateSimulationRunSettingName(payload: string) {
            this.simulationRunSettingName = payload;
        },
        async getSharedScenariosPage(payload: PagingRequest<Scenario>) {
            await ScenarioService.getSharedScenariosPage(payload).then(
                (response: AxiosResponse<PagingPage<Scenario>>) => {
                    if (hasValue(response, 'data')) {
                        this.currentSharedScenariosPage = clone(response.data.items);
                        this.totalSharedScenarios = response.data.totalItems;
                    }
                },
            );
        },
        async getWorkQueuePage(payload: PagingRequest<QueuedWork>) {
            await ScenarioService.getWorkQueuePage(payload).then(
                (response: AxiosResponse<PagingPage<QueuedWork>>) => {
                    if (hasValue(response, 'data')) {
                        this.currentWorkQueuePage = clone(response.data.items);
                        this.totalQueuedSimulations = response.data.totalItems;
                    }
                },
            );
        },
        async getFastWorkQueuePage(payload: PagingRequest<QueuedWork>) {
            await ScenarioService.getFastWorkQueuePage(payload).then(
                (response: AxiosResponse<PagingPage<QueuedWork>>) => {
                    if (hasValue(response, 'data')) {
                        this.currentFastWorkQueuePage = clone(response.data.items);
                        this.totalFastQueuedItems = response.data.totalItems;
                    }
                },
            );
        },
        async getUserScenariosPage(payload: PagingRequest<Scenario>) {
            await ScenarioService.getUserScenariosPage(payload).then(
                (response: AxiosResponse<PagingPage<Scenario>>) => {
                    if (hasValue(response, 'data')) {
                        this.currentUserScenarioPage = clone(response.data.items);
                        this.totalUserScenarios = response.data.totalItems;
                    }
                },
            );
        },
        async getAllScenarioNames() {
            await ScenarioService.GetAllSimulationNames().then(
                (response: AxiosResponse<string[]>) => {
                    if (hasValue(response, 'data')) {
                        this.scenarioNames = clone(response.data);
                    }
                },
            );
        },
        async runSimulation(payload: RunSimulationPayload) {
            const notificationStore = useNotificationStore();
            await ScenarioService.runSimulation(
                payload.networkId,
                payload.scenarioId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    notificationStore.addSuccessNotification({
                        message: 'Simulation analysis started',
                    });
                }
            });
        },
        async migrateLegacySimulationData(payload: MigrateLegacyScenarioPayload) {
            const response = await ScenarioService.migrateLegacySimulationData(
                payload.simulationId,
            );

            if (
                hasValue(response, 'status') &&
                http2XX.test(response.status.toString())
            ) {
                await getScenarios(this);
            }
        },
        async createScenario(payload: CreateScenarioPayload) {
            const notificationStore = useNotificationStore();
            return await ScenarioService.createScenario(
                payload.scenario,
                payload.networkId,
            ).then((response: AxiosResponse) => {
                if (hasValue(response, 'data')) {
                    notificationStore.addSuccessNotification({
                        message: `Created scenario ${payload.scenario.name}`,
                    });
                }

                return response;
            });
        },
        async cloneScenario(payload: CloneScenarioPayload) {
            const notificationStore = useNotificationStore();
            const cloneScenarioData: CloneScenarioData = {
                scenarioId: payload.scenarioId,
                destinationNetworkId: payload.destinationNetworkId,
                networkId: payload.networkId,
                scenarioName: payload.scenarioName,
            };

            return await ScenarioService.cloneScenario(cloneScenarioData).then(
                (response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        notificationStore.addSuccessNotification({
                            message: `Cloned scenario ${cloneScenarioData.scenarioName}`,
                        });
                    }

                    return response;
                },
            );
        },
        async cloneScenarioWithDestinationNetwork(
            payload: CloneScenarioWithDestinationNetworkPayload,
        ) {
            const notificationStore = useNotificationStore();
            const cloneScenarioData: CloneScenarioData = {
                scenarioId: payload.id,
                destinationNetworkId: payload.destinationNetworkId,
                networkId: payload.networkId,
                scenarioName: payload.name,
            };

            return await ScenarioService.cloneScenario(cloneScenarioData).then(
                (response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        notificationStore.addSuccessNotification({
                            message: `Cloned scenario ${cloneScenarioData.scenarioName}`,
                        });
                    }

                    return response;
                },
            );
        },
        async updateScenario(payload: UpdateScenarioPayload) {
            const notificationStore = useNotificationStore();
            return await ScenarioService.updateScenario(payload.scenario).then(
                (response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        notificationStore.addSuccessNotification({
                            message: `Updated scenario ${payload.scenario.name}`,
                        });
                    }

                    return response;
                },
            );
        },
        async deleteScenario(payload: DeleteScenarioPayload) {
            const notificationStore = useNotificationStore();
            return await ScenarioService.deleteScenario(payload.scenarioId as any).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        notificationStore.addSuccessNotification({
                            message: `Deleting scenario ${payload.scenarioName}`,
                        });
                    }

                    return response;
                },
            );
        },
        async deleteScenarioOutput(dateParams: SimulationOutputDeletionParameters) {
            const notificationStore = useNotificationStore();
            return await ScenarioService.deleteScenarioOutputsWithingDaterange(
                dateParams,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    notificationStore.addSuccessNotification({
                        message: `Deleting scenario outputs of from ${dateParams.lowerBoundDate}to ${dateParams.upperBoundDate}`,
                    });
                }

                return response;
            });
        },
        async cancelWorkQueueItem(payload: CancelWorkQueueItemPayload) {
            const notificationStore = useNotificationStore();
            return await ScenarioService.cancelWorkQueueItem(payload.simulationId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        notificationStore.addSuccessNotification({
                            message: 'Canceling queue operation',
                        });
                    }

                    return response;
                },
            );
        },
        async cancelFastQueueItem(workId: string) {
            const notificationStore = useNotificationStore();
            return await ScenarioService.cancelFastQueueItem(workId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        notificationStore.addSuccessNotification({
                            message: 'Canceling queue operation',
                        });
                    }

                    return response;
                },
            );
        },
        async getCurrentUserOrSharedScenario(payload: ScenarioBySimulationIdPayload) {
            if (this.currentUserOrSharedScenario.id === emptyScenario.id) {
                await ScenarioService.getCurrentUserOrSharedScenario(
                    payload.simulationId,
                ).then((response: AxiosResponse<Scenario>) => {
                    if (hasValue(response, 'data')) {
                        this.currentUserOrSharedScenario = clone(response.data);
                    }
                });
            }
        },
    },
});
