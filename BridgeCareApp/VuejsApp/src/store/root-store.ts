import Vuex, { createStore } from 'vuex';

import busyModule from '@/store-modules/busy.module';
import authenticationModule from '@/store-modules/authentication.module';
import networkModule from '../store-modules/network.module';
import inventoryModule from '@/store-modules/inventory.module';
import investmentModule from '@/store-modules/investment.module';
import performanceCurveModule from '@/store-modules/performance-curve.module';
import attributeModule from '@/store-modules/attribute.module';
import deficientConditionGoalModule from '@/store-modules/deficient-condition-goal.module';
import budgetPriorityModule from '@/store-modules/budget-priority.module';
import remainingLifeLimitModule from '@/store-modules/remaining-life-limit.module';
import pollingModule from '@/store-modules/polling.module';
import announcementModule from '@/store-modules/announcement.module';
import cashFlowModule from '@/store-modules/cash-flow.module';
import criterionModule from '@/store-modules/criterion-library.module';
import analysisMethodModule from '@/store-modules/analysis-method.module';
import azureB2CModule from '@/store-modules/azureB2C.module';
import calculatedAttributeModule from '@/store-modules/calculated-attribute.module';
import notificationModule from '@/store-modules/notification.module';
import datasourceModule from '@/store-modules/data-source.module';
import committedProjectsModule from '@/store-modules/committed-project.module';
import adminDataModule from '@/store-modules/admin-data.module';
import adminSiteSettingsModule from '@/store-modules/admin-site-settings.module';
import alertModule from '@/store-modules/alert.module';
import configModule from '@/store-modules/config.module';


export default createStore({
    modules: {
        configModule,
        busyModule,
        authenticationModule,
        networkModule,
        inventoryModule,
        investmentModule,
        performanceCurveModule,
        attributeModule,
        deficientConditionGoalModule,
        budgetPriorityModule,
        remainingLifeLimitModule,
        pollingModule,
        announcementModule,
        cashFlowModule,
        criterionModule,
        analysisMethodModule,
        azureB2CModule,
        calculatedAttributeModule,
        notificationModule,
        datasourceModule,
        committedProjectsModule,
        adminDataModule,
        adminSiteSettingsModule,
        alertModule,
    },
});
