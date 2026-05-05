import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { clone } from 'ramda';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import AdminDataService from '@/services/admin-data.service';
import { useNotificationStore } from './notification.store';

export interface AdminDataStoreState {
    availableReportNames: string[];
    simulationReportNames: string[];
    inventoryReportNames: string[];
    primaryNetwork: string;
    rawdataNetwork: string;
    keyFields: string[];
    rawDataKeyFields: string[];
    assetType: string[];
    constraintType: string;
}

function cloneStringArray(values: string[] | null): string[] {
    return values !== null ? clone(values) : [];
}

function normalizeStringValue(value: string | null): string {
    return value !== null ? value : '';
}

export const useAdminDataStore = defineStore('adminData', {
    state: (): AdminDataStoreState => ({
        availableReportNames: [],
        simulationReportNames: [],
        inventoryReportNames: [],
        primaryNetwork: '',
        rawdataNetwork: '',
        keyFields: [],
        rawDataKeyFields: [],
        assetType: [],
        constraintType: '',
    }),
    actions: {
        async getAvailableReports() {
            await AdminDataService.getAvailableReportNames().then(
                (response: AxiosResponse<string[]>) => {
                    if (hasValue(response, 'data')) {
                        this.availableReportNames = cloneStringArray(response.data);
                    }
                },
            );
        },
        async getSimulationReports() {
            await AdminDataService.getSimulationReportNames().then(
                (response: AxiosResponse<string[]>) => {
                    if (hasValue(response, 'data')) {
                        this.simulationReportNames = cloneStringArray(response.data);
                    }
                },
            );
        },
        async getInventoryReports() {
            await AdminDataService.getInventoryReports().then(
                (response: AxiosResponse<string[]>) => {
                    if (hasValue(response, 'data')) {
                        this.inventoryReportNames = cloneStringArray(response.data);
                    }
                },
            );
        },
        async getPrimaryNetwork() {
            await AdminDataService.getPrimaryNetwork().then(
                (response: AxiosResponse<string>) => {
                    if (hasValue(response, 'data')) {
                        this.primaryNetwork = normalizeStringValue(response.data);
                    }
                },
            );
        },
        async getRawdataNetwork() {
            await AdminDataService.getRawdataNetwork().then(
                (response: AxiosResponse<string>) => {
                    if (hasValue(response, 'data')) {
                        this.rawdataNetwork = normalizeStringValue(response.data);
                    }
                },
            );
        },
        async getKeyFields() {
            await AdminDataService.getKeyFields().then(
                (response: AxiosResponse<string[]>) => {
                    if (hasValue(response, 'data')) {
                        this.keyFields = cloneStringArray(response.data);
                    }
                },
            );
        },
        async getRawDataKeyFields() {
            await AdminDataService.getRawDataKeyFields().then(
                (response: AxiosResponse<string[]>) => {
                    if (hasValue(response, 'data')) {
                        this.rawDataKeyFields = cloneStringArray(response.data);
                    }
                },
            );
        },
        async getAssetType() {
            await AdminDataService.getAssetType().then(
                (response: AxiosResponse<string[]>) => {
                    if (hasValue(response, 'data')) {
                        this.assetType = cloneStringArray(response.data);
                    }
                },
            );
        },
        async getConstraintType() {
            await AdminDataService.getConstraintType().then(
                (response: AxiosResponse<string>) => {
                    if (hasValue(response, 'data')) {
                        this.constraintType = normalizeStringValue(response.data);
                    }
                },
            );
        },
        async setSimulationReports(reports: string) {
            const notificationStore = useNotificationStore();
            await AdminDataService.setSimulationReports(reports).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.simulationReportNames = reports.split(',');
                        notificationStore.addSuccessNotification({
                            message: 'Modified simulation reports',
                        });
                    }
                },
            );
        },
        async setInventoryReports(reports: string) {
            const notificationStore = useNotificationStore();
            await AdminDataService.setInventoryReports(reports).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.inventoryReportNames = reports.split(',');
                        notificationStore.addSuccessNotification({
                            message: 'Modified inventory reports',
                        });
                    }
                },
            );
        },
        async setPrimaryNetwork(network: string) {
            const notificationStore = useNotificationStore();
            await AdminDataService.setPrimaryNetwork(network).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.primaryNetwork = network;
                        notificationStore.addSuccessNotification({
                            message: 'Modified primary network',
                        });
                    }
                },
            );
        },
        async setRawdataNetwork(network: string) {
            const notificationStore = useNotificationStore();
            await AdminDataService.setRawdataNetwork(network).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.rawdataNetwork = network;
                        notificationStore.addSuccessNotification({
                            message: 'Modified primary network',
                        });
                    }
                },
            );
        },
        async setKeyFields(keyFields: string) {
            const notificationStore = useNotificationStore();
            await AdminDataService.setKeyFields(keyFields).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.keyFields = keyFields.split(',');
                        notificationStore.addSuccessNotification({
                            message: 'Modified key fields',
                        });
                    }
                },
            );
        },
        async setRawDataKeyFields(keyFields: string) {
            const notificationStore = useNotificationStore();
            await AdminDataService.setRawDataKeyFields(keyFields).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.rawDataKeyFields = keyFields.split(',');
                        notificationStore.addSuccessNotification({
                            message: 'Modified raw data key fields',
                        });
                    }
                },
            );
        },
        async setAssetType(assetType: string) {
            const notificationStore = useNotificationStore();
            await AdminDataService.setAssetType(assetType).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.assetType = assetType.split(',');
                        notificationStore.addSuccessNotification({
                            message: 'Modified asset type',
                        });
                    }
                },
            );
        },
        async setConstraintType(constraintType: string) {
            const notificationStore = useNotificationStore();
            await AdminDataService.setConstraintType(constraintType).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.constraintType = constraintType;
                        notificationStore.addSuccessNotification({
                            message: 'Modified constraint type',
                        });
                    }
                },
            );
        },
    },
});
