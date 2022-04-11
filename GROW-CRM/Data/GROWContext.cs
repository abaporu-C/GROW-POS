using GROW_CRM.Models;
using GROW_CRM.Models.Interfaces;
using GROW_CRM.Models.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GROW_CRM.ViewModels.ReportsViewModels;
using GROW_CRM.ViewModels;

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
        public DbSet<About> Abouts { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<DietaryRestriction> DietaryRestrictions { get; set; }

        public DbSet<DietaryRestrictionMember> DietaryRestrictionMembers { get; set; }

        public DbSet<DocumentType> DocumentTypes { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Gender> Genders { get; set; }

        public DbSet<HealthIssueType> HealthIssueTypes { get; set; }

        public DbSet<Household> Households { get; set; }

        public DbSet<HouseholdNotification> HouseholdNotifications { get; set; }

        public DbSet<IncomeSituation> IncomeSituations { get; set; }

        public DbSet<Item> Items { get; set; }        

        public DbSet<Message> Messages { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<MemberDocument> MemberDocuments { get; set; }

        public DbSet<MemberIncomeSituation> MemberIncomeSituations { get; set; }

        //public DbSet<Message> Messages { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<NotificationType> NotificationTypes { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<PaymentType> PaymentTypes { get; set; }

        public DbSet<Province> Provinces { get; set; }

        public DbSet<HouseholdStatus> HouseholdStatuses { get; set; }

        public DbSet<RoleWithUserVM> RolesWithUsers { get; set; }

        public DbSet<UploadedFile> UploadedFiles { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        //Methods

        //Model Builder Event
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.HasDefaultSchema("GROW");

            //Default values
            modelBuilder.Entity<Household>()
            .Property(p => p.HasCustomLICO)
            .HasDefaultValue(false);

            //Adding Unique Constraints
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.Code)
                .IsUnique();

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.Name)
                .IsUnique();

            //Add a unique index to the Employee Email
            modelBuilder.Entity<Employee>()
            .HasIndex(a => new { a.Email })
            .IsUnique();

            //For the RolesWithUsers View
            modelBuilder
                .Entity<RoleWithUserVM>()
                .ToView(nameof(RolesWithUsers))
                .HasNoKey();

            //Adding Composite Keys
            modelBuilder.Entity<DietaryRestrictionMember>()
                .HasKey(dm => new { dm.MemberID, dm.DietaryRestrictionID });

            modelBuilder.Entity<HouseholdNotification>()
                .HasKey(hn => new { hn.HouseholdID, hn.NotificationID });

            modelBuilder.Entity<MemberIncomeSituation>()
                .HasIndex(mis => new { mis.MemberID, mis.IncomeSituationID})
                .IsUnique();


            //Cascading Delete Behavior


            //DietaryRestrictions
            modelBuilder.Entity<DietaryRestriction>()
                .HasOne(dr => dr.HealthIssueType)
                .WithMany(hit => hit.DietaryRestrictions)
                .HasForeignKey(dr => dr.HealthIssueTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            //DietaryRestriction-Member
            modelBuilder.Entity<DietaryRestrictionMember>()
                .HasOne(drm => drm.DietaryRestriction)
                .WithMany(dr => dr.DietaryRestrictionMembers)
                .HasForeignKey(drm => drm.DietaryRestrictionID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DietaryRestrictionMember>()
                .HasOne(drm => drm.Member)
                .WithMany(dr => dr.DietaryRestrictionMembers)
                .HasForeignKey(drm => drm.MemberID)
                .OnDelete(DeleteBehavior.Cascade);

            //Items
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            //HouseHold
              modelBuilder.Entity<Household>()
                  .HasOne(h => h.Province)
                  .WithMany(p => p.Households)
                  .HasForeignKey(h => h.ProvinceID)
                  .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Household>()
                .HasOne(h => h.City)
                .WithMany(c => c.Households)
                .HasForeignKey(h => h.CityID)
                .OnDelete(DeleteBehavior.Restrict);

            //MemberDocuments            
            modelBuilder.Entity<MemberDocument>()
                .HasOne(hd => hd.DocumentType)
                .WithMany(dt => dt.MemberDocuments)
                .HasForeignKey(hd => hd.DocumentTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            //MemberIncomeSituations
            modelBuilder.Entity<MemberIncomeSituation>()
                .HasOne(m => m.Member)
                .WithMany(ics => ics.MemberIncomeSituations)
                .HasForeignKey(m => m.MemberID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MemberIncomeSituation>()
                .HasOne(mis => mis.IncomeSituation)
                .WithMany(ics => ics.MemberIncomeSituations)
                .HasForeignKey(mis => mis.IncomeSituationID)
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

            //Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Member)
                .WithMany(m => m.Orders)
                .HasForeignKey(o => o.MemberID)
                .OnDelete(DeleteBehavior.Cascade);

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

           /* //About          
            modelBuilder.Entity<About>()
                .HasOne(oi => oi.Item)
                .WithMany(i => i.OrderItems)
                .HasForeignKey(oi => oi.ItemID)
                .OnDelete(DeleteBehavior.Restrict);*/
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
                    var now = DateTime.Now;
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
