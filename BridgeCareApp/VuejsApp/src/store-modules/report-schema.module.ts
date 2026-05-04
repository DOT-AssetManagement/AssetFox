import { clone } from 'ramda';
import { AxiosResponse } from 'axios';
import ReportSchemaService from '@/services/report-schema.service';
import { ReportSchemaDTO, emptyReportSchemaDTO } from '@/shared/models/iAM/report-schema';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';

const state = {
    reportSchemas: [] as ReportSchemaDTO[],
    selectedReportSchema: clone(emptyReportSchemaDTO) as ReportSchemaDTO,
    isDirty: false as boolean,
};

const mutations = {
    reportSchemasMutator(state: any, schemas: ReportSchemaDTO[]) {
        state.reportSchemas = clone(schemas);
    },
    selectedReportSchemaMutator(state: any, schema: ReportSchemaDTO) {
        state.selectedReportSchema = clone(schema);
    },
    isDirtyMutator(state: any, isDirty: boolean) {
        state.isDirty = isDirty;
    },
};

const actions = {
    async getReportSchemas({ commit }: any) {
        await ReportSchemaService.getAll()
            .then((response: AxiosResponse) => {
                if (hasValue(response, 'data')) {
                    commit('reportSchemasMutator', response.data as ReportSchemaDTO[]);
                }
            });
    },
    async getReportSchema({ commit }: any, payload: { id: string }) {
        await ReportSchemaService.get(payload.id)
            .then((response: AxiosResponse) => {
                if (hasValue(response, 'data')) {
                    commit('selectedReportSchemaMutator', response.data as ReportSchemaDTO);
                }
            });
    },
    async saveReportSchema({ dispatch, commit }: any, payload: { schema: ReportSchemaDTO }) {
        const schema = payload.schema;
        const isNew = !schema.id || schema.id === '00000000-0000-0000-0000-000000000000';
        const request = isNew
            ? ReportSchemaService.create(schema)
            : ReportSchemaService.update(schema.id, schema);

        return await request
            .then((response: AxiosResponse) => {
                if (hasValue(response, 'data')) {
                    commit('selectedReportSchemaMutator', response.data as ReportSchemaDTO);
                    commit('isDirtyMutator', false);
                    dispatch('addSuccessNotification', { message: 'Report schema saved successfully.' });
                    dispatch('getReportSchemas');
                }
            })
            .catch(() => {
                dispatch('addErrorNotification', { message: 'Failed to save report schema.' });
            });
    },
    async deleteReportSchema({ dispatch }: any, payload: { id: string }) {
        return await ReportSchemaService.delete(payload.id)
            .then((response: AxiosResponse) => {
                if (hasValue(response, 'status') && http2XX.test(response.status.toString())) {
                    dispatch('addSuccessNotification', { message: 'Report schema deleted.' });
                    dispatch('getReportSchemas');
                }
            })
            .catch(() => {
                dispatch('addErrorNotification', { message: 'Failed to delete report schema.' });
            });
    },
    async requestUniversalReportExport(
        { dispatch }: any,
        payload: { schemaId: string; simulationId: string },
    ) {
        return await ReportSchemaService.requestUniversalReportExport(payload)
            .then((response: AxiosResponse) => {
                dispatch('addSuccessNotification', { message: 'Excel export queued.' });
                return response.data;
            })
            .catch((error) => {
                dispatch('addErrorNotification', { message: 'Failed to start Excel export.' });
                throw error;
            });
    },
};

const getters = {};

export default { state, getters, actions, mutations };
