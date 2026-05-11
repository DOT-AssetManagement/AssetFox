import { SecurityTypes } from '@/shared/utils/security-types';
import { clearRefreshIntervalID } from '@/shared/utils/refresh-interval-id';
import { UnsecuredRoutePathNames } from '@/shared/utils/route-paths';
import AuthenticationService from '@/services/authentication.service';
import { hasValue } from './has-value-util';
import { UserInfoLocal } from '../models/iAM/authentication';
import pinia, {
    useAuthenticationStore,
    useAzureB2CStore,
    useConfigStore,
    useNotificationStore,
    useUserStore,
} from '@/stores';

function getAuthenticationStore() {
    return useAuthenticationStore(pinia);
}

function getAzureB2CStore() {
    return useAzureB2CStore(pinia);
}

function getConfigStore() {
    return useConfigStore(pinia);
}

function getNotificationStore() {
    return useNotificationStore(pinia);
}

function getUserStore() {
    return useUserStore(pinia);
}

const isAuthenticatedEsecUser = () => {
    const authenticationStore = getAuthenticationStore();
    const notificationStore = getNotificationStore();
    const userStore = getUserStore();

    return authenticationStore
        .checkBrowserTokens()
        .then(async () => {
            // Check user active status via API
            const activeStatus = await AuthenticationService.getActiveStatus();
            
            if (activeStatus.data == true) {
                // User is active, proceed with fetching user info and other data
                return authenticationStore.getUserInfo().then(() =>
                    userStore.getUserCriteriaFilter().then(() => {
                        if (authenticationStore.authenticated) {
                            return true;
                        } else {
                            throw new Error('Failed to authenticate');
                        }
                    })
                );
            } else {
                // User is not active, handle accordingly
                throw new Error('User is not active');
            }
        })
        .catch((error) => {
            notificationStore.addErrorNotification({
                message: 'Authentication Error.',
                longMessage: error,
            });

            return false;
        });
};

const isAuthenticatedAzureUser = () => {
    const authenticationStore = getAuthenticationStore();
    const azureB2CStore = getAzureB2CStore();
    const notificationStore = getNotificationStore();

    return azureB2CStore
        .getAzureB2CAccessToken()
        .then(() =>
            azureB2CStore.getAzureAccountDetails().then(() => {
                if (authenticationStore.authenticated) {
                    return true;
                } else {
                let isAccessDenied = authenticationStore.accessDenied;
                if(isAccessDenied == true)
                {
                    throw new Error('Failed to authenticate');
                }
                }
            }),
        )
        .catch((error: any) => {
            notificationStore.addErrorNotification({
                message: 'Authenticaton Error.',
                longMessage: error,
            });

            return false;
        });
};

const isAuthenticatedLocalDebugUserUser = () => {
    const authenticationStore = getAuthenticationStore();
    const configStore = getConfigStore();

    if(hasValue(localStorage.getItem('LoggedInUser'))){
        var config = configStore.getConfig;
        if (!config) {
            return Promise.resolve(false);
        }

        var payload = config.localDegugInfo as UserInfoLocal
        return Promise.resolve(authenticationStore.localDebugLogin(payload)).then(() => true)
    }
    return Promise.resolve(false);
};

export const isAuthenticatedUser = () => {
    const authenticationStore = getAuthenticationStore();

    if (authenticationStore.securityType === SecurityTypes.esec) {
        return isAuthenticatedEsecUser();
    }
    if (authenticationStore.securityType === SecurityTypes.b2c) {
        return isAuthenticatedAzureUser();
    }

    return isAuthenticatedLocalDebugUserUser();
};

const onLogout = () => {
    const authenticationStore = getAuthenticationStore();

    authenticationStore.logOut().then(() => {
        clearRefreshIntervalID(); 
        localStorage.clear();
        sessionStorage.clear();      
    });
};

const onAzureLogout = () => {
    const azureB2CStore = getAzureB2CStore();

    azureB2CStore.azureB2CLogout().then(() => onLogout());
};

export const onHandleLogout = () => {
    const authenticationStore = getAuthenticationStore();

    if (
        authenticationStore.securityType === SecurityTypes.esec
    ) {
        onLogout();
    } else {
        onAzureLogout();
    }
};
