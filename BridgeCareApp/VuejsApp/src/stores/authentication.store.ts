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
    pennDotSecurityType: string;
    azureSecurityType: string;
    localDebugSecurityType: string;
    accessDenied: boolean;
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
        pennDotSecurityType: 'ESEC',
        azureSecurityType: 'B2C',
        localDebugSecurityType:'LocalDebug',
        accessDenied: false,
    }),
    actions: {
        async getUserTokens(code: string) {
            await AuthenticationService.getUserTokens(code).then(
                (response: AxiosResponse) => {
                    const expirationInMilliseconds = moment().add(30, 'minutes');                
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        const userTokens: UserTokens = response.data as UserTokens;
                        localStorage.setItem(
                            'UserTokens',
                            JSON.stringify(userTokens),
                        );
                        localStorage.setItem(
                            'TokenExpiration',
                            expirationInMilliseconds.valueOf().toString(),
                        );
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

            const storedTokenExpiration: number = parseInt(
                localStorage.getItem('TokenExpiration') as string,
            );
            if (isNaN(storedTokenExpiration)) {
                throw new Error('Cannot determine user authentication status');
            }

            const currentDateTime = moment();
            const tokenExpirationDateTime = moment(storedTokenExpiration);
            const differenceInMinutes = tokenExpirationDateTime.diff(
                currentDateTime,
                'minutes',
            );
            
            if (differenceInMinutes > 15) {
                return;
            }
           
            await this.refreshTokens();
        },

// TODO check with ESEC not B2C
// On component load, check the token's time stamp, and set a delayed call to refresh the token when less than 5 minutes remain.
// TODO try checking and renwing interval in logOut? Or atleast on each component load in router.ts
        async refreshTokens() {
            if (!hasValue(localStorage.getItem('UserTokens'))) {
                throw new Error('Cannot determine user authentication status');
            } else {
                this.refreshing = true;
                const userTokens: UserTokens = JSON.parse(
                    localStorage.getItem('UserTokens') as string,
                ) as UserTokens;            
                await AuthenticationService.refreshTokens(
                    userTokens.refresh_token,
                ).then((response: AxiosResponse) => {
                    const expirationInMilliseconds = moment().add(30, 'minutes');
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        const refreshedTokens: UserTokens = response.data as UserTokens;
                        localStorage.setItem(
                            'UserTokens',
                            JSON.stringify(refreshedTokens),
                        );
                        localStorage.setItem(
                            'TokenExpiration',
                            expirationInMilliseconds.valueOf().toString(),
                        );
                    }
                });
                this.refreshing = false;
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
});
