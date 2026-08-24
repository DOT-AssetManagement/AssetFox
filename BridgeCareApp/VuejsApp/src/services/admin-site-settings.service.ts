import {AxiosPromise} from 'axios';
import {API, coreAxiosInstance} from '@/shared/utils/axios-instance';

export default class AdminSiteSettingsService {
    static getImplementationName(): AxiosPromise {
        return coreAxiosInstance.get(`${API.AdminSettings}/GetImplementationName`);
    }
    static importImplementationName(input: string): AxiosPromise {
        return coreAxiosInstance.post(
            `${API.AdminSettings}/SetImplementationName/` + input,
            input,
            {headers: {'Content-Type': 'text/plain'}},
        );
    }
    static getAdminContactEmail(): AxiosPromise {
        return coreAxiosInstance.get(`${API.AdminSettings}/GetAdminContactEmail`);
    }
    static setAdminContactEmail(email: string): AxiosPromise {
        return coreAxiosInstance.post(
          `${API.AdminSettings}/SetAdminContactEmail/` + encodeURIComponent(email),
          null,
          { headers: { 'Content-Type': 'application/json' } }
        );
    }
}
