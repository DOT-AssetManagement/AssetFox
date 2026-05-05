import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { clone } from 'ramda';
import { hasValue } from '@/shared/utils/has-value-util';
import AnalysisMethodService from '@/services/analysis-method.service';
import {
    AnalysisMethod,
    emptyAnalysisMethod,
} from '@/shared/models/iAM/analysis-method';
import { http2XX } from '@/shared/utils/http-utils';
import { useNotificationStore } from './notification.store';

export interface AnalysisMethodStoreState {
    analysisMethod: AnalysisMethod;
    simulationAnalysisSetting: boolean;
}

export interface ScenarioIdPayload {
    scenarioId: string;
}

export interface UpsertAnalysisMethodPayload {
    analysisMethod: AnalysisMethod;
    scenarioId: string;
}

export const useAnalysisMethodStore = defineStore('analysisMethod', {
    state: (): AnalysisMethodStoreState => ({
        analysisMethod: clone(emptyAnalysisMethod) as AnalysisMethod,
        simulationAnalysisSetting: true,
    }),
    actions: {
        async getAnalysisMethod(payload: ScenarioIdPayload) {
            await AnalysisMethodService.getAnalysisMethod(payload.scenarioId).then(
                (response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        this.analysisMethod = clone(response.data as AnalysisMethod);
                    }
                },
            );
        },
        async getSimulationAnalysisSetting(payload: ScenarioIdPayload) {
            try {
                const response: AxiosResponse =
                    await AnalysisMethodService.getSimulationAnalysisSetting(
                        payload.scenarioId,
                    );
                if (response.data === false) {
                    this.simulationAnalysisSetting = false;
                } else if (response.data && typeof response.data === 'object') {
                    if ('isSettingStored' in response.data) {
                        this.simulationAnalysisSetting = Boolean(
                            response.data.isSettingStored,
                        );
                    }
                }
            } catch (error) {
                console.error('Error fetching analysis setting:', error);
                this.simulationAnalysisSetting = false;
            }
        },
        async upsertAnalysisMethod(payload: UpsertAnalysisMethodPayload) {
            const notificationStore = useNotificationStore();
            return await AnalysisMethodService.upsetAnalysisMethod(
                payload.analysisMethod,
                payload.scenarioId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.analysisMethod = clone(payload.analysisMethod);
                    notificationStore.addSuccessNotification({
                        message: 'Analysis method saved successfully',
                    });
                }
                return response.data;
            });
        },
    },
});
