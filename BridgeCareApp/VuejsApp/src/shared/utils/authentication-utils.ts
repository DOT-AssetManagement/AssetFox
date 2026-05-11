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

const authenticationStore = useAuthenticationStore(pinia);
const azureB2CStore = useAzureB2CStore(pinia);
const configStore = useConfigStore(pinia);
const notificationStore = useNotificationStore(pinia);
const userStore = useUserStore(pinia);

const isAuthenticatedEsecUser = () => {
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
    if(hasValue(localStorage.getItem('LoggedInUser'))){
        var config = configStore.config;
        if (!config) {
            return Promise.resolve(false);
        }

        var payload = config.localDegugInfo as UserInfoLocal
        return Promise.resolve(authenticationStore.localDebugLogin(payload)).then(() => true)
    }
    return Promise.resolve(false);
};

export const isAuthenticatedUser = () => {
    if (authenticationStore.securityType === SecurityTypes.esec) {
        return isAuthenticatedEsecUser();
    }
    if (authenticationStore.securityType === SecurityTypes.b2c) {
        return isAuthenticatedAzureUser();
    }

    return isAuthenticatedLocalDebugUserUser();
};

const onLogout = () => {
    authenticationStore.logOut().then(() => {
        clearRefreshIntervalID(); 
        localStorage.clear();
        sessionStorage.clear();      
    });
};

const onAzureLogout = () => {
    azureB2CStore.azureB2CLogout().then(() => onLogout());
};

export const onHandleLogout = () => {
    if (
        authenticationStore.securityType === SecurityTypes.esec
    ) {
        onLogout();
    } else {
        onAzureLogout();
    }
};
