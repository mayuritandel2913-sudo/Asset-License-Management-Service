using AssetManagement.Infrastructure.Entities;
using AssetManagement.Infrastructure.Entities.License;
using AssetManagement.Infrastructure.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace AssetManagement.Infrastructure.Data;
 
 
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
 
    public DbSet<Asset> Asset { get; set; }
    public DbSet<AssetStatus> AssetStatus { get; set; }
    public DbSet<AssetHealthStatus> AssetHealthStatus { get; set; }
    public DbSet<AssetProperty> AssetProperty { get; set; }
    public DbSet<AssetPropertyValue> AssetPropertyValue { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<ResourceType> ResourceType { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Role> Role { get; set; }
    public DbSet<AssignmentDetails> AssignmentDetails { get; set; }
    public DbSet<DataType> DataType { get; set; }
    public DbSet<ActionType> ActionType { get; set; }
    public DbSet<Department> Department { get; set; }
    public DbSet<License> License { get; set; }
    public DbSet<LicenseStatus> LicenseStatus { get; set; }
    public DbSet<LicenseType> LicenseType { get; set; }
    public DbSet<LicensePurchaseType> LicensePurchaseType { get; set; }
    public DbSet<ReminderConfig> ReminderConfig { get; set; }
    public DbSet<LicenseReminder> LicenseReminder { get; set; }
    public DbSet<LicenseAssignment> LicenseAssignment { get; set; }
    public DbSet<LicenseRenewal> LicenseRenewal { get; set; }
    public DbSet<LicenseAuditLog> LicenseAuditLog { get; set; }
    public DbSet<LicenseAuditLogDetail> LicenseAuditLogDetail { get; set; }
    public DbSet<AssetAuditLog> AssetAuditLog { get; set; }
    public DbSet<AssetAuditLogDetail> AssetAuditLogDetail { get; set;}

      public DbSet<NotificationStatus> NotificationStatus { get; set; }
    public DbSet<NotificationType> NotificationType { get; set; }
    public DbSet<Notification> Notification { get; set; }

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.SqlServerEventId.ByteIdentityColumnWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            var licenseEntity = modelBuilder.Entity<License>();
            licenseEntity.ToTable(tb => 
            {
                tb.HasTrigger("trg_License_Created");
                tb.HasTrigger("trg_License_Updated_Deleted");
            });

            var assetEntity = modelBuilder.Entity<Asset>();
            assetEntity.ToTable(tb => 
            {
                tb.HasTrigger("trg_Asset_Created");
                tb.HasTrigger("trg_Asset_Updated");
                tb.HasTrigger("trg_Asset_Updated_Deleted");
            });

            var licenseAssignmentEntity = modelBuilder.Entity<LicenseAssignment>();
            licenseAssignmentEntity.ToTable(tb => 
            {
                tb.HasTrigger("trg_License_Assigned");
                tb.HasTrigger("trg_License_Unassigned");
            });

            var assignmentDetailsEntity = modelBuilder.Entity<AssignmentDetails>();
            assignmentDetailsEntity.ToTable(tb => 
            {
                tb.HasTrigger("trg_Asset_Assigned");
                tb.HasTrigger("trg_Asset_Unassigned");
                tb.HasTrigger("trg_AssignmentDetails_Updated");
            });

            var licenseRenewalEntity = modelBuilder.Entity<LicenseRenewal>();
            licenseRenewalEntity.ToTable(tb => tb.HasTrigger("trg_License_Renewed"));
        }
}
