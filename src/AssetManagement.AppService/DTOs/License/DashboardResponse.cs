using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagement.AppService.DTOs.License
{
    public class LicenseUtilizationResponse
    {
        public int AssignedCount { get; set; }
        public int UnassignedCount { get; set; }
        public int TotalLicenses { get; set; }
        public decimal AssignedPercentage { get; set; }
        public decimal UnassignedPercentage { get; set; }
    }

    public class LicenseStatusOverviewResponse
    {
        public int New { get; set; }
        public int Active { get; set; }
        public int Expired { get; set; }
        public int TotalLicenses { get; set; }
        public decimal NewPercentage { get; set; }
        public decimal ActivePercentage { get; set; }
        public decimal ExpiredPercentage { get; set; }
    }

    public class MonthlyCostItem
    {
        public string Month { get; set; } = null!;
        public decimal TotalCost { get; set; }
    }

    public class LicenseCostOvertimeResponse
    {
        public int Year { get; set; }
        public IEnumerable<MonthlyCostItem> MonthlyCosts { get; set; } = new List<MonthlyCostItem>();

    }


    public class UpcomingLicenseExpirationItem
    {
        public string LicenseName { get; set; } = null!;
        public string ExpiryDate { get; set; } = null!;
        public int DaysRemaining { get; set; }
    }

    public class UpcomingLicenseExpirationResponse
    {
        public IEnumerable<UpcomingLicenseExpirationItem> Licenses { get; set; } = new List<UpcomingLicenseExpirationItem>();
    }

    


}
