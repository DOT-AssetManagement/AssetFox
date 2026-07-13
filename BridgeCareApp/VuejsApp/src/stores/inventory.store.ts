import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { clone } from 'ramda';
import InventoryService from '@/services/inventory.service';
import { InventoryItem, InventoryParam } from '@/shared/models/iAM/inventory';
import { hasValue } from '@/shared/utils/has-value-util';

export interface InventoryStoreState {
    inventoryItems: InventoryItem[];
    staticHTMLForInventory: string | null;
    querySet: InventoryParam[];
}

export const useInventoryStore = defineStore('inventory', {
    state: (): InventoryStoreState => ({
        inventoryItems: [],
        staticHTMLForInventory: '',
        querySet: [],
    }),
    actions: {
        async getInventory(keyProperties: any) {
            const response = await InventoryService.getInventory(keyProperties);
            if (hasValue(response, 'data')) {
                const mappedItems = response.data.map(
                    (item: { keyProperties: any }) => ({
                        keyProperties: item.keyProperties,
                    }),
                ) as InventoryItem[];
                this.inventoryItems = clone(mappedItems);
            }
        },
        async getStaticInventoryHTML(reportType: string, filterData: any) {
            await InventoryService.getStaticInventoryHTML(
                reportType,
                filterData,
            ).then((response: AxiosResponse<string>) => {
                if (hasValue(response, 'data')) {
                    this.staticHTMLForInventory = response.data;
                }
            });
        },
        async getQuery(querySet: InventoryParam[]) {
            await InventoryService.getQuery(querySet).then(
                (response: AxiosResponse<InventoryParam[]>) => {
                    if (hasValue(response, 'data')) {
                        this.querySet = clone(response.data);
                    }
                },
            );
        },
    },
    getters: {
        getInventoryItems: (state) => state.inventoryItems,
        getStaticHTMLForInventory: (state) => state.staticHTMLForInventory,
        getQuerySet: (state) => state.querySet
    }
});