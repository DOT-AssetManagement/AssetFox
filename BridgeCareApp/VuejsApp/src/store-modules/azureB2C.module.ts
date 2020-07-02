import {
    msalConfig,
    azureB2CConfig,
    msalConfigForPasswordReset,
    acquireTokenConfig,
} from '../config/azure-b2c-config';
import * as Msal from 'msal';

const state = {
    authenticatedFromAzure: false,
    app: new Msal.UserAgentApplication(msalConfig),
    appPasswordReset: new Msal.UserAgentApplication(msalConfigForPasswordReset),
};

const mutations = {
    authenticatedMutator(state: any, status: boolean) {
        state.authenticatedFromAzure = status;
    },
};

const actions = {
    async azureB2CLogin({ commit, dispatch }: any) {
        state.app.loginPopup().then(
            (token: any) => {
                var statusAndUser: any = {
                    status: true,
                    userName: token.account.name,
                };
                dispatch('getAuthenticationAzure', { statusAndUser });
                commit('authenticatedMutator', true);
            },
            (error: any) => {
                console.log('Login error ' + error);
                var statusAndUser: any = { status: false, userName: '' };
                dispatch('getAuthenticationAzure', { statusAndUser });
                //error handling
                if (error.errorMessage) {
                    // Check for forgot password error
                    if (error.errorMessage.indexOf('AADB2C90118') > -1) {
                        state.appPasswordReset
                            .loginPopup(azureB2CConfig.forgetPasswordAuthority)
                            .then((loginResponse: any) => {
                                console.log(loginResponse);
                                window.alert(
                                    'Password has been reset successfully. \nPlease sign-in with your new password.',
                                );
                            });
                    }
                }
            },
        );
    },

    async azureB2CLogout({ commit }: any) {
        state.app.logout();
    },

    async azureB2CgetToken({ commit, dispatch }: any, payload: any) {
        return state.app.acquireTokenSilent(payload.request).catch(error => {
            console.log(
                'Silent token acquisition fails. Acquiring token using popup',
            );
            console.log(error);
            // fallback to interaction when silent call fails
            return state.app
                .acquireTokenPopup(payload.request)
                .then(tokenResponse => {
                    console.log(
                        'access_token acquired at: ' + new Date().toString(),
                    );
                    return tokenResponse;
                })
                .catch(error => {
                    console.log(error);
                });
        });
    },

    async getAzureAccountDetails({ commit, dispatch }: any) {
        var accountDetails = state.app.getAccount();
        if (accountDetails) {
            var statusAndUser: any = {
                status: true,
                userName: accountDetails.name,
            };
            dispatch('getAuthenticationAzure', { statusAndUser });
        } 
        else{
            var statusAndUser: any = {
                status: false,
                userName: '',
            };
            dispatch('getAuthenticationAzure', { statusAndUser });
        }
    },
};

const getters = {};

export default {
    state,
    getters,
    actions,
    mutations,
};
