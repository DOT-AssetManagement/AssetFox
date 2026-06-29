import { Configuration, PopupRequest } from '@azure/msal-browser';

// Microsoft Entra External ID (CIAM) configuration. Selected when config.json
// SecurityType = "Entra". Unlike B2C, Entra External ID uses the "ciamlogin.com"
// authority with no "/tfp/" path or "B2C_1_*" policy segment, and handles password
// reset within the single sign-up/sign-in flow.
// NOTE: the values below are placeholders. Real tenant/client values are
// environment-specific and supplied per deployment.
export const entraConfig: any = {
  clientId: 'ENTRA_APP_CLIENT_ID',
  tenantId: 'ENTRA_TENANT_ID',
  tenantName: 'YOUR_TENANT.ciamlogin.com',
  redirectUri: import.meta.env.VITE_APP_B2C_REDIRECT_URI as string,
  postLogoutRedirectUri: import.meta.env.VITE_APP_B2C_POST_LOGOUT_REDIRECT_URI as string,
  authority: 'https://YOUR_TENANT.ciamlogin.com/YOUR_TENANT.onmicrosoft.com',
};

export const msalConfig: Configuration = {
  auth: {
    clientId: entraConfig.clientId,
    authority: entraConfig.authority,
    // Required by MSAL when the authority host is not a well-known Microsoft endpoint.
    knownAuthorities: ['YOUR_TENANT.ciamlogin.com'],
    redirectUri: entraConfig.redirectUri,
    postLogoutRedirectUri: entraConfig.postLogoutRedirectUri,
  },
  cache: {
    cacheLocation: 'localStorage',
  },
};

// Scopes are exposed by the AssetFox API app registration (App ID URI api://<client-id>).
export const acquireTokenConfig: PopupRequest = {
  scopes: [
    'api://ENTRA_APP_CLIENT_ID/read_data',
    'api://ENTRA_APP_CLIENT_ID/write_data',
  ],
};
