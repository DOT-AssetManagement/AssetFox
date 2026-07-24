import { SecurityTypes } from '@/shared/utils/security-types';
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

const isAuthenticatedEsecUser = () => {
    const notificationStore = useNotificationStore(pinia);
    const authenticationStore = useAuthenticationStore(pinia);
    const userStore = useUserStore(pinia);
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
        .catch((error: any) => {
            const errorMessage =
                error?.response?.data?.message ??
                error?.message ??
                'Your ESEC session could not be renewed. Please sign in again.';
            notificationStore.addErrorNotification({
                message: 'Authentication Error.',
                longMessage: errorMessage,
            });

            return false;
        });
};

const isAuthenticatedAzureUser = () => {
    const notificationStore = useNotificationStore(pinia);
    const authenticationStore = useAuthenticationStore(pinia);
    const azureB2CStore = useAzureB2CStore(pinia);
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
    const configStore = useConfigStore(pinia);
    const authenticationStore = useAuthenticationStore(pinia);
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
    const authenticationStore = useAuthenticationStore(pinia);
    if (authenticationStore.securityType === SecurityTypes.esec) {
        return isAuthenticatedEsecUser();
    }
    if (authenticationStore.securityType === SecurityTypes.b2c) {
        return isAuthenticatedAzureUser();
    }

    return isAuthenticatedLocalDebugUserUser();
};

const onLogout = () => {
    const authenticationStore = useAuthenticationStore(pinia);
    authenticationStore.logOut().then(() => {
        localStorage.clear();
        sessionStorage.clear();      
    });
};

const onAzureLogout = () => {
    const azureB2CStore = useAzureB2CStore(pinia);
    azureB2CStore.azureB2CLogout().then(() => onLogout());
};

export const onHandleLogout = () => {
    const authenticationStore = useAuthenticationStore(pinia);
    if (
        authenticationStore.securityType === SecurityTypes.esec
    ) {
        onLogout();
    } else {
        onAzureLogout();
    }
};
