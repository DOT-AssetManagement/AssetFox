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

export const useAnalysisMethodStore = defineStore('analysisMethod', {
    state: (): AnalysisMethodStoreState => ({
        analysisMethod: clone(emptyAnalysisMethod) as AnalysisMethod,
        simulationAnalysisSetting: true,
    }),
    getters: {
        analysisMethodGetter: (state) => state.analysisMethod,
        simulationAnalysisSettingGetter: (state) => state.simulationAnalysisSetting,
    },
    actions: {
        async getAnalysisMethod(scenarioId: string) {
            await AnalysisMethodService.getAnalysisMethod(scenarioId).then(
                (response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        this.analysisMethod = clone(response.data as AnalysisMethod);
                    }
                },
            );
        },
        async getSimulationAnalysisSetting(scenarioId: string) {
            try {
                const response: AxiosResponse =
                    await AnalysisMethodService.getSimulationAnalysisSetting(
                        scenarioId,
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
        async upsertAnalysisMethod(analysisMethod: AnalysisMethod, scenarioId: string) {
            const notificationStore = useNotificationStore();
            return await AnalysisMethodService.upsetAnalysisMethod(
                analysisMethod,
                scenarioId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.analysisMethod = clone(analysisMethod);
                    notificationStore.addSuccessNotification({
                        message: 'Analysis method saved successfully',
                    });
                }
                return response.data;
            });
        },
    },
});
