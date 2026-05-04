import { AxiosPromise } from 'axios';
import { API, coreAxiosInstance } from '@/shared/utils/axios-instance';
import { ReportSchemaDTO } from '@/shared/models/iAM/report-schema';

export default class ReportSchemaService {
    static getAll(): AxiosPromise {
        return coreAxiosInstance.get(`${API.ReportSchema}`);
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

    static getDefaults(): AxiosPromise {
        return coreAxiosInstance.get(`${API.ReportSchema}/defaults`);
    }

    static requestUniversalReportExport(payload: { schemaId: string; simulationId: string }): AxiosPromise {
        return coreAxiosInstance.post(`${API.Report}/universal/export`, payload);
    }
}
