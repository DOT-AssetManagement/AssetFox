import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import AdminSiteSettingsService from '@/services/admin-site-settings.service';
import { hasValue } from '@/shared/utils/has-value-util';
import { useNotificationStore } from './notification.store';

export interface AdminSiteSettingsStoreState {
    productLogo: string;
    implementationName: string;
    adminContactEmail: string;
    isSuccessfulImport: boolean;
}

function convertFileToBase64(file: File): Promise<string> {
    return new Promise<string>((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => {
            const image = new Image();
            image.onload = () => {
                const targetHeight = 50;
                const ratio = image.width / image.height;
                const targetWidth = Math.round(targetHeight * ratio);

                const canvas = document.createElement('canvas');
                canvas.width = targetWidth;
                canvas.height = targetHeight;

                const context = canvas.getContext('2d');
                context?.drawImage(image, 0, 0, targetWidth, targetHeight);

                const resizedBase64 = canvas.toDataURL(file.type);
                resolve(resizedBase64);
            };
            image.onerror = (error) => reject(error);
            image.src = reader.result as string;
        };
        reader.onerror = (error) => reject(error);
        reader.readAsDataURL(file);
    });
}

export const useAdminSiteSettingsStore = defineStore('adminSiteSettings', {
    state: (): AdminSiteSettingsStoreState => ({
        productLogo: '',
        implementationName: '',
        adminContactEmail: '',
        isSuccessfulImport: false,
    }),
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
        async getProductLogo() {
            await AdminSiteSettingsService.getProductLogo().then(
                (response: AxiosResponse<string>) => {
                    if (hasValue(response, 'data')) {
                        this.productLogo = response.data;
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
        async importProductLogo(payload: File) {
            const notificationStore = useNotificationStore();
            await AdminSiteSettingsService.importProductLogo(payload).then(
                async (response: AxiosResponse) => {
                    if (response.status >= 200 && response.status < 300) {
                        const base64 = await convertFileToBase64(payload);
                        this.productLogo = base64;
                        this.isSuccessfulImport = true;
                        notificationStore.addSuccessNotification({
                            message: 'Product logo imported',
                        });
                    }
                },
            );
        },
        async convertAndStoreProductLogo(productLogo: File) {
            const base64 = await convertFileToBase64(productLogo);
            this.productLogo = base64;
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
