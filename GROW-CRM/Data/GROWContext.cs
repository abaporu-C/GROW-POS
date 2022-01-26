using GROW_CRM.Models;
using GROW_CRM.Models.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GROW_CRM.Data
{
    public class GROWContext : DbContext
    {
        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public GROWContext(DbContextOptions<GROWContext> options) : base(options)
        {
            UserName = "SeedData";
        }

        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;        

        public GROWContext(DbContextOptions<GROWContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
            UserName = UserName ?? "Unknown";
        }

        //Datasets
        public DbSet<DietaryRestriction> DietaryRestrictions { get; set; }

        public DbSet<DietaryRestrictionMember> DietaryRestrictionMembers { get; set; }

        public DbSet<DocumentType> DocumentTypes { get; set; }

        public DbSet<Gender> Genders { get; set; }

        public DbSet<Household> Households { get; set; }

        public DbSet<HouseholdDocument> HouseholdDocuments { get; set; }

        public DbSet<HouseholdNotification> HouseholdNotifications { get; set; }

        public DbSet<IncomeSituation> IncomeSituations { get; set; }

        public DbSet<Item> Items { get; set; }        

        public DbSet<Member> Members { get; set; }   
        
        public DbSet<Message> Messages { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<NotificationType> NotificationTypes { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<PaymentType> PaymentTypes { get; set; }

        public DbSet<Province> Provinces { get; set; }

        //Methods

        //Model Builder Event
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("GROW");

            //Adding Composite Keys
            modelBuilder.Entity<DietaryRestrictionMember>()
                .HasKey(dm => new { dm.MemberID, dm.DietaryRestrictionID });

            modelBuilder.Entity<HouseholdNotification>()
                .HasKey(hn => new { hn.HouseholdID, hn.NotificationID });

            

            //Cascading Delete Behavior
            
            //DietaryRestriction-Member
            modelBuilder.Entity<DietaryRestrictionMember>()
                .HasOne(drm => drm.DietaryRestriction)
                .WithMany(dr => dr.DietaryRestrictionMembers)
                .HasForeignKey(drm => drm.DietaryRestrictionID)
                .OnDelete(DeleteBehavior.Restrict);

            //HouseHold
              modelBuilder.Entity<Household>()
                  .HasOne(h => h.Province)
                  .WithMany(p => p.Households)
                  .HasForeignKey(h => h.ProvinceID)
                  .OnDelete(DeleteBehavior.Restrict);


            //HouseHoldDocuments
            modelBuilder.Entity<HouseholdDocument>()
                .HasOne(hd => hd.Household)
                .WithMany(h => h.HouseholdDocuments)
                .HasForeignKey(hd => hd.HouseholdID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HouseholdDocument>()
                .HasOne(hd => hd.DocumentType)
                .WithMany(dt => dt.HouseholdDocuments)
                .HasForeignKey(hd => hd.DocumentTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            //Members
            modelBuilder.Entity<Member>()
                .HasOne(m => m.Gender)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GenderID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Household)
                .WithMany(h => h.Members)
                .HasForeignKey(m => m.HouseholdID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.IncomeSituation)
                .WithMany(i => i.Members)
                .HasForeignKey(m => m.IncomeSituationID)
                .OnDelete(DeleteBehavior.Restrict);

            //Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Member)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MemberID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.PaymentType)
                .WithMany(pt => pt.Orders)
                .HasForeignKey(o => o.PaymentTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            //Order-Item            
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Item)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(oi => oi.ItemID)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
    }
}