import {AxiosResponse} from 'axios';
import AdminSiteSettingsService from '@/services/admin-site-settings.service';
import {hasValue} from '@/shared/utils/has-value-util';

const state = {
    implementationName: '',
    adminContactEmail: '',
    isSuccessfulImport: false as boolean,
};

const mutations = {
    implementationNameMutator(state: any, implementationName: String) {
        state.implementationName = implementationName;
    },
    setAdminContactEmailMutator(state: any, adminContactEmail: string) {
        state.adminContactEmail = adminContactEmail;
    },
    isSuccessfulImportMutator(state: any, isSuccessful: boolean) {
        state.isSuccessfulImport = isSuccessful;
    }
};

const actions = {
    async getImplementationName({commit}: any) {
        await AdminSiteSettingsService.getImplementationName()
        .then((response: AxiosResponse<string>) => {
            if (hasValue(response, 'data')) {
                commit('implementationNameMutator', response.data);
            }
        });
    },
    async importImplementationName({commit, dispatch}: any, payload: any) {
        await AdminSiteSettingsService.importImplementationName(payload)
        .then((response: AxiosResponse) => {
            if (response.status >= 200 && response.status < 300) {
                const payloadString = payload.toString();
    
                commit('implementationNameMutator', payloadString);
                commit('isSuccessfulImportMutator', true);
                dispatch('addSuccessNotification', {
                  message: 'Implementation Name imported'
                });
            }
        });
    },
    async getAdminContactEmail({commit}: any) {
        const response = await AdminSiteSettingsService.getAdminContactEmail();
        if (response.status === 200) {
            commit('setAdminContactEmailMutator', response.data);
        }
    },
    async setAdminContactEmail({commit, dispatch}: any, email: string) {
        try {
            const response = await AdminSiteSettingsService.setAdminContactEmail(email)
            .then((response: AxiosResponse) => {
                if (response.status >= 200 && response.status < 300) {
                    commit('setAdminContactEmailMutator', email); 
                    dispatch('addSuccessNotification',{
                        message: 'Administrator email saved.'
                    }); 
                }
            })
        } catch (error) {
            console.error('Failed to save admin contact email', error);
        }
    }
};

const getters = {};

export default {
    state,
    getters,
    actions,
    mutations
};
