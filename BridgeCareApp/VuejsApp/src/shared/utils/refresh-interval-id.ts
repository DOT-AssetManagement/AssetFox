import { hasValue } from '@/shared/utils/has-value-util';
import pinia from '@/stores';
import { useAuthenticationStore } from '@/stores/authentication.store';
import { useNotificationStore } from '@/stores/notification.store';
import { onHandleLogout } from '@/shared/utils/authentication-utils';

let refreshIntervalID: any | null = null;

export const clearRefreshIntervalID = () => {
    if (hasValue(refreshIntervalID)) {
        window.clearInterval(refreshIntervalID);
    }
};

export const setRefreshIntervalID = () => {
    alert("in setRefreshIntervalID");
    if (!hasValue(refreshIntervalID)) {
        refreshIntervalID = window.setInterval(checkBrowserTokens, 30000);
    }
};

const checkBrowserTokens = () => {
    const authenticationStore = useAuthenticationStore(pinia);
    const notificationStore = useNotificationStore(pinia);

    authenticationStore.checkBrowserTokens().catch((err: any) => {
        alert(checkBrowserTokens + " " + err);
        notificationStore.addErrorNotification({
            message: 'Browser token error',
            longMessage: err.errorMessage,
        });
        onHandleLogout();
    });
};
