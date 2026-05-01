import { defineStore } from 'pinia';

export interface Config {
    authorizationEndpoint: string;
    esecScope: string;
    clientId: string;
    redirectUri: string;
    securityType: string;
    localDegugInfo: {
        name: string;
        email: string;
        hasAdminAccess: boolean;
        hasSimulationAccess: boolean;
    };
    logos: {
        agencyFilename: string;
        implementationFilename: string;
    };
}

export interface ConfigStoreState {
    config: Config | null;
}

export const useConfigStore = defineStore('config', {
    state: (): ConfigStoreState => ({
        config: null,
    }),
    getters: {
        getConfig(state): Config | null {
            return state.config;
        },
    },
    actions: {
        setConfig(config: Config) {
            this.config = config;
        },
        async loadConfig() {
            try {
                const response = await fetch('/config.json');
                if (!response.ok) {
                    throw new Error('Failed to load config.json');
                }

                const config = (await response.json()) as Config;
                if (!config.logos || typeof config.logos.agencyFilename !== 'string' || typeof config.logos.implementationFilename !== 'string') {
                    console.warn('Config.json loaded but missing or invalid "logos" structure.',);
                }

                this.setConfig(config);
            } catch (error) {
                console.error('Error loading config:', error);
                throw error;
            }
        },
    },
});
