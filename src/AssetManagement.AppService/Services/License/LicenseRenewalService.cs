using AssetManagement.AppService.Contracts;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Entities.License;
using AssetManagement.Utility.Resource;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssetManagement.AppService.Services;

public class LicenseRenewalService : ILicenseRenewalService
{
    private readonly ILicenseRepository _licenseRepository;
    private readonly ILogger<LicenseRenewalService> _logger;

    public LicenseRenewalService(ILicenseRepository licenseRepository, ILogger<LicenseRenewalService> logger)
    {
        _licenseRepository = licenseRepository;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var currentDate = DateTime.Today;
        _logger.LogInformation("License renewal processing started for {CurrentDate}", currentDate.ToString(CommonResource.DateFormat));

        var renewals = await _licenseRepository.GetRenewalsByDateAsync(currentDate);

        if (renewals.Count == 0)
        {
            _logger.LogInformation("No license renewals were found for {CurrentDate}", currentDate.ToString(CommonResource.DateFormat));
            return;
        }

        foreach (var renewal in renewals)
        {
            await ApplyRenewalAsync(renewal);
        }

        _logger.LogInformation("License renewal processing completed for {CurrentDate}", currentDate.ToString(CommonResource.DateFormat));
    }

    private async Task ApplyRenewalAsync(LicenseRenewal renewal)
    {
        if (renewal.LicenseRenewalDate.Date != DateTime.Today)
        {
            _logger.LogInformation(
                "Skipping renewal {LicenseRenewalId} because renewal date {RenewalDate} is not today.",
                renewal.LicenseRenewalID,
                renewal.LicenseRenewalDate.ToString(CommonResource.DateFormat));
            return;
        }

        if (renewal.License == null)
        {
            _logger.LogWarning("Skipping renewal {LicenseRenewalId} because the related license was not loaded.", renewal.LicenseRenewalID);
            return;
        }

        if (!renewal.UpdatedCost.HasValue || !renewal.UpdatedTotalSeats.HasValue)
        {
            _logger.LogWarning("Skipping renewal {LicenseRenewalId} because updated cost or total seats is missing.", renewal.LicenseRenewalID);
            return;
        }

        var updatedLicenseKey = string.IsNullOrWhiteSpace(renewal.UpdatedLicenseKey)
            ? renewal.License.LicenseKey
            : renewal.UpdatedLicenseKey.Trim().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(updatedLicenseKey) && !string.Equals(updatedLicenseKey, renewal.License.LicenseKey, StringComparison.OrdinalIgnoreCase))
        {
            var licenseKeyExists = await _licenseRepository.LicenseKeyExistsForOtherLicenseAsync(updatedLicenseKey, renewal.License.LicenseID);

            if (licenseKeyExists)
            {
                _logger.LogWarning(
                    "Renewal {LicenseRenewalId} has a duplicate updated license key {LicenseKey}; keeping the current license key and updating the other renewal fields.",
                    renewal.LicenseRenewalID,
                    updatedLicenseKey);
                updatedLicenseKey = renewal.License.LicenseKey;
            }
        }

        try
        {
            var updatedTotalSeats = Convert.ToByte(renewal.UpdatedTotalSeats.Value);

            renewal.License.ExpiryDate = renewal.ExpiryDate;
            renewal.License.StartDate = renewal.LicenseRenewalDate;
            renewal.License.TotalSeats = updatedTotalSeats;
            renewal.License.Cost = renewal.UpdatedCost.Value;
            renewal.License.LicenseKey = updatedLicenseKey;
            renewal.License.ModifiedByID = renewal.CreatedByID;
            renewal.License.ModifiedDate = DateTime.UtcNow;

            await _licenseRepository.UpdateLicenseAsync(renewal.License);

            _logger.LogInformation(
                "Updated license {LicenseId} from renewal {LicenseRenewalId}",
                renewal.LicenseID,
                renewal.LicenseRenewalID);
        }
        catch (OverflowException ex)
        {
            _logger.LogError(ex, "Skipping renewal {LicenseRenewalId} because updated seats exceed the license column range.", renewal.LicenseRenewalID);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Skipping renewal {LicenseRenewalId} because the renewed license could not be saved.", renewal.LicenseRenewalID);
        }
    }
}