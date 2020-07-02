const azureB2CConfig: any = {
    clientId: '8c7f8f66-30ce-47db-ab02-13a10f98c865',
    //tenantId: '73e26d61-77cd-4798-9495-083fa8912f8d',
    tenantId: 'aratranstest.onmicrosoft.com/B2C_1_SU-SI-POL',
    tenantName: 'aratranstest.b2clogin.com/tfp',
    validateAuthority: false,
    redirectUri: 'http://localhost:8080/Authentication',
    postLogoutRedirectUri: 'http://localhost:8080/iAM',
    authority: 'https://aratranstest.b2clogin.com/tfp/aratranstest.onmicrosoft.com/B2C_1_SU-SI-POL',
    forgetPasswordAuthority: 'https://aratranstest.b2clogin.com/tfp/aratranstest.onmicrosoft.com/B2C_1_PR-POL'
  };

  const msalConfig: any = {
    auth: {
        clientId: azureB2CConfig.clientId,
        authority: azureB2CConfig.authority,
        redirectUri: azureB2CConfig.redirectUri,
        postLogoutRedirectUri: azureB2CConfig.postLogoutRedirectUri,
        cacheLocation: 'localStorage',
        validateAuthority: azureB2CConfig.validateAuthority,
    }
};

const msalConfigForPasswordReset: any = {
  auth: {
    clientId: azureB2CConfig.clientId,
    authority: azureB2CConfig.forgetPasswordAuthority,
    redirectUri: azureB2CConfig.redirectUri,
    postLogoutRedirectUri: azureB2CConfig.postLogoutRedirectUri,
    cacheLocation: 'localStorage',
    validateAuthority: azureB2CConfig.validateAuthority
}
}

const acquireTokenConfig: any = {
  auth: {
    clientId: azureB2CConfig.clientId,
    authority: azureB2CConfig.authority,
    redirectUri: azureB2CConfig.redirectUri,
    postLogoutRedirectUri: azureB2CConfig.postLogoutRedirectUri,
    cacheLocation: 'localStorage',
    validateAuthority: azureB2CConfig.validateAuthority,
    scopes: ['user.read'],
}
}

  export {msalConfig, azureB2CConfig, msalConfigForPasswordReset, acquireTokenConfig};