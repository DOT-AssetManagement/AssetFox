import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, propEq, reject, update } from 'ramda';
import PerformanceCurveService from '@/services/performance-curve.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import {
    emptyPerformanceCurveLibrary,
    PerformanceCurve,
    PerformanceCurveLibrary,
} from '@/shared/models/iAM/performance';
import { UserCriteriaFilter } from '@/shared/models/iAM/user-criteria-filter';
import { useAlertStore } from './alert.store';

export interface PerformanceCurveImportPayload {
    file: File;
    id: string;
    currentUserCriteriaFilter: UserCriteriaFilter;
}

export interface PerformanceCurveStoreState {
    performanceCurveLibraries: PerformanceCurveLibrary[];
    selectedPerformanceCurveLibrary: PerformanceCurveLibrary;
    scenarioPerformanceCurves: PerformanceCurve[];
    hasPermittedAccess: boolean;
    isSharedLibrary: boolean;
    isDeteriorationModelApiRunning: boolean;
    distinctPerformanceFactorAttributes: string[];
}

export const usePerformanceCurveStore = defineStore('performanceCurve', {
    state: (): PerformanceCurveStoreState => ({
        performanceCurveLibraries: [],
        selectedPerformanceCurveLibrary: clone(emptyPerformanceCurveLibrary),
        scenarioPerformanceCurves: [],
        hasPermittedAccess: false,
        isSharedLibrary: false,
        isDeteriorationModelApiRunning: false,
        distinctPerformanceFactorAttributes: [],
    }),
    actions: {
        performanceCurveLibraryMutator(library: PerformanceCurveLibrary) {
            this.performanceCurveLibraries = any(
                propEq('id', library.id),
                this.performanceCurveLibraries,
            )
                ? (update(
                      findIndex(propEq('id', library.id), this.performanceCurveLibraries),
                      library,
                      this.performanceCurveLibraries,
                  ) as PerformanceCurveLibrary[])
                : (append(library, this.performanceCurveLibraries) as PerformanceCurveLibrary[]);
        },
        selectedPerformanceCurveLibraryMutator(libraryId: string) {
            this.selectedPerformanceCurveLibrary = any(
                propEq('id', libraryId),
                this.performanceCurveLibraries,
            )
                ? (find(
                      propEq('id', libraryId),
                      this.performanceCurveLibraries,
                  ) as PerformanceCurveLibrary)
                : clone(emptyPerformanceCurveLibrary);
        },
        selectPerformanceCurveLibrary(libraryId: string) {
            this.selectedPerformanceCurveLibraryMutator(libraryId);
        },
        async getPerformanceCurveLibraries() {
            await PerformanceCurveService.getPerformanceCurveLibraries().then(
                (response: AxiosResponse<PerformanceCurveLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.performanceCurveLibraries = clone(response.data);
                    }
                },
            );
        },
        async deletePerformanceCurveLibrary(libraryId: string) {
            const notificationStore = useNotificationStore();
            await PerformanceCurveService.deletePerformanceCurveLibrary(
                libraryId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.performanceCurveLibraries = reject(
                        propEq('id', libraryId),
                        this.performanceCurveLibraries,
                    ) as PerformanceCurveLibrary[];
                    notificationStore.addSuccessNotification({
                        message: 'Deleted deterioration model library',
                    });
                }
            });
        },
        async getDistinctScenarioPerformanceFactorAttributeNames() {
            await PerformanceCurveService.GetDistinctScenarioPerformanceFactorAttributeNames().then(
                (response: AxiosResponse<string[]>) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.distinctPerformanceFactorAttributes = clone(response.data);
                    }
                },
            );
        },
        async getScenarioPerformanceCurves(scenarioId: string) {
            await PerformanceCurveService.GetPerformanceCurves(scenarioId).then(
                (response: AxiosResponse<PerformanceCurve[]>) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.scenarioPerformanceCurves = clone(response.data);
                    }
                },
            );
        },
        async importScenarioPerformanceCurvesFile(
            payload: PerformanceCurveImportPayload,
        ) {
            const alertStore = useAlertStore();
            await PerformanceCurveService.importPerformanceCurves(
                payload.file,
                payload.id,
                true,
                payload.currentUserCriteriaFilter,
            ).then((response: AxiosResponse) => {
                if (response) {
                    alertStore.setAlertMessage(
                        'A performance curve import has been added to the queue',
                    );
                }
            });
        },
        async importLibraryPerformanceCurvesFile(
            payload: PerformanceCurveImportPayload,
        ) {
            const alertStore = useAlertStore();
            await PerformanceCurveService.importPerformanceCurves(
                payload.file,
                payload.id,
                false,
                payload.currentUserCriteriaFilter,
            ).then((response: AxiosResponse) => {
                if (response) {
                    alertStore.setAlertMessage(
                        'A performance curve import has been added to the queue',
                    );
                }
            });
        },
        async getIsSharedPerformanceCurveLibrary(payload: { id: string }) {
            await PerformanceCurveService.getIsSharedLibrary(payload.id).then(
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
        getIsDeteriorationModelApiRunning(payload: boolean) {
            this.isDeteriorationModelApiRunning = payload;
        },
        async getHasPermittedAccess() {
            await PerformanceCurveService.getHasPermittedAccess().then(
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
