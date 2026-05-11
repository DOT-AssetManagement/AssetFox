import { defineStore } from 'pinia';
import * as msal from 'msal';
import {
    acquireTokenConfig,
    azureB2CConfig,
    msalConfig,
    msalPasswordResetConfig,
} from '@/azure-b2c-config';
import { hasValue } from '@/shared/utils/has-value-util';
import router from '@/router';
import { useAuthenticationStore } from './authentication.store';
import { useNotificationStore } from './notification.store';

export interface AzureB2CStoreState {
    authenticatedFromAzure: boolean;
    app: msal.UserAgentApplication;
    appPasswordReset: msal.UserAgentApplication;
}

export const useAzureB2CStore = defineStore('azureB2C', {
    state: (): AzureB2CStoreState => ({
        authenticatedFromAzure: false,
        app: new msal.UserAgentApplication(msalConfig),
        appPasswordReset: new msal.UserAgentApplication(msalPasswordResetConfig),
    }),
    actions: {
        setAuthenticatedFromAzure(status: boolean) {
            this.authenticatedFromAzure = status;
        },
        async azureB2CLogin() {
            const authenticationStore = useAuthenticationStore();
            const notificationStore = useNotificationStore();

            await this.app
                .loginPopup()
                .then((authResponse: msal.AuthResponse) => {
                    if (
                        hasValue(authResponse.account) &&
                        hasValue(authResponse.account.name)
                    ) {
                        this.getAzureB2CAccessToken(authResponse).then(() =>
                            this.getAzureAccountDetails().then(async () => {
                                if (authenticationStore.authenticated) {
                                    router.push('/Scenarios/');
                                } else {
                                    let isAccessDenied = authenticationStore.accessDenied;
                                    if (isAccessDenied == false) {
                                        router.push('/AccessDenied/');
                                    }
                                }
                            }),
                        );
                    }
                })
                .catch(async (error: any) => {
                    if (error.response == undefined || error.response.status == 500) {
                        notificationStore.addErrorNotification({
                            message: 'The authorization system is not available at the moment.',
                            longMessage:
                                error.errorMessage == undefined
                                    ? 'Unknown Error'
                                    : error.errorMessage,
                        });
                    }

                    this.getAzureAccountDetails();

                    if (
                        hasValue(error, 'errorMessage') &&
                        error.errorMessage.indexOf('AADB2C90118') !== -1
                    ) {
                        await this.appPasswordReset
                            .loginPopup(azureB2CConfig.forgotPasswordAuthority)
                            .then(() => {
                                notificationStore.addSuccessNotification({
                                    message: 'Password Reset.',
                                    longMessage:
                                        'Password has been reset successfully. Please sign-in with your new password.',
                                });
                            });
                    }
                });
        },
        async azureB2CLogout() {
            localStorage.removeItem('access_token');
            localStorage.removeItem('LoggedInUser');
            this.app.logout();
        },
        async getAzureB2CAccessToken(authResponse?: msal.AuthResponse) {
            if (hasValue(authResponse) && hasValue(authResponse!.accessToken)) {
                localStorage.setItem('access_token', authResponse!.accessToken);
            } else {
                await this.app
                    .acquireTokenSilent(acquireTokenConfig)
                    .then((silentResp: msal.AuthResponse) => {
                        if (hasValue(silentResp.accessToken)) {
                            localStorage.setItem(
                                'access_token',
                                silentResp.accessToken,
                            );
                        }
                    })
                    .catch(async (error: any) => {
                        console.log(
                            'Silent token acquisition fail. Acquiring token using popup.',
                        );
                        console.log(`error: ${error}`);

                        await this.app
                            .acquireTokenPopup(acquireTokenConfig)
                            .then((popupResp: msal.AuthResponse) => {
                                if (hasValue(popupResp.accessToken)) {
                                    localStorage.setItem(
                                        'access_token',
                                        popupResp.accessToken,
                                    );
                                } else {
                                    throw new Error('Could not authenticate user');
                                }
                            })
                            .catch((popupError: any) => {
                                throw new Error(popupError);
                            });
                    });
            }
        },
        async getAzureAccountDetails() {
            const authenticationStore = useAuthenticationStore();
            const accountDetails: msal.Account = await this.app.getAccount();
            if (hasValue(accountDetails.name)) {
                localStorage.setItem('LoggedInUser', accountDetails.name);
                await authenticationStore.setAzureUserInfo(
                    true,
                    accountDetails.name,
                );
            } else {
                localStorage.removeItem('LoggedInUser');
                await authenticationStore.setAzureUserInfo(false, '');
            }
        },
    },
});
