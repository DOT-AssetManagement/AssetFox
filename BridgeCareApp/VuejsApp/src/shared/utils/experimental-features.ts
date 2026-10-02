import pinia, { useAuthenticationStore, useConfigStore } from '@/stores';

// Remaining life limits, target condition goals and deficient condition goals are experimental. They stay hidden unless
// config.json explicitly sets "experimentalFeaturesEnabled": true, and even then only admins see them.
export function canSeeExperimentalFeatures(): boolean {
    return useConfigStore(pinia).experimentalFeaturesEnabled && useAuthenticationStore(pinia).hasAdminAccess;
}
