import {
    msalConfig as b2cMsalConfig,
    acquireTokenConfig as b2cAcquireTokenConfig,
    forgotPasswordAuthority,
} from '@/azure-b2c-config';
import {
    msalConfig as entraMsalConfig,
    acquireTokenConfig as entraAcquireTokenConfig,
} from '@/entra-config';
import {
    AccountInfo,
    AuthenticationResult,
    Configuration,
    PopupRequest,
    PublicClientApplication,
} from '@azure/msal-browser';
import { SecurityTypes } from '@/shared/utils/security-types';
import { hasValue } from '@/shared/utils/has-value-util';
import store from '@/store/root-store';
import router from '@/router';

// B2C and Entra External ID share the same msal-browser runtime and differ only in their
// configuration. The active SecurityType comes from config.json (loaded into the store before
// the app mounts), so the MSAL instance is built lazily on first use rather than at module load.
function isEntra(): boolean {
    return store.state.authenticationModule.securityType === SecurityTypes.entra;
}

function activeMsalConfig(): Configuration {
    return isEntra() ? entraMsalConfig : b2cMsalConfig;
}

function activeAcquireTokenConfig(): PopupRequest {
    return isEntra() ? entraAcquireTokenConfig : b2cAcquireTokenConfig;
}

let msalInstance: PublicClientApplication | null = null;
function getMsalInstance(): PublicClientApplication {
    if (msalInstance === null) {
        msalInstance = new PublicClientApplication(activeMsalConfig());
        state.app = msalInstance;
    }
    return msalInstance;
}

// MSAL Browser v3+ must be initialized once before any login/token call.
let initialization: Promise<void> | null = null;
function ensureInitialized(): Promise<void> {
    const instance = getMsalInstance();
    if (initialization === null) {
        initialization = instance.initialize();
    }
    return initialization;
}

// Returns the signed-in account MSAL should use for silent token calls.
function getCurrentAccount(): AccountInfo | null {
    const instance = getMsalInstance();
    const active = instance.getActiveAccount();
    if (active !== null) {
        return active;
    }
    const all = instance.getAllAccounts();
    return all.length > 0 ? all[0] : null;
}

const state: any = {
    authenticatedFromAzure: false,
    app: null as PublicClientApplication | null,
};

const mutations = {
    authenticatedMutator(state: any, status: boolean) {
        state.authenticatedFromAzure = status;
    },
};

const actions = {
    async azureB2CLogin({ dispatch }: any) {
        await ensureInitialized();
        await getMsalInstance()
            .loginPopup(activeAcquireTokenConfig())
            .then((authResponse: AuthenticationResult) => {
                if (hasValue(authResponse.account)) {
                    getMsalInstance().setActiveAccount(authResponse.account);
                }
                if (
                    hasValue(authResponse.account) &&
                    hasValue(authResponse.account!.name)
                ) {
                    dispatch('getAzureB2CAccessToken', authResponse).then(() =>
                        dispatch('getAzureAccountDetails').then(async () => {
                            if (
                                // @ts-ignore
                                store.state.authenticationModule.authenticated
                            ) {
                                router.push('/Scenarios/');
                            }
                            else
                            {
                                // @ts-ignore
                                let isAccessDenied = store.state.authenticationModule.accessDenied;
                                if(isAccessDenied == false)
                                {
                                    router.push('/AccessDenied/');
                                }
                            }
                        }),
                    );
                }
            })
            .catch(async (error: any) => {
                if(error.response == undefined || error.response.status == 500)
                {
                    dispatch('addErrorNotification', {
                        message: 'The authorization system is not available at the moment.',
                        longMessage: error.errorMessage == undefined ? 'Unknown Error' : error.errorMessage
                    });
                }

                dispatch('getAzureAccountDetails');

                // B2C surfaces "forgot password" as the AADB2C90118 error and expects a
                // redirect to the separate password-reset policy authority. Entra External ID
                // handles password reset within its single sign-up/sign-in flow, so this
                // branch only applies to the B2C path.
                if (
                    !isEntra() &&
                    hasValue(error, 'errorMessage') &&
                    error.errorMessage.indexOf('AADB2C90118') !== -1
                ) {
                    await getMsalInstance()
                        .loginPopup({
                            ...activeAcquireTokenConfig(),
                            authority: forgotPasswordAuthority,
                        })
                        .then(() => {
                            dispatch('addSuccessNotification', {
                                message: 'Password Reset.',
                                longMessage:
                                    'Password has been reset successfully. Please sign-in with your new password.',
                            });
                        });
                }
            });
    },
    async azureB2CLogout() {
        await ensureInitialized();
        localStorage.removeItem('access_token');
        localStorage.removeItem('LoggedInUser');
        // Use popup logout to match the popup login flow. A redirect logout sets
        // MSAL's "interaction_in_progress" flag and requires handleRedirectPromise()
        // on return to clear it; the popup flow completes in-page and avoids the
        // stuck flag that otherwise blocks the next login.
        await getMsalInstance().logoutPopup();
    },
    async getAzureB2CAccessToken(
        { commit, dispatch }: any,
        authResponse?: AuthenticationResult,
    ) {
        await ensureInitialized();
        if (hasValue(authResponse) && hasValue(authResponse!.accessToken)) {
            localStorage.setItem('access_token', authResponse!.accessToken);
            return;
        }

        const account = getCurrentAccount();
        if (account !== null) {
            try {
                const silentResp = await getMsalInstance().acquireTokenSilent({
                    ...activeAcquireTokenConfig(),
                    account,
                });
                if (hasValue(silentResp.accessToken)) {
                    localStorage.setItem('access_token', silentResp.accessToken);
                    return;
                }
            } catch (error) {
                console.log(
                    'Silent token acquisition fail. Acquiring token using popup.',
                );
                console.log(`error: ${error}`);
            }
        }

        const popupResp = await getMsalInstance().acquireTokenPopup(activeAcquireTokenConfig());
        if (hasValue(popupResp.accessToken)) {
            localStorage.setItem('access_token', popupResp.accessToken);
        } else {
            throw new Error('Could not authenticate user');
        }
    },
    async getAzureAccountDetails({ dispatch }: any) {
        await ensureInitialized();
        const accountDetails = getCurrentAccount();
        if (hasValue(accountDetails) && hasValue(accountDetails!.name)) {
            localStorage.setItem('LoggedInUser', accountDetails!.name as string);
           await dispatch('setAzureUserInfo', {
                status: true,
                username: accountDetails!.name,
            });
        } else {
            localStorage.removeItem('LoggedInUser');
           await dispatch('setAzureUserInfo', {
                status: false,
                username: '',
            });
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
