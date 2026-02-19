import { AxiosPromise } from 'axios';
import { API, coreAxiosInstance } from '@/shared/utils/axios-instance';
import { ViewerDataRequest } from '@/shared/models/iAM/viewer';

export default class ViewerService {
    static getMetadata(simulationId: string): AxiosPromise {
        return coreAxiosInstance.get(
            `${API.Report}/ViewerMetadata/${simulationId}`,
        );
    }

    static getComponentData(
        schemaId: string,
        componentId: string,
        request: ViewerDataRequest,
    ): AxiosPromise {
        return coreAxiosInstance.post(
            `${API.Report}/ViewerData/${schemaId}/${componentId}`,
            request,
        );
    }
}
