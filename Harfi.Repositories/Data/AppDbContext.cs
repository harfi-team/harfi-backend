using Harfi.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Data;

public class AppDbContext : IdentityUserContext<User, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets ────────────────────────────────────────────────
    // Users is already provided by IdentityUserContext<User, int>
    public DbSet<Craftsman> Craftsmen { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AIChatMessage> AIChatMessages { get; set; }
    public DbSet<RAGDocument> RAGDocuments { get; set; }
    public DbSet<MediaFile> MediaFiles { get; set; }
    public DbSet<JobFeedback> JobFeedbacks { get; set; }
    public DbSet<UserConnection> UserConnections { get; set; }
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
    public DbSet<PhoneVerification> PhoneVerifications => Set<PhoneVerification>();
    public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<FeatureFlag> FeatureFlags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── USERS ────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users");
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Role).HasDatabaseName("IX_Users_Role");
            e.HasIndex(u => u.IsDeleted).HasDatabaseName("IX_Users_IsDeleted");
            e.HasIndex(u => u.IsActive).HasDatabaseName("IX_Users_IsActive");
            e.Property(u => u.IsActive).HasDefaultValue(true);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── CRAFTSMEN ────────────────────────────────────────
        modelBuilder.Entity<Craftsman>(e =>
        {
            e.HasIndex(c => c.UserId).IsUnique(); // 1:1 with Users

            e.HasOne(c => c.User)
             .WithOne(u => u.CraftsmanProfile)
             .HasForeignKey<Craftsman>(c => c.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(c => c.IsApproved).HasDefaultValue(false);
            e.Property(c => c.IsAvailable).HasDefaultValue(true);
            e.Property(c => c.Rating).HasDefaultValue(0m);
            e.Property(c => c.Experience).HasDefaultValue(0);
            e.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(c => c.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── JOBS ─────────────────────────────────────────────
        modelBuilder.Entity<Job>(e =>
        {
            e.HasOne(j => j.Customer)
             .WithMany(u => u.JobsAsCustomer)
             .HasForeignKey(j => j.CustomerId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(j => j.Craftsman)
             .WithMany(c => c.Jobs)
             .HasForeignKey(j => j.CraftsmanId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(j => j.Status).HasDefaultValue("مفتوح");
            e.Property(j => j.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(j => j.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Index for frequent filtering by status
            e.HasIndex(j => j.Status);
            e.HasIndex(j => j.CraftsmanId);
        });

        // ── CONVERSATIONS ─────────────────────────────────────
        modelBuilder.Entity<Conversation>(e =>
        {
            e.HasIndex(c => c.JobId).IsUnique(); // 1:1 with Jobs

            e.HasOne(c => c.Job)
             .WithOne(j => j.Conversation)
             .HasForeignKey<Conversation>(c => c.JobId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Customer)
             .WithMany()
             .HasForeignKey(c => c.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Craftsman)
             .WithMany(cr => cr.Conversations)
             .HasForeignKey(c => c.CraftsmanId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(c => c.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── MESSAGES ─────────────────────────────────────────
        modelBuilder.Entity<Message>(e =>
        {
            e.HasOne(m => m.Conversation)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ConversationId)
             .OnDelete(DeleteBehavior.Cascade); // deleting conversation deletes messages

            e.HasOne(m => m.Sender)
             .WithMany(u => u.SentMessages)
             .HasForeignKey(m => m.SenderId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(m => m.MessageType).HasDefaultValue("text");
            e.Property(m => m.IsRead).HasDefaultValue(false);
            e.Property(m => m.SentAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── REVIEWS ──────────────────────────────────────────
        modelBuilder.Entity<Review>(e =>
        {
            e.HasIndex(r => r.JobId).IsUnique(); // one review per job

            e.HasOne(r => r.Job)
             .WithOne(j => j.Review)
             .HasForeignKey<Review>(r => r.JobId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Customer)
             .WithMany()
             .HasForeignKey(r => r.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Craftsman)
             .WithMany(c => c.Reviews)
             .HasForeignKey(r => r.CraftsmanId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── NOTIFICATIONS ─────────────────────────────────────
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasOne(n => n.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(n => n.UserId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(n => n.RelatedJob)
             .WithMany(j => j.Notifications)
             .HasForeignKey(n => n.RelatedJobId)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(n => n.IsRead).HasDefaultValue(false);
            e.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── REFRESH TOKENS ────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasIndex(rt => rt.Token).IsUnique();

            e.HasOne(rt => rt.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(rt => rt.UserId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(rt => rt.IsRevoked).HasDefaultValue(false);
            e.Property(rt => rt.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── AI CHAT MESSAGES ──────────────────────────────────
        modelBuilder.Entity<AIChatMessage>(e =>
        {
            e.ToTable("AIChatMessages");

            e.HasOne(a => a.User)
             .WithMany(u => u.AIChatMessages)
             .HasForeignKey(a => a.UserId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(a => a.SessionId); // frequently queried to load session history

            e.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── RAG DOCUMENTS ─────────────────────────────────────
        modelBuilder.Entity<RAGDocument>(e =>
        {
            e.HasOne(r => r.Job)
             .WithMany(j => j.RAGDocuments)
             .HasForeignKey(r => r.JobId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(r => r.EmbeddingModel).HasDefaultValue("text-embedding-3-small");
            e.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── MEDIA FILES ───────────────────────────────────────
        modelBuilder.Entity<MediaFile>(e =>
        {
            e.HasOne(mf => mf.Uploader)
             .WithMany(u => u.UploadedFiles)
             .HasForeignKey(mf => mf.UploadedBy)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(mf => mf.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── JOB FEEDBACK ──────────────────────────────────────
        modelBuilder.Entity<JobFeedback>(e =>
        {
            e.HasOne(jf => jf.User)
             .WithMany()
             .HasForeignKey(jf => jf.UserId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(jf => jf.RAGDocument)
             .WithMany(r => r.Feedbacks)
             .HasForeignKey(jf => jf.RAGDocumentId)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(jf => jf.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── USER CONNECTIONS ──────────────────────────────────
        modelBuilder.Entity<UserConnection>(e =>
        {
            e.HasIndex(uc => uc.ConnectionId).IsUnique();

            e.HasOne(uc => uc.User)
             .WithMany(u => u.UserConnections)
             .HasForeignKey(uc => uc.UserId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Cascade);

            e.Property(uc => uc.IsConnected).HasDefaultValue(true);
            e.Property(uc => uc.ConnectedAt).HasDefaultValueSql("GETUTCDATE()");
            e.Property(uc => uc.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── EMAIL VERIFICATION ─────────────────────────────────
        modelBuilder.Entity<EmailVerification>(e =>
        {
            e.HasOne(ev => ev.User)
             .WithMany(u => u.EmailVerifications)
             .HasForeignKey(ev => ev.UserId)
             .IsRequired(false);
        });

        // ── PHONE VERIFICATION ─────────────────────────────────
        modelBuilder.Entity<PhoneVerification>(e =>
        {
            e.HasOne(pv => pv.User)
             .WithMany()
             .HasForeignKey(pv => pv.UserId)
             .IsRequired(false);
        });

        // ── GLOBAL QUERY FILTERS (soft-delete + required nav guards) ─
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Craftsman>().HasQueryFilter(c => !c.IsDeleted && !c.User.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted && !r.Customer.IsDeleted && !r.Craftsman.IsDeleted);
        modelBuilder.Entity<Conversation>().HasQueryFilter(c => !c.Customer.IsDeleted);

        // ── ADMIN AUDIT LOGS ──────────────────────────────────
        modelBuilder.Entity<AdminAuditLog>(e =>
        {
            e.ToTable("AdminAuditLogs");
            e.HasOne(a => a.Admin)
             .WithMany()
             .HasForeignKey(a => a.AdminId)
             .IsRequired(false)
             .OnDelete(DeleteBehavior.Restrict);
            e.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasIndex(a => a.AdminId);
            e.HasIndex(a => a.Action);
            e.HasIndex(a => a.TargetType);
            e.HasIndex(a => a.CreatedAt);
        });

        // ── REPORTS ──────────────────────────────────────────
        modelBuilder.Entity<Report>(e =>
        {
            e.ToTable("Reports");
            e.Property(r => r.Status).HasDefaultValue("pending");
            e.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasIndex(r => r.Status);
            e.HasIndex(r => r.TargetType);
        });

        // ── SERVICE TYPES ─────────────────────────────────────
        modelBuilder.Entity<ServiceType>(e =>
        {
            e.ToTable("ServiceTypes");
            e.Property(s => s.IsActive).HasDefaultValue(true);
            e.HasIndex(s => s.NameAr).IsUnique();
            e.HasIndex(s => s.NameEn).IsUnique();
        });

        // ── CITIES ───────────────────────────────────────────
        modelBuilder.Entity<City>(e =>
        {
            e.ToTable("Cities");
            e.Property(c => c.IsActive).HasDefaultValue(true);
            e.HasIndex(c => c.NameAr).IsUnique();
            e.HasIndex(c => c.NameEn).IsUnique();
        });

        // ── FEATURE FLAGS ────────────────────────────────────
        modelBuilder.Entity<FeatureFlag>(e =>
        {
            e.ToTable("FeatureFlags");
            e.Property(f => f.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
