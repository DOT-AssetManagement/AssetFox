import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { UserInfo, UserInfoLocal, UserTokens } from '@/shared/models/iAM/authentication';
import { http2XX } from '@/shared/utils/http-utils';
import { parseLDAP } from '@/shared/utils/parse-ldap';
import { hasValue } from '@/shared/utils/has-value-util';
import moment from 'moment';
import AuthenticationService from '@/services/authentication.service';

export interface AuthenticationStoreState {
    authenticated: boolean;
    hasRole: boolean;
    checkedForRole: boolean;
    hasAdminAccess: boolean;
    hasSimulationAccess: boolean;
    username: string;
    refreshing: boolean;
    securityType: string;
    accessDenied: boolean;
}

const defaultTokenLifetimeSeconds = 30 * 60;
const tokenRefreshWindowMinutes = 15;
const tokenRefreshLockName = 'bridgecare-esec-token-refresh';
let refreshTokensPromise: Promise<void> | null = null;

function getTokenLifetimeSeconds(tokens: Partial<UserTokens>): number {
    const expiresInSeconds = Number(tokens.expires_in);
    return Number.isFinite(expiresInSeconds) && expiresInSeconds > 0
        ? expiresInSeconds
        : defaultTokenLifetimeSeconds;
}

function setTokenExpiration(tokens: Partial<UserTokens>) {
    const expirationInMilliseconds = moment()
        .add(getTokenLifetimeSeconds(tokens), 'seconds')
        .valueOf();
    localStorage.setItem(
        'TokenExpiration',
        expirationInMilliseconds.toString(),
    );
}

function browserTokensNeedRefresh(): boolean {
    const tokenExpirationValue = localStorage.getItem('TokenExpiration');
    if (!hasValue(tokenExpirationValue)) {
        throw new Error('Cannot determine user authentication status');
    }

    const storedTokenExpiration = Number(tokenExpirationValue);
    if (!Number.isFinite(storedTokenExpiration)) {
        throw new Error('Cannot determine user authentication status');
    }

    return (
        moment(storedTokenExpiration).diff(moment(), 'minutes') <=
        tokenRefreshWindowMinutes
    );
}

function resetAuthenticationState(state: AuthenticationStoreState) {
    state.hasRole = false;
    state.checkedForRole = false;
    state.hasAdminAccess = false;
    state.username = '';
    state.authenticated = false;
    state.hasSimulationAccess = false;
}

