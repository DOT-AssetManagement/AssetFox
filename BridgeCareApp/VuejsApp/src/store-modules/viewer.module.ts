import { clone } from 'ramda';
import ViewerService from '@/services/viewer.service';
import {
    ViewerMetadata,
    emptyViewerMetadata,
} from '@/shared/models/iAM/viewer';
import { hasValue } from '@/shared/utils/has-value-util';

const state = {
    viewerMetadata: clone(emptyViewerMetadata) as ViewerMetadata,
    activeSchemaId: '' as string,
    activeSimulationId: '' as string,
    isLoadingMetadata: false as boolean,
};

const mutations = {
    viewerMetadataMutator(state: any, metadata: ViewerMetadata) {
        state.viewerMetadata = clone(metadata);
    },
    activeSchemaIdMutator(state: any, schemaId: string) {
        state.activeSchemaId = schemaId;
    },
    activeSimulationIdMutator(state: any, simulationId: string) {
        state.activeSimulationId = simulationId;
    },
    isLoadingMetadataMutator(state: any, isLoading: boolean) {
        state.isLoadingMetadata = isLoading;
    },
};

const actions = {
    async loadViewerMetadata(
        { commit, dispatch }: any,
        payload: { simulationId: string },
    ) {
        commit('isLoadingMetadataMutator', true);
        try {
            const response = await ViewerService.getMetadata(
                payload.simulationId,
            );
            if (hasValue(response, 'data')) {
                commit('viewerMetadataMutator', response.data);
                commit('activeSimulationIdMutator', payload.simulationId);
            }
        } catch (e: any) {
            dispatch(
                'addErrorNotification',
                'Failed to load viewer metadata: ' + (e.message || ''),
            );
        } finally {
            commit('isLoadingMetadataMutator', false);
        }
    },
    setActiveSchemaId({ commit }: any, payload: { schemaId: string }) {
        commit('activeSchemaIdMutator', payload.schemaId);
    },
    clearViewerState({ commit }: any) {
        commit('viewerMetadataMutator', clone(emptyViewerMetadata));
        commit('activeSchemaIdMutator', '');
        commit('activeSimulationIdMutator', '');
    },
};

const getters = {};

export default { state, getters, actions, mutations };
