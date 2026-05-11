import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { clone, contains } from 'ramda';
import CommittedProjectsService from '@/services/committed-projects.service';
import { SectionCommittedProject } from '@/shared/models/iAM/committed-projects';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import { useNotificationStore } from './notification.store';

export interface CommittedProjectsStoreState {
    sectionCommittedProjects: SectionCommittedProject[];
}

export const useCommittedProjectStore = defineStore('committedProject', {
    state: (): CommittedProjectsStoreState => ({
        sectionCommittedProjects: [],
    }),
    actions: {
        async getCommittedProjects(scenarioId: string) {
            await CommittedProjectsService.getCommittedProjects(scenarioId).then(
                (response: AxiosResponse<SectionCommittedProject[]>) => {
                    if (hasValue(response, 'data')) {
                        this.sectionCommittedProjects = clone(response.data);
                    }
                },
            );
        },
        async deleteSpecificCommittedProjects(ids: string[]) {
            const notificationStore = useNotificationStore();
            await CommittedProjectsService.deleteSpecificCommittedProjects(ids).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.sectionCommittedProjects = this.sectionCommittedProjects.filter(
                            (project: SectionCommittedProject) =>
                                !contains(project.id, ids),
                        );
                        notificationStore.addSuccessNotification({
                            message: 'Deleted selected committed projects',
                        });
                    }
                },
            );
        },
        async deleteSimulationCommittedProjects(scenarioId: string) {
            const notificationStore = useNotificationStore();
            await CommittedProjectsService.deleteSimulationCommittedProjects(
                scenarioId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.sectionCommittedProjects = [];
                    notificationStore.addSuccessNotification({
                        message: 'Deleted committed projects',
                    });
                }
            });
        },
        async importCommittedProjects(file: File, selectedScenarioId: string) {
            const notificationStore = useNotificationStore();
            await CommittedProjectsService.importCommittedProjects(
                file,
                selectedScenarioId,
            ).then((response: AxiosResponse) => {
                if (hasValue(response, 'data')) {
                    notificationStore.addSuccessNotification({
                        message: 'Investment budgets file imported',
                    });
                }
            });
        },
    },
});