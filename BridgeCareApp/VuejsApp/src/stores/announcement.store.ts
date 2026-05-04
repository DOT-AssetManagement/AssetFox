import { defineStore } from 'pinia';
import { any, clone, findIndex, prepend, propEq, reject, update } from 'ramda';
import { AxiosResponse } from 'axios';
import { Announcement } from '@/shared/models/iAM/announcement';
import AnnouncementService from '@/services/announcement.service';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import { useNotificationStore } from './notification.store';

export interface AnnouncementStoreState {
    announcements: Announcement[];
    packageVersion: string;
}

export interface UpsertAnnouncementPayload {
    announcement: Announcement;
}

export interface DeleteAnnouncementPayload {
    deletedAnnouncementId: string;
}

export const useAnnouncementStore = defineStore('announcement', {
    state: (): AnnouncementStoreState => ({
        announcements: [] as Announcement[],
        packageVersion: import.meta.env.PACKAGE_VERSION || '0',
    }),
    actions: {
        async getAnnouncements() {
            await AnnouncementService.getAnnouncements().then(
                (response: AxiosResponse<any[]>) => {
                    if (hasValue(response, 'data')) {
                        this.announcements = clone(response.data as Announcement[]);
                    }
                },
            );
        },
        async upsertAnnouncement(payload: UpsertAnnouncementPayload) {
            const notificationStore = useNotificationStore();
            await AnnouncementService.upsertAnnouncement(payload.announcement).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        const message: string = any(propEq('id', payload.announcement.id), this.announcements)
                         ? 'Updated announcement' : 'Added announcement';

                        this.announcements = any(
                            propEq('id', payload.announcement.id),this.announcements)
                            ? update(
                                findIndex(
                                    propEq('id', payload.announcement.id),
                                    this.announcements),
                                payload.announcement,
                                this.announcements) : prepend(payload.announcement, this.announcements);

                        notificationStore.addSuccessNotification({ message });
                    }
                },
            );
        },
        async deleteAnnouncement(payload: DeleteAnnouncementPayload) {
            const notificationStore = useNotificationStore();
            await AnnouncementService.deleteAnnouncement(
                payload.deletedAnnouncementId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    if (
                        any(
                            propEq('id', payload.deletedAnnouncementId),this.announcements)) {
                        this.announcements = reject(
                            (announcement: Announcement) => announcement.id === payload.deletedAnnouncementId,
                            this.announcements);
                    }
                    notificationStore.addSuccessNotification({
                        message: 'Deleted announcement',
                    });
                }
            });
        },
    },
});
