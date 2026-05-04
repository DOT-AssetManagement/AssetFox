import { clone } from 'ramda';
import { AxiosResponse } from 'axios';
import ReportSchemaService, { ReportSchemaPagingRequest, ReportSchemaSharingPayload } from '@/services/report-schema.service';
import { ReportSchemaDTO, emptyReportSchemaDTO } from '@/shared/models/iAM/report-schema';
import { PagingPage } from '@/shared/models/iAM/paging';
import { LibraryUser } from '@/shared/models/iAM/user';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';

const state = {
    reportSchemas: [] as ReportSchemaDTO[],
    reportSchemaPage: { items: [], totalItems: 0 } as PagingPage<ReportSchemaDTO>,
    reportSchemaUsers: [] as LibraryUser[],
    selectedReportSchema: clone(emptyReportSchemaDTO) as ReportSchemaDTO,
    isDirty: false as boolean,
};

const mutations = {
    reportSchemasMutator(state: any, schemas: ReportSchemaDTO[]) {
        state.reportSchemas = clone(schemas);
    },
    reportSchemaPageMutator(state: any, page: PagingPage<ReportSchemaDTO>) {
        state.reportSchemaPage = clone(page);
    },
    reportSchemaUsersMutator(state: any, users: LibraryUser[]) {
        state.reportSchemaUsers = clone(users);
    },
    updateReportSchemaInPageMutator(state: any, schema: ReportSchemaDTO) {
        const page = state.reportSchemaPage as PagingPage<ReportSchemaDTO>;
        if (!page?.items) {
            return;
        }
        state.reportSchemaPage = {
            ...page,
            items: page.items.map((item) => item.id === schema.id ? clone(schema) : item),
        };
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
    async getReportSchemaPage({ commit }: any, payload: { request: ReportSchemaPagingRequest }) {
        return await ReportSchemaService.getPage(payload.request)
            .then((response: AxiosResponse) => {
                if (hasValue(response, 'data')) {
                    const page = response.data as PagingPage<ReportSchemaDTO>;
                    commit('reportSchemaPageMutator', page);
                    return page;
                }
                return { items: [], totalItems: 0 } as PagingPage<ReportSchemaDTO>;
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
                    return response.data as ReportSchemaDTO;
                }
                return null;
            })
            .catch(() => {
                dispatch('addErrorNotification', { message: 'Failed to save report schema.' });
                return null;
            });
    },
    async deleteReportSchema({ dispatch }: any, payload: { id: string }) {
        return await ReportSchemaService.delete(payload.id)
            .then((response: AxiosResponse) => {
                if (hasValue(response, 'status') && http2XX.test(response.status.toString())) {
                    dispatch('addSuccessNotification', { message: 'Report schema deleted.' });
                    dispatch('getReportSchemas');
                    return true;
                }
                return false;
            })
            .catch(() => {
                dispatch('addErrorNotification', { message: 'Failed to delete report schema.' });
                return false;
            });
    },
    async getReportSchemaUsers({ commit }: any, payload: { id: string }) {
        return await ReportSchemaService.getUsers(payload.id)
            .then((response: AxiosResponse) => {
                if (hasValue(response, 'data')) {
                    const users = response.data as LibraryUser[];
                    commit('reportSchemaUsersMutator', users);
                    return users;
                }
                return [] as LibraryUser[];
            });
    },
    async updateReportSchemaSharing(
        { dispatch, commit }: any,
        payload: { id: string } & ReportSchemaSharingPayload,
    ) {
        return await ReportSchemaService.updateSharing(payload.id, {
            isSharedWithAll: payload.isSharedWithAll,
            users: payload.users,
        })
            .then((response: AxiosResponse) => {
                if (hasValue(response, 'data')) {
                    const schema = response.data as ReportSchemaDTO;
                    commit('selectedReportSchemaMutator', schema);
                    commit('updateReportSchemaInPageMutator', schema);
                    dispatch('addSuccessNotification', { message: 'Report template sharing updated.' });
                    dispatch('getReportSchemas');
                    return schema;
                }
                return null;
            })
            .catch((error) => {
                dispatch('addErrorNotification', { message: 'Failed to update report template sharing.' });
                throw error;
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
