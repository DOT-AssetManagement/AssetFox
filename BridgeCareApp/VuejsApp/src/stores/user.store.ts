import { defineStore } from 'pinia';
import { User, UserNewsAccessDate, emptyUser } from '@/shared/models/iAM/user';
import { clone, findIndex, propEq, reject, update } from 'ramda';
import UserService from '@/services/user.service';
import { AxiosResponse } from 'axios';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import {
    emptyUserCriteriaFilter,
    UserCriteriaFilter,
} from '@/shared/models/iAM/user-criteria-filter';
import { useAuthenticationStore } from './authentication.store';
import { useNotificationStore } from './notification.store';

export interface UserStoreState {
    users: User[];
    usersCriteriaFilter: UserCriteriaFilter[];
    currentUserCriteriaFilter: UserCriteriaFilter;
    currentUser: User;
    checkedForCriteria: boolean;
}

export const useUserStore = defineStore('user', {
    state: (): UserStoreState => ({
        users: [] as User[],
        usersCriteriaFilter: [] as UserCriteriaFilter[],
        currentUserCriteriaFilter: clone(
            emptyUserCriteriaFilter,
        ) as UserCriteriaFilter,
        currentUser: clone(emptyUser),
        checkedForCriteria: false,
    }),
    getters: {
        getUserNameById: (state) => (id: string) => {
            var userIndex = findIndex(
                propEq('id', id),
                state.users,
            );

            return state.users[userIndex].username;
        },
        getIdByUserName: (state) => (username: string) => {
            var userIndex = findIndex(
                propEq('username', username),
                state.users,
            );

            return state.users[userIndex].id;
        },
    },
    actions: {
        async getAllUsers() {
            await UserService.getAllUsers().then(
                (response: AxiosResponse<any[]>) => {
                    if (hasValue(response, 'data')) {
                        this.users = clone(response.data as User[]);
                    }
                },
            );
        },

        async deactivateUser(userId: string) {
            const notificationStore = useNotificationStore();
            await UserService.deactivateUser(userId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.usersCriteriaFilter = this.usersCriteriaFilter.map((user) => {
                            if (user.userId === userId) {
                                return { ...user, activeStatus: false };
                            }
                            return user;
                        });

                        this.users = this.users.map((user) => {
                            if (user.id === userId) {
                                return { ...user, activeStatus: false };
                            }
                            return user;
                        });
                        notificationStore.addSuccessNotification({
                            message: 'Deactivated user',
                        });
                    }
                },
            );
        },
        async reactivateUser(userId: string) {
            const notificationStore = useNotificationStore();
            await UserService.reactivateUser(userId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.usersCriteriaFilter = this.usersCriteriaFilter.map((user) => {
                            if (user.userId === userId) {
                                return { ...user, activeStatus: true };
                            }
                            return user;
                        });

                        this.users = this.users.map((user) => {
                            if (user.id === userId) {
                                return { ...user, activeStatus: true };
                            }
                            return user;
                        });
                        notificationStore.addSuccessNotification({
                            message: 'Reactivated user',
                        });
                    }
                },
            );
        },

        async getUserCriteriaFilter() {
            const authenticationStore = useAuthenticationStore();
            const notificationStore = useNotificationStore();
            if (!this.checkedForCriteria) {
                const message =
                    'You do not have access to any bridge data. \
                Please contact an administrator to gain access to the data you need.';

                await UserService.getUserCriteriaFilterData().then(
                    (response: AxiosResponse) => {
                        if (hasValue(response, 'data')) {
                            this.checkedForCriteria = true;
                            const userCriteriaFilter: UserCriteriaFilter = response.data as UserCriteriaFilter;
                            this.currentUserCriteriaFilter = clone(userCriteriaFilter);
                            if (
                                !userCriteriaFilter.hasAccess &&
                                !authenticationStore.hasAdminAccess
                            ) {
                                notificationStore.addInfoNotification({
                                    message: 'Access Denied.',
                                    longMessage: message,
                                });
                            }
                        }
                    },
                );
            }
        },

        async getAllUserCriteriaFilter() {
            await UserService.getAllUsersCriteriaFilterData().then(
                (response: AxiosResponse<any[]>) => {
                    if (hasValue(response, 'data')) {
                        this.usersCriteriaFilter = clone(
                            response.data as UserCriteriaFilter[],
                        );
                    }
                },
            );
        },
        async updateUserCriteriaFilter(userCriteriaFilter: UserCriteriaFilter) {
            const notificationStore = useNotificationStore();
            await UserService.updateUserCriteriaFilterData(
                userCriteriaFilter,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    var criteriaIndex = findIndex(
                        propEq('userId', userCriteriaFilter.userId),
                        this.usersCriteriaFilter,
                    );
                    if (criteriaIndex != -1) {
                        this.usersCriteriaFilter[criteriaIndex].criteria =
                            userCriteriaFilter.criteria;
                        this.usersCriteriaFilter[criteriaIndex].hasAccess =
                            userCriteriaFilter.hasAccess;
                        this.usersCriteriaFilter[criteriaIndex].hasCriteria =
                            userCriteriaFilter.hasCriteria;
                    } else {
                        this.usersCriteriaFilter.push(userCriteriaFilter);
                    }

                    var index = findIndex(
                        propEq('id', userCriteriaFilter.userId),
                        this.users,
                    );
                    if (index != -1) {
                        var data = this.users[index];
                        data.hasInventoryAccess = userCriteriaFilter.hasAccess;
                        this.users = update(index, data, this.users);
                    }
                    notificationStore.addSuccessNotification({
                        message: 'Updated user criteria filter',
                    });
                }
            });
        },
        async revokeUserCriteriaFilter(userCriteriaId: string) {
            const notificationStore = useNotificationStore();
            await UserService.revokeUserCriteriaFilterData(
                userCriteriaId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    var index = findIndex(
                        propEq('criteriaId', userCriteriaId),
                        this.usersCriteriaFilter,
                    );
                    var currUserFilter = this.usersCriteriaFilter[index];

                    this.usersCriteriaFilter = reject(
                        propEq('criteriaId', userCriteriaId),
                        this.usersCriteriaFilter,
                    );

                    var userIndex = findIndex(
                        propEq('id', currUserFilter.userId),
                        this.users,
                    );
                    if (userIndex != -1) {
                        var data = this.users[userIndex];
                        data.description = currUserFilter.description;
                        data.name = currUserFilter.name;
                        data.hasInventoryAccess = false;
                        this.users = update(userIndex, data, this.users);
                    }
                    notificationStore.addSuccessNotification({
                        message: 'Deleted user criteria filter',
                    });
                }
            });
        },
        async updateUserLastNewsAccessDate(id: string, accessDate: string) {
            const notificationStore = useNotificationStore();
            let data: UserNewsAccessDate = {
                id,
                lastNewsAccessDate: accessDate
            };

            await UserService.updateLastNewsAccessDate(
                data
            ).then((response: AxiosResponse) => {
                if (
                    !(hasValue(response, 'status') &&
                    http2XX.test(response.status.toString()))
                ) {
                    notificationStore.addErrorNotification({
                        message: 'Failed to update news notification to server.',
                    });
                }
            });
        },
        async getCurrentUserByUserName(userName: string) {
            const notificationStore = useNotificationStore();
            await UserService.getUserByUserName(userName).then(
                (response: AxiosResponse<any>) => {
                    if (
                        hasValue(response, 'data') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.currentUser = response.data as User;
                    }
                    else {
                        notificationStore.addErrorNotification({
                            message: 'Failed to get current user from server.',
                        });
                    }
                },
            );
        },
    },
});
