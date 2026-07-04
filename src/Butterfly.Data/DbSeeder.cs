using Butterfly.Data.Entities;
using Butterfly.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Butterfly.Data;

/// <summary>
/// DEV-ONLY seed data so the app is demoable without a live Entra tenant. Seeds a placeholder
/// care manager + admin <see cref="AppUser"/> and 3–4 <see cref="ProfileStatus.Approved"/> mentee
/// profiles (plus one Pending, to exercise the admin queue).
///
/// Does NOT seed credentials or roles — those live in Entra. The placeholder EntraObjectIds here
/// will be reconciled/replaced when real users first authenticate via <c>GET /api/me</c>.
/// Call only when the host environment is Development.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(ButterflyDbContext db)
    {
        if (await db.MenteeProfiles.AnyAsync())
            return; // already seeded

        var careManagerUser = new AppUser
        {
            EntraObjectId = "seed-caremanager-0001",
            Email = "care.manager@butterfly.dev",
            DisplayName = "Rahima (Care Manager)",
            Role = UserRole.CareManager
        };
        var adminUser = new AppUser
        {
            EntraObjectId = "seed-admin-0001",
            Email = "admin@butterfly.dev",
            DisplayName = "Butterfly Admin",
            Role = UserRole.Admin
        };

        var careManager = new CareManager
        {
            AppUser = careManagerUser,
            Region = "Rangpur Division",
            IsVerified = true
        };

        db.AppUsers.AddRange(careManagerUser, adminUser);
        db.CareManagers.Add(careManager);

        var profiles = new List<MenteeProfile>
        {
            new()
            {
                DisplayName = "Ayaan",
                Age = 14,
                Region = "Kurigram",
                TalentCategory = TalentCategory.Athlete,
                Story = "A promising sprinter who trains barefoot before school. Dreams of competing nationally.",
                Tags = new() { "sports", "discipline", "resilience", "running" },
                SupportNeeded = SupportNeeded.Both,
                MonthlyNeedBDT = 4000m,
                Status = ProfileStatus.Approved,
                CreatedByCareManager = careManager,
                ApprovedByAdmin = adminUser
            },
            new()
            {
                DisplayName = "Nusrat",
                Age = 15,
                Region = "Gaibandha",
                TalentCategory = TalentCategory.Musician,
                Story = "Self-taught on a borrowed harmonium; composes songs about her village and river life.",
                Tags = new() { "music", "creativity", "culture", "arts" },
                SupportNeeded = SupportNeeded.Mentorship,
                MonthlyNeedBDT = null,
                Status = ProfileStatus.Approved,
                CreatedByCareManager = careManager,
                ApprovedByAdmin = adminUser
            },
            new()
            {
                DisplayName = "Tania",
                Age = 13,
                Region = "Lalmonirhat",
                TalentCategory = TalentCategory.Student,
                Story = "Top of her class in mathematics; walks 5km each way to school and never misses a day.",
                Tags = new() { "education", "mathematics", "discipline", "stem" },
                SupportNeeded = SupportNeeded.Financial,
                MonthlyNeedBDT = 3000m,
                Status = ProfileStatus.Approved,
                CreatedByCareManager = careManager,
                ApprovedByAdmin = adminUser
            },
            new()
            {
                DisplayName = "Rafi",
                Age = 16,
                Region = "Nilphamari",
                TalentCategory = TalentCategory.Artist,
                Story = "Paints murals on his school's walls using homemade pigments. Wants to study fine art.",
                Tags = new() { "arts", "creativity", "painting", "culture" },
                SupportNeeded = SupportNeeded.Both,
                MonthlyNeedBDT = 3500m,
                // Left Pending on purpose so the Admin approval queue has something to show.
                Status = ProfileStatus.Pending,
                CreatedByCareManager = careManager
            }
        };

        db.MenteeProfiles.AddRange(profiles);
        await db.SaveChangesAsync();
    }
}
