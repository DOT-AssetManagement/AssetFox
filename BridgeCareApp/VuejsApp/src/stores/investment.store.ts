import { defineStore } from 'pinia';
import { AxiosResponse } from 'axios';
import { any, append, clone, find, findIndex, isNil, propEq, reject, update } from 'ramda';
import InvestmentService from '@/services/investment.service';
import { useNotificationStore } from './notification.store';
import { hasValue } from '@/shared/utils/has-value-util';
import { http2XX } from '@/shared/utils/http-utils';
import {
    Budget,
    BudgetLibrary,
    emptyBudgetLibrary,
    emptyInvestmentPlan,
    Investment,
    InvestmentPlan,
    SimpleBudgetDetail,
} from '@/shared/models/iAM/investment';
import { UserCriteriaFilter } from '@/shared/models/iAM/user-criteria-filter';
import { useAlertStore } from './alert.store';

export interface UpsertInvestmentPayload {
    investment: Investment;
    scenarioId: string;
}

export interface ScenarioSimpleBudgetDetailsPayload {
    scenarioId: string;
}

export interface InvestmentBudgetsImportPayload {
    file: File;
    overwriteBudgets: boolean;
    id: string;
    currentUserCriteriaFilter: UserCriteriaFilter;
}

export interface InvestmentStoreState {
    budgetLibraries: BudgetLibrary[];
    selectedBudgetLibrary: BudgetLibrary;
    investmentPlan: InvestmentPlan;
    scenarioSimpleBudgetDetails: SimpleBudgetDetail[];
    scenarioBudgets: Budget[];
    isSuccessfulImport: boolean;
    hasPermittedAccess: boolean;
}

export const useInvestmentStore = defineStore('investment', {
    state: (): InvestmentStoreState => ({
        budgetLibraries: [],
        selectedBudgetLibrary: clone(emptyBudgetLibrary),
        investmentPlan: clone(emptyInvestmentPlan),
        scenarioSimpleBudgetDetails: [],
        scenarioBudgets: [],
        isSuccessfulImport: false,
        hasPermittedAccess: false,
    }),
    actions: {
        budgetLibraryMutator(library: BudgetLibrary) {
            this.budgetLibraries = any(
                propEq('id', library.id),
                this.budgetLibraries,
            )
                ? (update(
                      findIndex(propEq('id', library.id), this.budgetLibraries),
                      library,
                      this.budgetLibraries,
                  ) as BudgetLibrary[])
                : (append(library, this.budgetLibraries) as BudgetLibrary[]);
        },
        selectedBudgetLibraryMutator(libraryId: string) {
            this.selectedBudgetLibrary = any(
                propEq('id', libraryId),
                this.budgetLibraries,
            )
                ? (find(
                      propEq('id', libraryId),
                      this.budgetLibraries,
                  ) as BudgetLibrary)
                : clone(emptyBudgetLibrary);
        },
        investmentPlanMutator(investmentPlan: InvestmentPlan) {
            this.investmentPlan = clone(investmentPlan);
        },
        isSuccessfulImportMutator(isSuccessful: boolean) {
            this.isSuccessfulImport = isSuccessful;
        },
        selectBudgetLibrary(libraryId: string) {
            this.selectedBudgetLibraryMutator(libraryId);
        },
        async getInvestment(scenarioId: string) {
            await InvestmentService.getInvestment(scenarioId).then(
                (response: AxiosResponse<Investment>) => {
                    if (hasValue(response, 'data')) {
                        const investmentData = response.data as Investment;
                        this.scenarioBudgets = clone(investmentData.scenarioBudgets);
                        this.investmentPlan = clone(investmentData.investmentPlan);
                    }
                },
            );
        },
        async upsertInvestment(payload: UpsertInvestmentPayload) {
            const notificationStore = useNotificationStore();
            await InvestmentService.upsertInvestment(
                payload.investment as any,
                payload.scenarioId,
            ).then((response: AxiosResponse) => {
                if (
                    hasValue(response, 'status') &&
                    http2XX.test(response.status.toString())
                ) {
                    this.scenarioBudgets = clone(payload.investment.scenarioBudgets);
                    this.investmentPlan = clone(payload.investment.investmentPlan);
                    notificationStore.addSuccessNotification({
                        message: 'Modified investment',
                    });
                }
            });
        },
        async getBudgetLibraries() {
            await InvestmentService.getBudgetLibraries().then(
                (response: AxiosResponse<BudgetLibrary[]>) => {
                    if (hasValue(response, 'data')) {
                        this.budgetLibraries = clone(response.data);
                    }
                },
            );
        },
        async deleteBudgetLibrary(libraryId: string) {
            const notificationStore = useNotificationStore();
            await InvestmentService.deleteBudgetLibrary(libraryId).then(
                (response: AxiosResponse) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.budgetLibraries = reject(
                            propEq('id', libraryId),
                            this.budgetLibraries,
                        ) as BudgetLibrary[];
                        notificationStore.addSuccessNotification({
                            message: 'Deleted budget library',
                        });
                    }
                },
            );
        },
        async getScenarioSimpleBudgetDetails(
            payload: ScenarioSimpleBudgetDetailsPayload,
        ) {
            await InvestmentService.getScenarioSimpleBudgetDetails(
                payload.scenarioId,
            ).then((response: AxiosResponse<SimpleBudgetDetail[]>) => {
                if (hasValue(response, 'data')) {
                    this.scenarioSimpleBudgetDetails = clone(response.data);
                }
            });
        },
        async importScenarioInvestmentBudgetsFile(
            payload: InvestmentBudgetsImportPayload,
        ) {
            const alertStore = useAlertStore();
            await InvestmentService.importInvestmentBudgets(
                payload.file,
                payload.overwriteBudgets,
                payload.id,
                true,
                payload.currentUserCriteriaFilter,
            ).then((response: AxiosResponse) => {
                    if(!isNil(response))
                        alertStore.setAlertMessage('Investment Budgets import has been added to the queue');
                });
        },
        async importLibraryInvestmentBudgetsFile(
            payload: InvestmentBudgetsImportPayload,
        ) {
            const alertStore = useAlertStore();
            await InvestmentService.importInvestmentBudgets(
                payload.file,
                payload.overwriteBudgets,
                payload.id,
                false,
                payload.currentUserCriteriaFilter,
            ).then((response: AxiosResponse) => {
                    if(!isNil(response))
                        alertStore.setAlertMessage('Investment Budgets import has been added to the queue');
                });
        },
        async getHasPermittedAccess() {
            await InvestmentService.getHasPermittedAccess().then(
                (response: AxiosResponse<boolean>) => {
                    if (
                        hasValue(response, 'status') &&
                        http2XX.test(response.status.toString())
                    ) {
                        this.hasPermittedAccess = response.data as boolean;
                    }
                },
            );
        },
    },
});
