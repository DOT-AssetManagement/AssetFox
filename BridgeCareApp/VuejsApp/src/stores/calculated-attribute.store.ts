import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, propEq, reject, update } from 'ramda';
import CalculatedAttributeService from '@/services/calculated-attribute.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import { Attribute } from '@/shared/models/iAM/attribute';
import {
    CalculatedAttribute,
    CalculatedAttributeLibrary,
    emptyCalculatedAttributeLibrary,
} from '@/shared/models/iAM/calculated-attribute';

export interface CalculatedAttributeStoreState {
    calculatedAttributeLibraries: CalculatedAttributeLibrary[];
    selectedCalculatedAttributeLibrary: CalculatedAttributeLibrary;
    scenarioCalculatedAttributes: CalculatedAttribute[];
    selectedLibraryCalculatedAttributes: CalculatedAttribute[];
    calculatedAttributes: Attribute[];
    isSharedLibrary: boolean;
}

export const useCalculatedAttributeStore = defineStore('calculatedAttribute', {
    state: (): CalculatedAttributeStoreState => ({
        calculatedAttributeLibraries: [],
        selectedCalculatedAttributeLibrary: clone(emptyCalculatedAttributeLibrary),
        scenarioCalculatedAttributes: [],
        selectedLibraryCalculatedAttributes: [],
        calculatedAttributes: [],
        isSharedLibrary: false,
    }),
    actions: {
        calculatedAttributeLibraryMutator(library: CalculatedAttributeLibrary) {
            this.calculatedAttributeLibraries = any(
                propEq('id', library.id),
                this.calculatedAttributeLibraries,
            )
                ? (update(
                      findIndex(propEq('id', library.id), this.calculatedAttributeLibraries),
                      library,
                      this.calculatedAttributeLibraries,
                  ) as CalculatedAttributeLibrary[])
                : (append(library, this.calculatedAttributeLibraries) as CalculatedAttributeLibrary[]);
        },
        selectedCalculatedAttributeLibraryMutator(libraryId: string) {
            this.selectedCalculatedAttributeLibrary = any(
                propEq('id', libraryId),
                this.calculatedAttributeLibraries,
            )
                ? (find(
                      propEq('id', libraryId),
                      this.calculatedAttributeLibraries,
                  ) as CalculatedAttributeLibrary)
                : clone(emptyCalculatedAttributeLibrary);
        },
        selectCalculatedAttributeLibrary(libraryId: string) {
            this.selectedCalculatedAttributeLibraryMutator(libraryId);
        },
        async getCalculatedAttributes() {
            await CalculatedAttributeService.getCalculatedAttributes().then(
                (response: AxiosResponse<Attribute[]>) => {
                    if (hasValue(response, 'data')) {
                        this.calculatedAttributes = clone(response.data);
                    }
                },
            );
        },
        async getCalculatedAttributeLibraries() {
            await CalculatedAttributeService.getCalculatedAttributeLibraries().then(
                (response: AxiosResponse<CalculatedAttributeLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.calculatedAttributeLibraries = clone(response.data);
                    }
                },
            );
        },
        async getScenarioCalculatedAttribute(scenarioId: string) {
            await CalculatedAttributeService.getEmptyCalculatedAttributesByScenarioId(
                scenarioId,
            ).then((response: AxiosResponse<CalculatedAttribute[]>) => {
                if (hasValue(response, 'data')) {
                    this.scenarioCalculatedAttributes = clone(response.data);
                }
            });
        },
        async getSelectedLibraryCalculatedAttributes(libraryId: string) {
            await CalculatedAttributeService.getEmptyCalculatedAttributesByLibraryId(
                libraryId,
            ).then((response: AxiosResponse<CalculatedAttribute[]>) => {
                if (hasValue(response, 'data')) {
                    this.selectedLibraryCalculatedAttributes = clone(response.data);
                }
            });
        },
        async deleteCalculatedAttributeLibrary(libraryId: string) {
            const notificationStore = useNotificationStore();
            await CalculatedAttributeService.deleteCalculatedAttributeLibrary(
                libraryId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.calculatedAttributeLibraries = reject(
                        propEq('id', libraryId),
                        this.calculatedAttributeLibraries,
                    ) as CalculatedAttributeLibrary[];
                    notificationStore.addSuccessNotification({
                        message: 'Deleted calculated attribute library',
                    });
                }
            });
        },
        async getIsSharedCalculatedAttributeLibrary(payload: { id: string }) {
            await CalculatedAttributeService.getIsSharedCalculatedAttributeLibrary(
                payload.id,
            ).then((response: AxiosResponse<boolean>) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.isSharedLibrary = response.data as boolean;
                }
            });
        },
    },
});
