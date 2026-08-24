import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import AdminSiteSettingsService from '@/services/admin-site-settings.service';
import { hasValue } from '@/shared/utils/has-value-util';
import { useNotificationStore } from './notification.store';

export interface AdminSiteSettingsStoreState {
    implementationName: string;
    adminContactEmail: string;
    isSuccessfulImport: boolean;
}

export const useAdminSiteSettingsStore = defineStore('adminSiteSettings', {
    state: (): AdminSiteSettingsStoreState => ({
        implementationName: '',
        adminContactEmail: '',
        isSuccessfulImport: false,
    }),
    getters: {
        implementationNameGetter: (state) => state.implementationName,
        adminContactEmailGetter: (state) => state.adminContactEmail,
        getIsSuccessfulImport: (state) => state.isSuccessfulImport,
    },
    actions: {
        async getImplementationName() {
            await AdminSiteSettingsService.getImplementationName().then(
                (response: AxiosResponse<string>) => {
                    if (hasValue(response, 'data')) {
                        this.implementationName = response.data;
                    }
                },
            );
        },
        async importImplementationName(payload: string) {
            const notificationStore = useNotificationStore();
            await AdminSiteSettingsService.importImplementationName(payload).then(
                (response: AxiosResponse) => {
                    if (response.status >= 200 && response.status < 300) {
                        const payloadString = payload.toString();

                        this.implementationName = payloadString;
                        this.isSuccessfulImport = true;
                        notificationStore.addSuccessNotification({
                            message: 'Implementation Name imported',
                        });
                    }
                },
            );
        },
        async getAdminContactEmail() {
            const response = await AdminSiteSettingsService.getAdminContactEmail();
            if (response.status === 200) {
                this.adminContactEmail = response.data;
            }
        },
        async setAdminContactEmail(email: string) {
            const notificationStore = useNotificationStore();
            try {
                await AdminSiteSettingsService.setAdminContactEmail(email).then(
                    (response: AxiosResponse) => {
                        if (response.status >= 200 && response.status < 300) {
                            this.adminContactEmail = email;
                            notificationStore.addSuccessNotification({
                                message: 'Administrator email saved.',
                            });
                        }
                    },
                );
            } catch (error) {
                console.error('Failed to save admin contact email', error);
            }
        },
    },
});
