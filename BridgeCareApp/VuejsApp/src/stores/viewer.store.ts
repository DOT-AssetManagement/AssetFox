import { defineStore } from 'pinia';
import { clone } from 'ramda';
import ViewerService from '@/services/viewer.service';
import {
    ViewerMetadata,
    emptyViewerMetadata,
} from '@/shared/models/iAM/viewer';
import { hasValue } from '@/shared/utils/has-value-util';
import { useNotificationStore } from './notification.store';

export interface ViewerStoreState {
    viewerMetadata: ViewerMetadata;
    activeSchemaId: string;
    activeSimulationId: string;
    isLoadingMetadata: boolean;
}

export const useViewerStore = defineStore('viewer', {
    state: (): ViewerStoreState => ({
        viewerMetadata: clone(emptyViewerMetadata),
        activeSchemaId: '',
        activeSimulationId: '',
        isLoadingMetadata: false,
    }),
    getters: {
        viewerMetadataGetter: (state) => state.viewerMetadata,
        activeSchemaIdGetter: (state) => state.activeSchemaId,
        activeSimulationIdGetter: (state) => state.activeSimulationId,
        isLoadingMetadataGetter: (state) => state.isLoadingMetadata,
    },
    actions: {
        async loadViewerMetadata(simulationId: string ) {
            const notificationStore = useNotificationStore();
            this.isLoadingMetadata = true;
            try {
                const response = await ViewerService.getMetadata(
                    simulationId,
                );
                if (hasValue(response, 'data')) {
                    this.viewerMetadata = clone(response.data);
                    this.activeSimulationId = simulationId;
                }
            } catch (e: any) {
                notificationStore.addErrorNotification({
                    message: 'Failed to load viewer metadata: ' + (e.message || ''),
                });
            } finally {
                this.isLoadingMetadata = false;
            }
        },
        setActiveSchemaId(schemaId: string) {
            this.activeSchemaId = schemaId;
        },
        clearViewerState() {
            this.viewerMetadata = clone(emptyViewerMetadata);
            this.activeSchemaId = '';
            this.activeSimulationId = '';
        },
    },
});