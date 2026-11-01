using Enhanzer.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<LocationDetail> LocationDetails => Set<LocationDetail>();
    public DbSet<BillHeader> BillHeaders => Set<BillHeader>();
    public DbSet<BillNote> BillNotes => Set<BillNote>();
    public DbSet<BillDocument> BillDocuments => Set<BillDocument>();
    public DbSet<BillAccount> BillAccounts => Set<BillAccount>();
    public DbSet<BillTax> BillTaxes => Set<BillTax>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Location Details
        modelBuilder.Entity<LocationDetail>(entity =>
        {
            entity.ToTable("Location_Details");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyCode).HasMaxLength(100).HasDefaultValue(string.Empty);
            entity.Property(e => e.LocationCode).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LocationName).HasMaxLength(250).IsRequired();
            entity.HasIndex(e => new { e.CompanyCode, e.LocationCode }).IsUnique().HasDatabaseName("UQ_LocationDetails_Company_LocationCode");
            entity.HasIndex(e => e.LocationName).HasDatabaseName("IX_LocationDetails_LocationName");
            entity.HasIndex(e => e.UserEmail).HasDatabaseName("IX_LocationDetails_UserEmail");
        });

        // Bill Headers
        modelBuilder.Entity<BillHeader>(entity =>
        {
            entity.ToTable("Bill_Headers");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyCode, e.BillNumber }).IsUnique().HasDatabaseName("UQ_BillHeaders_Company_BillNo");
        });

        // Bill Notes
        modelBuilder.Entity<BillNote>(entity =>
        {
            entity.ToTable("Bill_Notes");
            entity.HasKey(e => e.Id);
        });

        // Bill Documents
        modelBuilder.Entity<BillDocument>(entity =>
        {
            entity.ToTable("Bill_Documents");
            entity.HasKey(e => e.Id);
        });

        // Bill Accounts
        modelBuilder.Entity<BillAccount>(entity =>
        {
            entity.ToTable("Bill_Accounts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyCode, e.AccountCode }).IsUnique().HasDatabaseName("UQ_BillAccounts_Company_Code");
        });

        // Bill Taxes
        modelBuilder.Entity<BillTax>(entity =>
        {
            entity.ToTable("Bill_Taxes");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyCode, e.TaxCode }).IsUnique().HasDatabaseName("UQ_BillTaxes_Company_Code");
        });
    }
}
