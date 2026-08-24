export const SecurityTypes: any = {
    esec: 'ESEC',
    b2c: 'B2C',
    entra: 'Entra',
    localDebug: 'LocalDebug'
};

// Both B2C and Entra External ID authenticate through the same MSAL (msal-browser)
// flow, differing only in their configuration. Use this helper anywhere the app needs
// to treat the two cloud-identity options the same way.
export const isAzureSecurityType = (securityType: string): boolean =>
    securityType === SecurityTypes.b2c || securityType === SecurityTypes.entra;