export const useAuthenticationStore = defineStore('authentication', {
    state: (): AuthenticationStoreState => ({
        authenticated: false,
        hasRole: false,
        checkedForRole: false,
        hasAdminAccess: false,
        hasSimulationAccess: false,
        username: '',
        refreshing: false,
        securityType: 'B2C',
        accessDenied: false,
    }),
    actions: {
        async getUserTokens(code: string) {
            await AuthenticationService.getUserTokens(code).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        const userTokens: UserTokens = response.data as UserTokens;
                        localStorage.setItem(
                            'UserTokens',
                            JSON.stringify(userTokens),
                        );
                        setTokenExpiration(userTokens);
                        this.authenticated = true;
                    }
                },
            )
            .catch(error => console.log(error));
        },

        async checkBrowserTokens() {
            if (!hasValue(localStorage.getItem('UserTokens'))) {
                throw new Error('Cannot determine user authentication status');
            }

            if (!browserTokensNeedRefresh()) {
                return;
            }
           
            await this.refreshTokens();
        },

        async refreshTokens() {
            if (refreshTokensPromise !== null) {
                return refreshTokensPromise;
            }

            const refresh = async () => {
                if (!hasValue(localStorage.getItem('UserTokens'))) {
                    throw new Error(
                        'Cannot determine user authentication status',
                    );
                }

                const userTokens: UserTokens = JSON.parse(
                    localStorage.getItem('UserTokens') as string,
                ) as UserTokens;
                if (!hasValue(userTokens.refresh_token)) {
                    throw new Error('No ESEC refresh token is available');
                }

                this.refreshing = true;
                try {
                    const response = await AuthenticationService.refreshTokens(
                        userTokens.refresh_token,
                    );
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        const refreshedTokens =
                            response.data as Partial<UserTokens>;
                        if (!hasValue(refreshedTokens, 'access_token')) {
                            throw new Error(
                                'The ESEC refresh response did not contain an access token',
                            );
                        }

                        const mergedTokens = {
                            ...userTokens,
                            ...refreshedTokens,
                        } as UserTokens;
                        if (!hasValue(refreshedTokens.refresh_token)) {
                            mergedTokens.refresh_token =
                                userTokens.refresh_token;
                        }
                        if (!hasValue(refreshedTokens.id_token)) {
                            mergedTokens.id_token = userTokens.id_token;
                        }
                        if (!hasValue(refreshedTokens.expires_in)) {
                            mergedTokens.expires_in = userTokens.expires_in;
                        }

                        localStorage.setItem(
                            'UserTokens',
                            JSON.stringify(mergedTokens),
                        );
                        setTokenExpiration(mergedTokens);
                    }
                } finally {
                    this.refreshing = false;
                }
            };

            const runRefresh = async () => {
                if (
                    typeof navigator !== 'undefined' &&
                    navigator.locks !== undefined
                ) {
                    await navigator.locks.request(
                        tokenRefreshLockName,
                        async () => {
                            // Another tab may have refreshed while this tab
                            // was waiting for the browser-wide lock.
                            if (!browserTokensNeedRefresh()) {
                                return;
                            }
                            await refresh();
                        },
                    );
                    return;
                }

                await refresh();
            };

            refreshTokensPromise = runRefresh().finally(() => {
                refreshTokensPromise = null;
            });
            return refreshTokensPromise;
        },

        async waitForTokenRefresh() {
            if (refreshTokensPromise !== null) {
                await refreshTokensPromise;
            }
        },

        async getUserInfo() {
            if (!hasValue(localStorage.getItem('UserTokens'))) {
                throw new Error('Cannot determine user authentication status');
            } else {
                const userTokens: UserTokens = JSON.parse(
                    localStorage.getItem('UserTokens') as string,
                ) as UserTokens;
                await AuthenticationService.getUserInfo(
                    userTokens.access_token,
                ).then((response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        const userInfo: UserInfo = response.data as UserInfo;
                        localStorage.setItem('UserInfo', JSON.stringify(userInfo));
                        this.username = parseLDAP(userInfo.sub)[0];

                        const hasRole: boolean = true;

                        this.checkedForRole = hasRole;
                        this.hasRole = hasRole;

                        if (hasRole) {
                            this.hasAdminAccess = userInfo.hasAdminAccess;
                            this.hasSimulationAccess = userInfo.hasSimulationAccess;
                        }

                        if (!this.authenticated) {
                            this.authenticated = true;
                        }
                    } else {
                        if (this.authenticated) {
                            this.authenticated = false;
                        }
                        throw new Error(
                            'Cannot determine user authentication status',
                        );
                    }
                });
            }
        },

        async logOut() {        
            this.username = '';
            this.authenticated = false;
            localStorage.removeItem('UserInfo');
            localStorage.removeItem('LoggedInUser');
            localStorage.removeItem('TokenExpiration');
            if (hasValue(localStorage.getItem('UserTokens'))) {
                await AuthenticationService.revokeIdToken();

                const userTokens: UserTokens = JSON.parse(
                    localStorage.getItem('UserTokens') as string,
                ) as UserTokens;

                localStorage.removeItem('UserTokens');

                await AuthenticationService.revokeToken(
                    userTokens.access_token,
                    'Access',
                );
                await AuthenticationService.revokeToken(
                    userTokens.refresh_token,
                    'Refresh',
                );
            }
        },
        async setAzureUserInfo(status: boolean, username: string) {
            if (status) {    
                var activeStatus = await AuthenticationService.getActiveStatus();
                if(activeStatus.data == true)
                    {
                        await AuthenticationService.getHasAdminAccess().then((response: AxiosResponse) => {
                            let hasAdminAccess: boolean = false;
                            if (hasValue(response, 'status') && http2XX.test(response.status.toString())) {
                                hasAdminAccess = response.data as boolean;
                            }
                                this.hasRole = true;
                                this.checkedForRole = true;
                                this.hasAdminAccess = hasAdminAccess;
                                this.username = username;
                                this.authenticated = true;
                                this.hasSimulationAccess = false;
                                this.accessDenied = true;
                        });
                    }
                    else
                    {
                        this.accessDenied = false;
                        resetAuthenticationState(this);
                    }
            } else {
                resetAuthenticationState(this);
            }
        },
        setSecurityType(payload: string) {
            this.securityType = payload;
        },
        localDebugLogin(payload: UserInfoLocal){
            var user = payload as UserInfoLocal;
            this.hasRole = true;
            this.checkedForRole = true;
            this.hasAdminAccess = user.hasAdminAccess;
            this.username = user.name;
            this.authenticated = true;
            this.hasSimulationAccess = user.hasSimulationAccess;
            
            localStorage.setItem('LoggedInUser', user.name);
        }
    },
    getters:{
        getAuthenticated: (state => state.authenticated),
        getHasRole: (state => state.hasRole),
        getCheckedForRole: (state => state.checkedForRole),
        getHasAdminAccess: (state => state.hasAdminAccess),
        getHasSimulationAccess: (state => state.hasSimulationAccess),
        getUsername: (state => state.username),
        getRefreshing: (state => state.refreshing),
        getSecurityType: (state => state.securityType),
        getAccessDenied: (state => state.accessDenied),
        pennDotSecurityType: (state => 'ESEC'),
        azureSecurityType: (state => 'B2C'),
        localDebugSecurityType: (state => 'LocalDebug'),
    }
});
