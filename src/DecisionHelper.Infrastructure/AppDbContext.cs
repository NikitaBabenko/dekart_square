using DecisionHelper.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace DecisionHelper.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<DecisionSession> DecisionSessions => Set<DecisionSession>();
    public DbSet<UsageCounter> UsageCounters => Set<UsageCounter>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.TelegramId).HasColumnName("telegram_id");
            e.Property(x => x.AnonId).HasColumnName("anon_id");
            e.Property(x => x.Locale).HasColumnName("locale").IsRequired().HasMaxLength(8);
            e.Property(x => x.IsPremium).HasColumnName("is_premium");
            e.Property(x => x.PremiumUntil).HasColumnName("premium_until");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.MergedIntoUserId).HasColumnName("merged_into_user_id");
            e.HasIndex(x => x.TelegramId).IsUnique().HasFilter("telegram_id IS NOT NULL");
            e.HasIndex(x => x.AnonId).IsUnique().HasFilter("anon_id IS NOT NULL");
        });

        b.Entity<DecisionSession>(e =>
        {
            e.ToTable("decision_sessions");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Dilemma).HasColumnName("dilemma").IsRequired();
            e.Property(x => x.SquareJson).HasColumnName("square").HasColumnType("jsonb").IsRequired();
            e.Property(x => x.Summary).HasColumnName("summary");
            e.Property(x => x.Locale).HasColumnName("locale").IsRequired().HasMaxLength(8);
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.UserId);
        });

        b.Entity<UsageCounter>(e =>
        {
            e.ToTable("usage_counters");
            e.HasKey(x => new { x.UserId, x.PeriodKind, x.PeriodKey });
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.PeriodKind).HasColumnName("period_kind").HasConversion<short>();
            e.Property(x => x.PeriodKey).HasColumnName("period_key").IsRequired().HasMaxLength(10);
            e.Property(x => x.Count).HasColumnName("count");
        });

        b.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.TelegramPaymentChargeId).HasColumnName("telegram_payment_charge_id").IsRequired();
            e.Property(x => x.Stars).HasColumnName("stars");
            e.Property(x => x.PremiumDays).HasColumnName("premium_days");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.HasIndex(x => x.TelegramPaymentChargeId).IsUnique();
            e.HasIndex(x => x.UserId);
        });
    }
}
