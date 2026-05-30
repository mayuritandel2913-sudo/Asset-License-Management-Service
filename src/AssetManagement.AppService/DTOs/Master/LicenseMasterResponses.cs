namespace AssetManagement.AppService.DTOs.License;

public class LicenseStatusResponse
{
    public byte LicenseStatusID { get; set; }
    public string LicenseStatusName { get; set; } = null!;
}

public class LicenseTypeResponse
{
    public byte LicenseTypeID { get; set; }
    public string LicenseTypeName { get; set; } = null!;
}

public class LicensePurchaseTypeResponse
{
    public byte LicensePurchaseTypeID { get; set; }
    public string LicensePurchaseTypeName { get; set; } = null!;
}

public class ReminderDaysResponse
{
    public byte ReminderConfigID { get; set; }
    public int DaysBeforeExpiry { get; set; }
}
