using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Data;
using AssetManagement.Infrastructure.Entities.License;
using AssetManagement.Utility.Resource;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repository;

public class LicenseRepository : ILicenseRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public LicenseRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;

    }
    public async Task<License> AddAsyncLicense(License license)
    {
        _applicationDbContext.License.Add(license);
        await _applicationDbContext.SaveChangesAsync();
        return license;
    }

    public async Task<bool> LicenseKeyExistsAsync(string licenseKey)
    {
        return await _applicationDbContext.License
            .AnyAsync(l => l.LicenseKey != null && l.LicenseKey.Trim().ToUpper() == licenseKey && l.DeletedDate == null);
    }

    public async Task<byte?> GetNewStatusIdAsync()
    {
        return await _applicationDbContext.LicenseStatus
            .Where(s => s.LicenseStatusName == "New")
            .Select(s => (byte?)s.LicenseStatusID)
            .FirstOrDefaultAsync();
    }

    public async Task<List<byte>> GetReminderConfigIdsByDaysAsync(List<int> days)
    {
        return await _applicationDbContext.ReminderConfig
            .Where(rc => days.Contains(rc.DaysBeforeExpiry) && rc.DeletedDate == null)
            .Select(rc => rc.ReminderConfigID)
            .ToListAsync();
    }

    public async Task<bool> LicenseTypeExistsAsync(byte licenseTypeId)
    {
        return await _applicationDbContext.LicenseType
            .AnyAsync(lt => lt.LicenseTypeID == licenseTypeId && lt.DeletedDate == null);
    }

    public async Task<bool> LicensePurchaseTypeExistsAsync(byte licensePurchaseTypeId)
    {
        return await _applicationDbContext.LicensePurchaseType
            .AnyAsync(lpt => lpt.LicensePurchaseTypeID == licensePurchaseTypeId && lpt.DeletedDate == null);
    }

    public async Task<string?> GetLicenseTypeNameAsync(byte licenseTypeId)
    {
        return await _applicationDbContext.LicenseType
            .Where(lt => lt.LicenseTypeID == licenseTypeId && lt.DeletedDate == null)
            .Select(lt => lt.LicenseTypeName)
            .FirstOrDefaultAsync();
    }

  
    

  public async Task<IEnumerable<License>> GetLicensesWithAssignmentsAsync(DateTime? startDate = null, DateTime? endDate = null, string? filter = null, string? search = null)
    {
        var query = _applicationDbContext.License
            .Include(l => l.LicenseType)
            .Include(l => l.LicensePurchaseType)
            .Include(l => l.LicenseStatus)
            .Include(l => l.LicenseAssignments)
                .ThenInclude(a => a.Assignee)
            .Where(l => l.DeletedDate == null);

        if (startDate.HasValue)
        {
            query = query.Where(l => l.PurchaseDate >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.PurchaseDate <= endDate.Value.Date);
        }

      
        if (!string.IsNullOrWhiteSpace(filter))
        {
            var today = DateTime.UtcNow.Date;

            if (filter.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => l.LicenseStatus != null && l.LicenseStatus.LicenseStatusName == "Active" && (l.ExpiryDate == null || l.ExpiryDate.Value.Date >= today));
            }
            else if (filter.Equals("New", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => l.LicenseStatus != null && l.LicenseStatus.LicenseStatusName == "New");
            }
            else if (filter.Equals("Expired", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date < today);
            }
            else if (filter.Equals("Expiring3d", StringComparison.OrdinalIgnoreCase))
            {
                var maxDate = today.AddDays(3);
                query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date >= today && l.ExpiryDate.Value.Date <= maxDate);
            }
            else if (filter.Equals("Expiring7d", StringComparison.OrdinalIgnoreCase))
            {
                var maxDate = today.AddDays(7);
                query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date >= today && l.ExpiryDate.Value.Date <= maxDate);
            }
            else if (filter.Equals("Expiring15d", StringComparison.OrdinalIgnoreCase))
            {
                var maxDate = today.AddDays(15);
                query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date >= today && l.ExpiryDate.Value.Date <= maxDate);
            }
            else if (filter.Equals("Expiring30d", StringComparison.OrdinalIgnoreCase))
            {
                var maxDate = today.AddDays(30);
                query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date >= today && l.ExpiryDate.Value.Date <= maxDate);
            }
        }

        
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(l => 
                (l.LicenseName != null && l.LicenseName.ToLower().Contains(searchTerm)) || 
                (l.VendorName != null && l.VendorName.ToLower().Contains(searchTerm)));
        }

        return await query.ToListAsync();
    }

    public async Task<bool> ReminderConfigExistsAsync(byte reminderConfigId)
    {
        return await _applicationDbContext.ReminderConfig
            .AnyAsync(rc => rc.ReminderConfigID == reminderConfigId && rc.DeletedDate == null);
    }

    public async Task<(IEnumerable<License> Licenses, int TotalRecords)> GetPaginatedLicensesAsync(string? search, string filter, int pageNo, int pageSize)
    {
        var query = _applicationDbContext.License
            .Include(l => l.LicenseStatus)
            .Where(l => l.DeletedDate == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim().ToLower();
            query = query.Where(l => 
                (l.LicenseName != null && l.LicenseName.ToLower().Contains(searchTerm)) || 
                (l.VendorName != null && l.VendorName.ToLower().Contains(searchTerm)));
        }

        var today = DateTime.UtcNow.Date;

        if (filter.Equals("Active", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(l => l.LicenseStatus != null && l.LicenseStatus.LicenseStatusName == "Active" && (l.ExpiryDate == null || l.ExpiryDate.Value.Date >= today));
        }
        else if (filter.Equals("New", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(l => l.LicenseStatus != null && l.LicenseStatus.LicenseStatusName == "New");
        }
        else if (filter.Equals("Expired", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date < today);
        }
        else if (filter.Equals("Expiring3d", StringComparison.OrdinalIgnoreCase))
        {
            var maxDate = today.AddDays(3);
            query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date >= today && l.ExpiryDate.Value.Date <= maxDate);
        }
        else if (filter.Equals("Expiring7d", StringComparison.OrdinalIgnoreCase))
        {
            var maxDate = today.AddDays(7);
            query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date >= today && l.ExpiryDate.Value.Date <= maxDate);
        }
        else if (filter.Equals("Expiring15d", StringComparison.OrdinalIgnoreCase))
        {
            var maxDate = today.AddDays(15);
            query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date >= today && l.ExpiryDate.Value.Date <= maxDate);
        }
        else if (filter.Equals("Expiring30d", StringComparison.OrdinalIgnoreCase))
        {
            var maxDate = today.AddDays(30);
            query = query.Where(l => l.ExpiryDate != null && l.ExpiryDate.Value.Date >= today && l.ExpiryDate.Value.Date <= maxDate);
        }

        var totalRecords = await query.CountAsync();

        var licenses = await query
            .OrderByDescending(l => l.CreatedDate)
            .Skip((pageNo - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (licenses, totalRecords);
    }

    public async Task<License?> GetLicenseByIDAsync(int licenseId)
    {
        return await _applicationDbContext.License
            .Include(l => l.LicenseType)
            .Include(l => l.LicensePurchaseType)
            .Include(l => l.LicenseStatus)
            .Include(l => l.LicenseAssignments)
                .ThenInclude(a => a.Assignee)
                .ThenInclude(u => u.Department)
            .FirstOrDefaultAsync(l => l.LicenseID == licenseId && l.DeletedDate == null);
    }

    public async Task<License?> GetLicenseByIdAsync(int licenseId)
    {
        return await _applicationDbContext.License
            .Include(l => l.LicenseReminders)
            .FirstOrDefaultAsync(l => l.LicenseID == licenseId && l.DeletedDate == null);
    }

    public async Task<List<LicenseRenewal>> GetRenewalsByDateAsync(DateTime renewalDate)
    {
        var targetDate = renewalDate.Date;
        var nextDate = targetDate.AddDays(1);

        return await _applicationDbContext.LicenseRenewal
            .Include(renewal => renewal.License)
            .Where(renewal =>
                renewal.LicenseRenewalDate >= targetDate &&
                renewal.LicenseRenewalDate < nextDate &&
                renewal.License.DeletedDate == null)
            .ToListAsync();
    }

    public async Task<bool> HasLicenseRenewalAsync(int licenseId)
    {
        return await _applicationDbContext.LicenseRenewal
            .AnyAsync(renewal => renewal.LicenseID == licenseId && renewal.License.DeletedDate == null);
    }

    public async Task<List<int>> GetActiveAssigneeIdsByLicenseIdAsync(int licenseId)
    {
        return await _applicationDbContext.LicenseAssignment
            .Where(assignment => assignment.LicenseID == licenseId && assignment.IsActive)
            .Select(assignment => assignment.AssigneeID)
            .Distinct()
            .ToListAsync();
    }

    public async Task<LicenseAssignment> AddLicenseAssignmentAsync(LicenseAssignment assignment)
    {
        _applicationDbContext.LicenseAssignment.Add(assignment);
        await _applicationDbContext.SaveChangesAsync();
        return assignment;
    }

    public async Task<LicenseRenewal?> GetLicenseRenewalByIdAsync(int renewalId)
    {
        return await _applicationDbContext.LicenseRenewal
            .FirstOrDefaultAsync(lr => lr.LicenseRenewalID == renewalId);
    }

    public async Task<List<LicenseRenewal>> GetLicenseRenewalsByLicenseIdAsync(int licenseId)
    {
        return await _applicationDbContext.LicenseRenewal
            .Where(lr => lr.LicenseID == licenseId)
            .OrderBy(lr => lr.LicenseRenewalDate)
            .ToListAsync();
    }

    public async Task<LicenseRenewal> AddLicenseRenewalAsync(LicenseRenewal renewal)
    {
        _applicationDbContext.LicenseRenewal.Add(renewal);
        await _applicationDbContext.SaveChangesAsync();
        return renewal;
    }

    public async Task UpdateLicenseRenewalAsync(LicenseRenewal renewal)
    {
        _applicationDbContext.LicenseRenewal.Update(renewal);
        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task<bool> CheckAvailableSeatsAsync(int licenseId, int assigneeCount)
    {
        var license = await _applicationDbContext.License
            .FirstOrDefaultAsync(l => l.LicenseID == licenseId && l.DeletedDate == null);

        if (license == null)
            return false;

        var currentAssignments = await _applicationDbContext.LicenseAssignment
            .CountAsync(la => la.LicenseID == licenseId && la.IsActive);
        return (currentAssignments + assigneeCount) <= license.TotalSeats;
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _applicationDbContext.User
            .AnyAsync(u => u.UserID == userId && u.DeletedDate == null);
    }

    public async Task<bool> IsUserActiveAsync(int userId)
    {
        return await _applicationDbContext.User
            .AnyAsync(u => u.UserID == userId && u.IsActive == true && u.DeletedDate == null);
    }

    public async Task<bool> IsUserAlreadyAssignedAsync(int licenseId, int userId)
    {
        return await _applicationDbContext.LicenseAssignment
            .AnyAsync(la => la.LicenseID == licenseId && la.AssigneeID == userId && la.IsActive);
    }

    public async Task<LicenseAssignment?> GetLicenseAssignmentAsync(int licenseId, int userId)
    {
        return await _applicationDbContext.LicenseAssignment
            .FirstOrDefaultAsync(la => la.LicenseID == licenseId && la.AssigneeID == userId && la.IsActive);
    }

    public async Task UnassignLicenseAsync(int licenseId, int userId, int unassignedById)
    {
        var assignment = await _applicationDbContext.LicenseAssignment
            .FirstOrDefaultAsync(la => la.LicenseID == licenseId && la.AssigneeID == userId && la.IsActive);

        if (assignment != null)
        {
            assignment.IsActive = false;
            assignment.UnassignedDate = DateTime.UtcNow;
            assignment.UnassignedBy = unassignedById.ToString();
            assignment.ModifiedByID = unassignedById;
            assignment.ModifiedDate = DateTime.UtcNow;
            _applicationDbContext.LicenseAssignment.Update(assignment);
            await _applicationDbContext.SaveChangesAsync();
        }
    }

    public async Task<int> GetAssignedSeatsCountAsync(int licenseId)
    {
        return await _applicationDbContext.LicenseAssignment
            .Where(la => la.LicenseID == licenseId && la.IsActive)
            .CountAsync();
    }

    public async Task UpdateLicenseAsync(License license)
    {
        _applicationDbContext.License.Update(license);
        await _applicationDbContext.SaveChangesAsync();
    }

    public async Task<bool> LicenseKeyExistsForOtherLicenseAsync(string licenseKey, int licenseId)
    {
        return await _applicationDbContext.License
            .AnyAsync(l => l.LicenseKey != null && l.LicenseKey.Trim().ToUpper() == licenseKey && l.LicenseID != licenseId && l.DeletedDate == null);
    }

    public async Task DeleteLicenseRemindersAsync(int licenseId)
    {
        var reminders = await _applicationDbContext.LicenseReminder
            .Where(lr => lr.LicenseID == licenseId)
            .ToListAsync();

        if (reminders.Count > 0)
        {
            _applicationDbContext.LicenseReminder.RemoveRange(reminders);
            await _applicationDbContext.SaveChangesAsync();
        }
    }

    public async Task<byte?> GetStatusIdByNameAsync(string statusName)
    {
        return await _applicationDbContext.LicenseStatus
            .Where(s => s.LicenseStatusName == statusName)
            .Select(s => (byte?)s.LicenseStatusID)
            .FirstOrDefaultAsync();
    }

    public async Task<(IEnumerable<LicenseAuditLog> Logs, int TotalRecords, string LicenseName)> GetLicenseAuditLogsAsync(int licenseId, int pageNo, int pageSize)
    {
        var license = await _applicationDbContext.License
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LicenseID == licenseId && l.DeletedDate == null);

        if (license == null) return (new List<LicenseAuditLog>(), 0, string.Empty);

        var query = _applicationDbContext.LicenseAuditLog
            .Include(l => l.ActionType)
            .Include(l => l.PerformedBy)
                .ThenInclude(u => u.Role)
            .Where(l => l.LicenseID == licenseId)
            .AsNoTracking();

        var totalRecords = await query.CountAsync();
        var logs = await query
            .OrderByDescending(l => l.DateTimestamp)
            .Skip((pageNo - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (logs, totalRecords, license.LicenseName);
    }

    public async Task<LicenseAuditLog?> GetLicenseAuditLogDetailsAsync(int logId)
    {
        return await _applicationDbContext.LicenseAuditLog
            .Include(l => l.License)
            .Include(l => l.Details)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LicenseAuditLogID == logId);
    }
    public async Task<(int Assigned, int Unassigned)> GetLicenseUtilizationAsync(string? licenseType = null)
    {
        var query = _applicationDbContext.License
            .Where(l => l.DeletedDate == null)
            .Include(l => l.LicenseType)
            .Include(l => l.LicenseAssignments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(licenseType))
            query = query.Where(l => l.LicenseType.LicenseTypeName == licenseType);

        var licenses = await query.ToListAsync();

        int assigned   = licenses.Count(l => l.LicenseAssignments.Any(a => a.IsActive));
        int unassigned = licenses.Count(l => !l.LicenseAssignments.Any(a => a.IsActive));

        return (assigned, unassigned);
    }
    public async Task<(int New, int Active, int Expired)> GetLicenseStatusOverviewAsync(string? licenseType = null)
    {
        var today = DateTime.UtcNow.Date;

        var query = _applicationDbContext.License
            .Where(l => l.DeletedDate == null)
            .Include(l => l.LicenseType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(licenseType))
            query = query.Where(l => l.LicenseType.LicenseTypeName == licenseType);

        var licenses = await query.Select(l => new { l.StartDate, l.ExpiryDate }).ToListAsync();

        int newCount     = licenses.Count(l => l.StartDate.Date > today);
        int expiredCount = licenses.Count(l => l.ExpiryDate.HasValue && l.ExpiryDate.Value.Date < today);
        int activeCount  = licenses.Count(l =>
            l.StartDate.Date <= today &&
            (!l.ExpiryDate.HasValue || l.ExpiryDate.Value.Date >= today));

        return (newCount, activeCount, expiredCount);
    }
    public async Task<IEnumerable<(string Month, decimal TotalCost)>> GetLicenseCostOvertimeAsync(int year, string? licenseType = null)
    {
        var query = _applicationDbContext.License
            .Where(l => l.DeletedDate == null && l.PurchaseDate.Year == year)
            .Include(l => l.LicenseType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(licenseType))
            query = query.Where(l => l.LicenseType.LicenseTypeName == licenseType);

        var data = await query
            .GroupBy(l => l.PurchaseDate.Month)
            .Select(g => new { Month = g.Key, TotalCost = g.Sum(l => l.Cost) })
            .ToListAsync();

      
        var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                  "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        return Enumerable.Range(1, 12).Select(m =>
        {
            var found = data.FirstOrDefault(d => d.Month == m);
            return (monthNames[m - 1], found?.TotalCost ?? 0m);
        });
    }

       public async Task<IEnumerable<(string LicenseName, DateTime ExpiryDate, int DaysRemaining)>> GetUpcomingLicenseExpirationsAsync(string? licenseType = null)
    {
        var today   = DateTime.UtcNow.Date;
        var cutoff  = today.AddDays(7);

        var query = _applicationDbContext.License
            .Where(l => l.DeletedDate == null
                     && l.ExpiryDate.HasValue
                     && l.ExpiryDate.Value.Date >= today
                     && l.ExpiryDate.Value.Date <= cutoff)
            .Include(l => l.LicenseType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(licenseType))
            query = query.Where(l => l.LicenseType.LicenseTypeName == licenseType);

        var licenses = await query
            .Select(l => new { l.LicenseName, ExpiryDate = l.ExpiryDate!.Value })
            .ToListAsync();

        return licenses.Select(l => (
            l.LicenseName,
            l.ExpiryDate,
            (l.ExpiryDate.Date - today).Days
        ));
    }

 
    public async Task<IEnumerable<int>> GetAvailableLicenseYearsAsync()
    {
        return await _applicationDbContext.License
            .Where(l => l.DeletedDate == null)
            .Select(l => l.PurchaseDate.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();
    }

    // Get all license reminders with related license and reminder configuration
    public async Task<List<LicenseReminder>> GetAllLicenseRemindersAsync()
    {
        return await _applicationDbContext.LicenseReminder
            .Include(lr => lr.License)
            .Include(lr => lr.ReminderConfig)
            .Where(lr => lr.License.DeletedDate == null)
            .ToListAsync();
    }

}
