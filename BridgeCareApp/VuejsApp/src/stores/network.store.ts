import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, propEq, reject, update } from 'ramda';
import prepend from 'ramda/es/prepend';
import NetworkService from '@/services/network.service';
import AggregationService from '@/services/aggregation.service';
import { BenefitQuantifier, emptyNetwork, Network } from '@/shared/models/iAM/network';
import { Attribute } from '@/shared/models/iAM/attribute';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import { useNotificationStore } from './notification.store';

export interface NetworkStoreState {
    networks: Network[];
    selectedNetwork: Network;
    compatibleNetworks: Network[];
}

function selectNetworkById(networks: Network[], networkId: string): Network {
    const selectedNetwork = find(
        propEq('id', networkId),
        networks,
    ) as Network | undefined;

    return selectedNetwork ? selectedNetwork : clone(emptyNetwork);
}

function applyBenefitQuantifier(
    networks: Network[],
    benefitQuantifier: BenefitQuantifier,
): Network[] {
    if (!any(propEq('id', benefitQuantifier.networkId), networks)) {
        return networks;
    }

    const network = find(
        propEq('id', benefitQuantifier.networkId),
        networks,
    ) as Network;

    return update(
        findIndex(propEq('id', network.id), networks),
        { ...network, benefitQuantifier },
        networks,
    ) as Network[];
}

export const useNetworkStore = defineStore('network', {
    state: (): NetworkStoreState => ({
        networks: [],
        selectedNetwork: clone(emptyNetwork),
        compatibleNetworks: [],
    }),
    actions: {
        selectNetwork(networkId: string) {
            this.selectedNetwork = selectNetworkById(this.networks, networkId);
        },
        async getNetworks() {
            await NetworkService.getNetworks().then(
                (response: AxiosResponse<Network[]>) => {
                    if (hasValue(response, 'data')) {
                        this.networks = clone(response.data);
                    }
                },
            );
        },
        async getCompatibleNetworks(networkId: string) {
            await NetworkService.getCompatibleNetworks(networkId).then(
                (response: AxiosResponse<Network[]>) => {
                    if (hasValue(response, 'data')) {
                        this.compatibleNetworks = clone(response.data);
                    }
                },
            );
        },
        async createNetwork(
            network: Network,
            dataSourceId: string,
            parameters: Attribute,
        ) {
            const notificationStore = useNotificationStore();
            return await NetworkService.createNetwork(
                network.name,
                dataSourceId,
                parameters,
            ).then((response: AxiosResponse<string>) => {
                if (hasValue(response, 'data')) {
                    const message = any(propEq('id', response.data), this.networks)
                        ? 'Updated network'
                        : 'Added network';

                    return this.getNetworks().then(() => {
                        this.selectedNetwork = selectNetworkById(
                            this.networks,
                            response.data,
                        );
                        notificationStore.addSuccessNotification({ message });
                        return response;
                    });
                }

                return response;
            });
        },
        async deleteNetwork(networkId: string) {
            const notificationStore = useNotificationStore();
            return await NetworkService.deleteNetwork(networkId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        notificationStore.addSuccessNotification({
                            message: 'Network deletion started',
                        });
                        this.networks = reject(
                            propEq('id', networkId),
                            this.networks,
                        ) as Network[];
                    }

                    return response;
                },
            );
        },
        async editNetworkName(networkId: string, newNetworkName: string) {
            const notificationStore = useNotificationStore();
            return await NetworkService.EditNetworkName(
                networkId,
                newNetworkName,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    notificationStore.addSuccessNotification({
                        message: 'Network name successfully updated',
                    });
                }

                return response;
            });
        },
        async upsertBenefitQuantifier(benefitQuantifier: BenefitQuantifier) {
            const notificationStore = useNotificationStore();
            return await NetworkService.upsertBenefitQuantifier(
                benefitQuantifier,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.networks = applyBenefitQuantifier(
                        this.networks,
                        benefitQuantifier,
                    );
                    notificationStore.addSuccessNotification({
                        message: 'Benefit quantifier upsertted',
                    });
                }

                return response;
            });
        },
        async aggregateNetworkData(dataSource: string, networkId: string) {
            const notificationStore = useNotificationStore();
            return await AggregationService.AggregateNetworkData(
                dataSource,
                networkId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    notificationStore.addSuccessNotification({
                        message: 'Aggregation Started',
                    });
                }

                return response;
            });
        },
    },
});