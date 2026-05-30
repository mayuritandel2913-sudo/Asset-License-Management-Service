using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssetManagement.AppService.DTOs.asset;

public class AssetStatusDistributionItem
{
    public string StatusName { get; set; } = null!;
    public int AssetCount { get; set; }
    public decimal Percentage { get; set; }
}

public class AssetStatusDistributionResponse
{
    public int TotalAssets { get; set; }

    public IEnumerable<AssetStatusDistributionItem> Statuses { get; set; } = new List<AssetStatusDistributionItem>();
}