import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { clone } from 'ramda';
import ReportSchemaService, {
    ReportSchemaPagingRequest,
    ReportSchemaSharingPayload,
} from '@/services/report-schema.service';
import {
    ReportSchemaDTO,
    emptyReportSchemaDTO,
} from '@/shared/models/iAM/report-schema';
import { PagingPage } from '@/shared/models/iAM/paging';
import { LibraryUser } from '@/shared/models/iAM/user';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import { useNotificationStore } from './notification.store';

export interface ReportSchemaStoreState {
    reportSchemas: ReportSchemaDTO[];
    reportSchemaPage: PagingPage<ReportSchemaDTO>;
    reportSchemaUsers: LibraryUser[];
    selectedReportSchema: ReportSchemaDTO;
    isDirty: boolean;
}

function updateReportSchemaInPage(
    state: Pick<ReportSchemaStoreState, 'reportSchemaPage'>,
    schema: ReportSchemaDTO,
) {
    const page = state.reportSchemaPage as PagingPage<ReportSchemaDTO>;
    if (!page?.items) {
        return;
    }
    state.reportSchemaPage = {
        ...page,
        items: page.items.map((item) => item.id === schema.id ? clone(schema) : item),
    };
}

export const useReportSchemaStore = defineStore('reportSchema', {
    state: (): ReportSchemaStoreState => ({
        reportSchemas: [],
        reportSchemaPage: { items: [], totalItems: 0 },
        reportSchemaUsers: [],
        selectedReportSchema: clone(emptyReportSchemaDTO),
        isDirty: false,
    }),
    getters: {
        reportSchemasGetter: (state) => state.reportSchemas,
        reportSchemaPageGetter: (state) => state.reportSchemaPage,
        reportSchemaUsersGetter: (state) => state.reportSchemaUsers,
        selectedReportSchemaGetter: (state) => state.selectedReportSchema,
        getIsDirty: (state) => state.isDirty,
    },
    actions: {
        selectedReportSchemaMutator(schema: ReportSchemaDTO) {
            this.selectedReportSchema = clone(schema);
        },
        isDirtyMutator(isDirty: boolean) {
            this.isDirty = isDirty;
        },
        async getReportSchemas() {
            await ReportSchemaService.getAll()
                .then((response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        this.reportSchemas = clone(response.data as ReportSchemaDTO[]);
                    }
                });
        },
        async getReportSchemaPage( request: ReportSchemaPagingRequest ) {
            return await ReportSchemaService.getPage(request)
                .then((response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        const page = response.data as PagingPage<ReportSchemaDTO>;
                        this.reportSchemaPage = clone(page);
                        return page;
                    }
                    return { items: [], totalItems: 0 } as PagingPage<ReportSchemaDTO>;
                });
        },
        async getReportSchema(id: string) {
            await ReportSchemaService.get(id)
                .then((response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        this.selectedReportSchema = clone(response.data as ReportSchemaDTO);
                    }
                });
        },
        async saveReportSchema(schema: ReportSchemaDTO) {
            const notificationStore = useNotificationStore();
            const isNew = !schema.id || schema.id === '00000000-0000-0000-0000-000000000000';
            const request = isNew
                ? ReportSchemaService.create(schema)
                : ReportSchemaService.update(schema.id, schema);

            return await request
                .then((response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        this.selectedReportSchema = clone(response.data as ReportSchemaDTO);
                        this.isDirty = false;
                        notificationStore.addSuccessNotification({ message: 'Report schema saved successfully.' });
                        this.getReportSchemas();
                        return response.data as ReportSchemaDTO;
                    }
                    return null;
                })
                .catch(() => {
                    notificationStore.addErrorNotification({ message: 'Failed to save report schema.' });
                    return null;
                });
        },
        async deleteReportSchema(id: string ) {
            const notificationStore = useNotificationStore();
            return await ReportSchemaService.delete(id)
                .then((response: AxiosResponse) => {
                    if (hasValue(response, 'status') && http2XX.test(response.status.toString())) {
                        notificationStore.addSuccessNotification({ message: 'Report schema deleted.' });
                        this.getReportSchemas();
                        return true;
                    }
                    return false;
                })
                .catch(() => {
                    notificationStore.addErrorNotification({ message: 'Failed to delete report schema.' });
                    return false;
                });
        },
        async getReportSchemaUsers(id: string) {
            return await ReportSchemaService.getUsers(id)
                .then((response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        const users = response.data as LibraryUser[];
                        this.reportSchemaUsers = clone(users);
                        return users;
                    }
                    return [] as LibraryUser[];
                });
        },
        async updateReportSchemaSharing(id: string, isSharedWithAll: boolean, users: LibraryUser[]) {
            const notificationStore = useNotificationStore();
            return await ReportSchemaService.updateSharing(id, {
                isSharedWithAll: isSharedWithAll,
                users: users,
            })
                .then((response: AxiosResponse) => {
                    if (hasValue(response, 'data')) {
                        const schema = response.data as ReportSchemaDTO;
                        this.selectedReportSchema = clone(schema);
                        updateReportSchemaInPage(this, schema);
                        notificationStore.addSuccessNotification({ message: 'Report template sharing updated.' });
                        this.getReportSchemas();
                        return schema;
                    }
                    return null;
                })
                .catch((error) => {
                    notificationStore.addErrorNotification({ message: 'Failed to update report template sharing.' });
                    throw error;
                });
        },
        async requestUniversalReportExport(schemaId: string, simulationId: string) {
            const notificationStore = useNotificationStore();
            return await ReportSchemaService.requestUniversalReportExport({ schemaId, simulationId })
                .then((response: AxiosResponse) => {
                    notificationStore.addSuccessNotification({ message: 'Excel export queued.' });
                    return response.data;
                })
                .catch((error) => {
                    notificationStore.addErrorNotification({ message: 'Failed to start Excel export.' });
                    throw error;
                });
        },
    },
});