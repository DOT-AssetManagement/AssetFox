using System;
using System.Collections.Generic;
using System.Drawing;

public interface IAdminSettingsRepository
{
    string GetConstraintType();
    void SetConstraintType(string constraintType);
    IList<string> GetKeyFields();
    IList<string> GetRawKeyFields();

    void SetKeyFields(string keyFields);

    public IList<string> GetRawDataKeyFields();
    public void SetRawDataKeyFields(string keyFields);
    public IList<string> GetAssetType();
    public void SetAssetType(string assetType);

    void SetPrimaryNetwork(string name);

    void SetRawDataNetwork(string name);
    Guid? GetPrimaryNetworkId();
    Guid? GetRawDataNetworkId();

    string GetPrimaryNetwork();

    string GetRawDataNetwork();

    IList<string> GetSimulationReportNames();

    void SetInventoryReports(string inventoryReports);

    IList<string> GetInventoryReports();

    string GetAttributeName(Guid attributeId);

    void SetSimulationReports(string simulationReports);

    string GetImplementationName();

    void SetImplementationName(string name);

    string GetAgencyLogo();

    void SetAgencyLogo(Image agencyLogo, string imageType);
    void SetAgencyLogo(byte[] productLogo);

    string GetImplementationLogo();

    void SetImplementationLogo(Image productLogo, string imageType);
    void SetImplementationLogo(byte[] productLogo);

    void DeleteAdminSetting(string settingKey);

    public string GetAdminContactEmail();

    public void SetAdminContactEmail(string email);
}
