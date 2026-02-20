import { AxiosPromise } from 'axios';
import { API, coreAxiosInstance } from '@/shared/utils/axios-instance';
import { TreatmentRuleOverride, ScenarioOverrideSetting } from '@/shared/models/iAM/treatment-rule-override';

export default class TreatmentRuleOverrideService {
    static getRuleOverrides(libraryId: string): AxiosPromise {
        return coreAxiosInstance.get(
            `${API.Treatment}/GetRuleOverrides/${libraryId}`,
        );
    }

    static upsertRuleOverrides(libraryId: string, overrides: TreatmentRuleOverride[]): AxiosPromise {
        return coreAxiosInstance.post(
            `${API.Treatment}/UpsertRuleOverrides/${libraryId}`,
            overrides,
        );
    }

    static deleteRuleOverride(overrideId: string): AxiosPromise {
        return coreAxiosInstance.delete(
            `${API.Treatment}/DeleteRuleOverride/${overrideId}`,
        );
    }

    static getScenarioOverrideSettings(simulationId: string): AxiosPromise {
        return coreAxiosInstance.get(
            `${API.Treatment}/GetScenarioOverrideSettings/${simulationId}`,
        );
    }

    static upsertScenarioOverrideSettings(simulationId: string, settings: ScenarioOverrideSetting[]): AxiosPromise {
        return coreAxiosInstance.post(
            `${API.Treatment}/UpsertScenarioOverrideSettings/${simulationId}`,
            settings,
        );
    }
}
