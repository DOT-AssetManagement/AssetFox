import { defineStore } from 'pinia';
import { Notification } from '@/shared/models/iAM/notifications';

export interface NotificationPayload {
    message: string;
    longMessage?: string;
    stackTrace?: string;
}

export interface NotificationStoreState {
    notifications: Notification[];
    counter: number;
    totalNotifications: number;
}

function createNotification(
    id: number,
    payload: NotificationPayload,
    icon: string,
    iconColor: string,
): Notification {
    return {
        id,
        icon,
        iconColor,
        active: false,
        shortMessage: payload.message,
        longMessage: payload.longMessage || 'No further information provided.',
        stackTrace: payload.stackTrace,
    };
}

export const useNotificationStore = defineStore('notification', {
    state: (): NotificationStoreState => ({
        notifications: [],
        counter: 0,
        totalNotifications: 0,
    }),
    actions: {
        addNotification(notification: Notification) {
            this.notifications.unshift(notification);
            this.counter += 1;
            this.totalNotifications += 1;
        },
        addSuccessNotification(payload: NotificationPayload) {
            this.addNotification(
                createNotification(
                    this.totalNotifications + 1,
                    payload,
                    'fas fa-check-circle',
                    'green',
                ),
            );
        },
        addWarningNotification(payload: NotificationPayload) {
            this.addNotification(
                createNotification(
                    this.totalNotifications + 1,
                    payload,
                    'fas fa-exclamation-circle',
                    'yellow',
                ),
            );
        },
        addErrorNotification(payload: NotificationPayload) {
            this.addNotification(
                createNotification(
                    this.totalNotifications + 1,
                    payload,
                    'fas fa-exclamation-circle',
                    'red',
                ),
            );
        },
        addErrorNotificationWithStackTrace(payload: NotificationPayload) {
            this.addNotification(
                createNotification(
                    this.totalNotifications + 1,
                    payload,
                    'fas fa-exclamation-circle',
                    'red',
                ),
            );
        },
        addInfoNotification(payload: NotificationPayload) {
            this.addNotification(
                createNotification(
                    this.totalNotifications + 1,
                    payload,
                    'fas fa-info-circle',
                    'primary',
                ),
            );
        },
        addTaskCompletedNotification(payload: NotificationPayload) {
            this.addNotification(
                createNotification(
                    this.totalNotifications + 1,
                    payload,
                    'fas fa-exclamation-circle',
                    'green',
                ),
            );
        },
        removeNotification(id: number) {
            this.notifications = this.notifications.filter(
                (notification) => notification.id !== id,
            );
        },
        clearNotificationCounter() {
            this.counter = 0;
        },
    },
    getters: {        
        getNotifications: (state) => state.notifications,
        getCounter: (state) => state.counter,
        getTotalNotifications: (state) => state.totalNotifications
    }
});
