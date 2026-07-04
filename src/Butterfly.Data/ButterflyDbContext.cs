using System.Text.Json;
using Butterfly.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Butterfly.Data;

/// <summary>
/// EF Core context for Butterfly. Code-first against Azure SQL (LocalDB/local SQL Server for dev).
/// Tag lists are stored as JSON columns (documented per-property) rather than join tables.
/// </summary>
public class ButterflyDbContext : DbContext
{
    public ButterflyDbContext(DbContextOptions<ButterflyDbContext> options) : base(options) { }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Mentor> Mentors => Set<Mentor>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();
    public DbSet<CareManager> CareManagers => Set<CareManager>();
    public DbSet<MenteeProfile> MenteeProfiles => Set<MenteeProfile>();
    public DbSet<Mentorship> Mentorships => Set<Mentorship>();
    public DbSet<ImpactUpdate> ImpactUpdates => Set<ImpactUpdate>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // JSON <-> List<string> converter for tag collections (no separate tag table).
        var stringListConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        // Structural comparer so EF change-tracking detects edits to the list contents.
        var stringListComparer = new ValueComparer<List<string>>(
            (a, c) => (a ?? new List<string>()).SequenceEqual(c ?? new List<string>()),
            v => v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v.ToList());

        // ---- AppUser ----
        b.Entity<AppUser>(e =>
        {
            e.Property(x => x.EntraObjectId).IsRequired().HasMaxLength(64);
            e.HasIndex(x => x.EntraObjectId).IsUnique();
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);

            e.HasOne(x => x.Mentor).WithOne(m => m.AppUser)
                .HasForeignKey<Mentor>(m => m.AppUserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CareManager).WithOne(c => c.AppUser)
                .HasForeignKey<CareManager>(c => c.AppUserId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Mentor ----
        b.Entity<Mentor>(e =>
        {
            e.Property(x => x.Country).IsRequired().HasMaxLength(2).HasDefaultValue("US");
            e.HasIndex(x => x.AppUserId).IsUnique();
            e.HasOne(x => x.Survey).WithOne(s => s.Mentor)
                .HasForeignKey<SurveyResponse>(s => s.MentorId).OnDelete(DeleteBehavior.Cascade);
        });

        // ---- SurveyResponse ----
        b.Entity<SurveyResponse>(e =>
        {
            e.HasIndex(x => x.MentorId).IsUnique(); // one current survey per mentor
            e.Property(x => x.Values).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            e.Property(x => x.Interests).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            e.Property(x => x.PreferredTalentCategory).HasConversion<string?>().HasMaxLength(20);
        });

        // ---- CareManager ----
        b.Entity<CareManager>(e =>
        {
            e.Property(x => x.Region).IsRequired().HasMaxLength(120);
            e.HasIndex(x => x.AppUserId).IsUnique();
        });

        // ---- MenteeProfile ----
        b.Entity<MenteeProfile>(e =>
        {
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(60);
            e.Property(x => x.Region).IsRequired().HasMaxLength(120);
            e.Property(x => x.Story).IsRequired().HasMaxLength(2000);
            e.Property(x => x.PhotoUrl).HasMaxLength(500);
            e.Property(x => x.RejectionReason).HasMaxLength(1000);
            e.Property(x => x.TalentCategory).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.SupportNeeded).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.MonthlyNeedBDT).HasPrecision(18, 2);
            e.Property(x => x.Tags).HasConversion(stringListConverter).Metadata.SetValueComparer(stringListComparer);
            e.HasIndex(x => x.Status); // Mentor match queries filter on Approved

            e.HasOne(x => x.CreatedByCareManager).WithMany(c => c.ManagedProfiles)
                .HasForeignKey(x => x.CreatedByCareManagerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ApprovedByAdmin).WithMany()
                .HasForeignKey(x => x.ApprovedByAdminId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Mentorship ----
        b.Entity<Mentorship>(e =>
        {
            e.Property(x => x.RelationshipType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.MeetingCadence).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.MonthlyAmountUSD).HasPrecision(18, 2);
            e.HasIndex(x => x.MentorId);
            e.HasIndex(x => new { x.MentorId, x.MenteeProfileId }).IsUnique(); // one mentorship per pair

            e.HasOne(x => x.Mentor).WithMany(m => m.Mentorships)
                .HasForeignKey(x => x.MentorId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.MenteeProfile).WithMany(p => p.Mentorships)
                .HasForeignKey(x => x.MenteeProfileId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ImpactUpdate ----
        b.Entity<ImpactUpdate>(e =>
        {
            e.Property(x => x.UpdateType).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.SpendDescription).HasMaxLength(1000);
            e.Property(x => x.SessionSummary).HasMaxLength(2000);
            e.Property(x => x.ImpactNote).IsRequired().HasMaxLength(2000);
            e.Property(x => x.PhotoUrl).HasMaxLength(500);
            e.Property(x => x.AmountSpentBDT).HasPrecision(18, 2);
            e.HasIndex(x => x.MentorshipId);

            e.HasOne(x => x.Mentorship).WithMany(m => m.ImpactUpdates)
                .HasForeignKey(x => x.MentorshipId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CareManager).WithMany(c => c.ImpactUpdates)
                .HasForeignKey(x => x.CareManagerId).OnDelete(DeleteBehavior.Restrict);
        });

        // ---- Payment ----
        b.Entity<Payment>(e =>
        {
            e.Property(x => x.AmountUSD).HasPrecision(18, 2);
            e.Property(x => x.Method).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ExternalRef).HasMaxLength(200);
            e.HasIndex(x => x.MentorshipId);

            e.HasOne(x => x.Mentorship).WithMany(m => m.Payments)
                .HasForeignKey(x => x.MentorshipId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        StampAudits();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAudits();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void StampAudits()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
