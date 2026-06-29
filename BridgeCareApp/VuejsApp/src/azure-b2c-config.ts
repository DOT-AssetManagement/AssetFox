import { Configuration, PopupRequest } from '@azure/msal-browser';

// Azure AD B2C configuration. B2C uses the "b2clogin.com/tfp/<tenant>/<policy>" authority
// form with "B2C_1_*" user-flow (policy) names. Selected when config.json SecurityType = "B2C".
export const azureB2CConfig: any = {
  clientId: '8c7f8f66-30ce-47db-ab02-13a10f98c865',
  tenantId: 'aratranstest.onmicrosoft.com/B2C_1_SU-SI-POL',
  tenantName: 'aratranstest.b2clogin.com/tfp',
  redirectUri: import.meta.env.VITE_APP_B2C_REDIRECT_URI as string,
  postLogoutRedirectUri: import.meta.env.VITE_APP_B2C_POST_LOGOUT_REDIRECT_URI as string,
  authority: 'https://aratranstest.b2clogin.com/tfp/aratranstest.onmicrosoft.com/B2C_1_SU-SI-POL',
  forgotPasswordAuthority: 'https://aratranstest.b2clogin.com/tfp/aratranstest.onmicrosoft.com/B2C_1_PR-POL',
};

export const msalConfig: Configuration = {
  auth: {
    clientId: azureB2CConfig.clientId,
    authority: azureB2CConfig.authority,
    // Required by MSAL when the authority host is not a well-known Microsoft endpoint.
    knownAuthorities: ['aratranstest.b2clogin.com'],
    redirectUri: azureB2CConfig.redirectUri,
    postLogoutRedirectUri: azureB2CConfig.postLogoutRedirectUri,
  },
  cache: {
    cacheLocation: 'localStorage',
  },
};

// B2C handles password reset through a separate "B2C_1_PR-POL" policy authority. The store
// module redirects here when B2C returns the AADB2C90118 "forgot password" error.
export const forgotPasswordAuthority: string = azureB2CConfig.forgotPasswordAuthority;

export const acquireTokenConfig: PopupRequest = {
  scopes: [
    'https://aratranstest.onmicrosoft.com/api/read_data',
    'https://aratranstest.onmicrosoft.com/api/write_data',
  ],
};
