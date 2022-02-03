﻿// <auto-generated />
using System;
using GROW_CRM.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GROW_CRM.Data.GROWMigrations
{
    [DbContext(typeof(GROWContext))]
    partial class GROWContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.21");

            modelBuilder.Entity("GROW_CRM.Models.City", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Cities");
                });

            modelBuilder.Entity("GROW_CRM.Models.DietaryRestriction", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Restriction")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("DietaryRestrictions");
                });

            modelBuilder.Entity("GROW_CRM.Models.DietaryRestrictionMember", b =>
                {
                    b.Property<int>("MemberID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DietaryRestrictionID")
                        .HasColumnType("INTEGER");

                    b.HasKey("MemberID", "DietaryRestrictionID");

                    b.HasIndex("DietaryRestrictionID");

                    b.ToTable("DietaryRestrictionMembers");
                });

            modelBuilder.Entity("GROW_CRM.Models.DocumentType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("DocumentTypes");
                });

            modelBuilder.Entity("GROW_CRM.Models.Gender", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Genders");
                });

            modelBuilder.Entity("GROW_CRM.Models.Household", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AptNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CityID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<int>("HouseholdStatusID")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LICOVerified")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastVerification")
                        .HasColumnType("TEXT");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ProvinceID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StreetName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(100);

                    b.Property<int>("StreetNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("CityID");

                    b.HasIndex("HouseholdStatusID");

                    b.HasIndex("ProvinceID");

                    b.ToTable("Households");
                });

            modelBuilder.Entity("GROW_CRM.Models.HouseholdNotification", b =>
                {
                    b.Property<int>("HouseholdID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NotificationID")
                        .HasColumnType("INTEGER");

                    b.HasKey("HouseholdID", "NotificationID");

                    b.HasIndex("NotificationID");

                    b.ToTable("HouseholdNotifications");
                });

            modelBuilder.Entity("GROW_CRM.Models.HouseholdStatus", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("HouseholdStatuses");
                });

            modelBuilder.Entity("GROW_CRM.Models.IncomeSituation", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Situation")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("IncomeSituations");
                });

            modelBuilder.Entity("GROW_CRM.Models.Item", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("GROW_CRM.Models.Member", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DOB")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(255);

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<int>("GenderID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("HouseholdID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("IncomeSituationID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(100);

                    b.Property<string>("MiddleName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT")
                        .HasMaxLength(2000);

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasMaxLength(10);

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.Property<double>("YearlyIncome")
                        .HasColumnType("REAL");

                    b.HasKey("ID");

                    b.HasIndex("GenderID");

                    b.HasIndex("HouseholdID");

                    b.HasIndex("IncomeSituationID");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("GROW_CRM.Models.Message", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("GROW_CRM.Models.Notification", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("MessageID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("NotificationTypeID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("MessageID");

                    b.HasIndex("NotificationTypeID");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("GROW_CRM.Models.NotificationType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("NotificationTypes");
                });

            modelBuilder.Entity("GROW_CRM.Models.Order", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int>("MemberID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PaymentTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Subtotal")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Taxes")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Total")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("MemberID");

                    b.HasIndex("PaymentTypeID");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("GROW_CRM.Models.OrderItem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ItemID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OrderID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Quantity")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("ItemID");

                    b.HasIndex("OrderID");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("GROW_CRM.Models.PaymentType", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("PaymentTypes");
                });

            modelBuilder.Entity("GROW_CRM.Models.Province", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Code")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Provinces");
                });

            modelBuilder.Entity("GROW_CRM.Models.Utilities.FileContent", b =>
                {
                    b.Property<int>("FileContentID")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Content")
                        .HasColumnType("BLOB");

                    b.Property<string>("MimeType")
                        .HasColumnType("TEXT")
                        .HasMaxLength(255);

                    b.HasKey("FileContentID");

                    b.ToTable("FileContent");
                });

            modelBuilder.Entity("GROW_CRM.Models.Utilities.UploadedFile", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(255);

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("UploadedFiles");

                    b.HasDiscriminator<string>("Discriminator").HasValue("UploadedFile");
                });

            modelBuilder.Entity("GROW_CRM.Models.MemberDocument", b =>
                {
                    b.HasBaseType("GROW_CRM.Models.Utilities.UploadedFile");

                    b.Property<int>("DocumentTypeID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MemberID")
                        .HasColumnType("INTEGER");

                    b.HasIndex("DocumentTypeID");

                    b.HasIndex("MemberID");

                    b.HasDiscriminator().HasValue("MemberDocument");
                });

            modelBuilder.Entity("GROW_CRM.Models.DietaryRestrictionMember", b =>
                {
                    b.HasOne("GROW_CRM.Models.DietaryRestriction", "DietaryRestriction")
                        .WithMany("DietaryRestrictionMembers")
                        .HasForeignKey("DietaryRestrictionID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.Member", "Member")
                        .WithMany("DietaryRestrictionMembers")
                        .HasForeignKey("MemberID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GROW_CRM.Models.Household", b =>
                {
                    b.HasOne("GROW_CRM.Models.City", "City")
                        .WithMany("Households")
                        .HasForeignKey("CityID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.HouseholdStatus", "HouseholdStatus")
                        .WithMany("Households")
                        .HasForeignKey("HouseholdStatusID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.Province", "Province")
                        .WithMany("Households")
                        .HasForeignKey("ProvinceID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("GROW_CRM.Models.HouseholdNotification", b =>
                {
                    b.HasOne("GROW_CRM.Models.Household", "Household")
                        .WithMany("HouseholdNotifications")
                        .HasForeignKey("HouseholdID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.Notification", "Notification")
                        .WithMany("HouseholdNotifications")
                        .HasForeignKey("NotificationID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GROW_CRM.Models.Member", b =>
                {
                    b.HasOne("GROW_CRM.Models.Gender", "Gender")
                        .WithMany("Members")
                        .HasForeignKey("GenderID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.Household", "Household")
                        .WithMany("Members")
                        .HasForeignKey("HouseholdID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.IncomeSituation", "IncomeSituation")
                        .WithMany("Members")
                        .HasForeignKey("IncomeSituationID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("GROW_CRM.Models.Notification", b =>
                {
                    b.HasOne("GROW_CRM.Models.Message", "Message")
                        .WithMany("Notifications")
                        .HasForeignKey("MessageID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.NotificationType", "NotificationType")
                        .WithMany("Notifications")
                        .HasForeignKey("NotificationTypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GROW_CRM.Models.Order", b =>
                {
                    b.HasOne("GROW_CRM.Models.Member", "Member")
                        .WithMany("Orders")
                        .HasForeignKey("MemberID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.PaymentType", "PaymentType")
                        .WithMany("Orders")
                        .HasForeignKey("PaymentTypeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("GROW_CRM.Models.OrderItem", b =>
                {
                    b.HasOne("GROW_CRM.Models.Item", "Item")
                        .WithMany("OrderItems")
                        .HasForeignKey("ItemID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.Order", "Order")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GROW_CRM.Models.Utilities.FileContent", b =>
                {
                    b.HasOne("GROW_CRM.Models.Utilities.UploadedFile", "UploadedFile")
                        .WithOne("FileContent")
                        .HasForeignKey("GROW_CRM.Models.Utilities.FileContent", "FileContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GROW_CRM.Models.MemberDocument", b =>
                {
                    b.HasOne("GROW_CRM.Models.DocumentType", "DocumentType")
                        .WithMany("MemberDocuments")
                        .HasForeignKey("DocumentTypeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("GROW_CRM.Models.Member", "Member")
                        .WithMany("MemberDocuments")
                        .HasForeignKey("MemberID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
