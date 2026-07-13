import { AxiosPromise } from 'axios';
import { API, coreAxiosInstance } from '@/shared/utils/axios-instance';
import { ReportCalculatedField, ReportSchemaDTO } from '@/shared/models/iAM/report-schema';
import { PagingRequest } from '@/shared/models/iAM/paging';
import { LibraryUser } from '@/shared/models/iAM/user';

export interface ReportSchemaPagingRequest extends PagingRequest<ReportSchemaDTO> {
    scope: string;
}

export interface ReportSchemaSharingPayload {
    isSharedWithAll: boolean;
    users: LibraryUser[];
}

export default class ReportSchemaService {
    static getAll(): AxiosPromise {
        return coreAxiosInstance.get(`${API.ReportSchema}`);
    }

    static getPage(request: ReportSchemaPagingRequest): AxiosPromise {
        return coreAxiosInstance.post(`${API.ReportSchema}/page`, request);
    }

    static get(id: string): AxiosPromise {
        return coreAxiosInstance.get(`${API.ReportSchema}/${id}`);
    }

    static create(dto: ReportSchemaDTO): AxiosPromise {
        return coreAxiosInstance.post(`${API.ReportSchema}`, dto);
    }

    static update(id: string, dto: ReportSchemaDTO): AxiosPromise {
        return coreAxiosInstance.put(`${API.ReportSchema}/${id}`, dto);
    }

    static delete(id: string): AxiosPromise {
        return coreAxiosInstance.delete(`${API.ReportSchema}/${id}`);
    }

    static getUsers(id: string): AxiosPromise {
        return coreAxiosInstance.get(`${API.ReportSchema}/${id}/users`);
    }

    static updateSharing(id: string, payload: ReportSchemaSharingPayload): AxiosPromise {
        return coreAxiosInstance.put(`${API.ReportSchema}/${id}/sharing`, payload);
    }

    static getDefaults(): AxiosPromise {
        return coreAxiosInstance.get(`${API.ReportSchema}/defaults`);
    }

    static requestUniversalReportExport(payload: { schemaId: string; simulationId: string }): AxiosPromise {
        return coreAxiosInstance.post(`${API.Report}/universal/export`, payload);
    }

    static validateCalculatedFields(fields: ReportCalculatedField[]): AxiosPromise {
        return coreAxiosInstance.post(`${API.ReportSchema}/validate-calculated-fields`, fields);
    }
}
