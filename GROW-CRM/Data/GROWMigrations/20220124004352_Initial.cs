using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GROW_CRM.Data.GROWMigrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "GROW");

            migrationBuilder.CreateTable(
                name: "DietaryRestrictions",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Restriction = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietaryRestrictions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Genders",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genders", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "IncomeSituations",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Situation = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeSituations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    Text = table.Column<string>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTypes",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTypes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NotificationTypeID = table.Column<int>(nullable: false),
                    MessageID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Notifications_Messages_MessageID",
                        column: x => x.MessageID,
                        principalSchema: "GROW",
                        principalTable: "Messages",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationTypes_NotificationTypeID",
                        column: x => x.NotificationTypeID,
                        principalSchema: "GROW",
                        principalTable: "NotificationTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Households",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    StreetNumber = table.Column<int>(nullable: false),
                    StreetName = table.Column<string>(nullable: true),
                    AptNumber = table.Column<int>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    PostalCode = table.Column<string>(nullable: true),
                    YearlyIncome = table.Column<decimal>(nullable: false),
                    NumberOfMembers = table.Column<int>(nullable: false),
                    LICOVerified = table.Column<bool>(nullable: false),
                    ProvinceID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Households", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Households_Provinces_ProvinceID",
                        column: x => x.ProvinceID,
                        principalSchema: "GROW",
                        principalTable: "Provinces",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HouseholdNotifications",
                schema: "GROW",
                columns: table => new
                {
                    HouseholdID = table.Column<int>(nullable: false),
                    NotificationID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdNotifications", x => new { x.HouseholdID, x.NotificationID });
                    table.ForeignKey(
                        name: "FK_HouseholdNotifications_Households_HouseholdID",
                        column: x => x.HouseholdID,
                        principalSchema: "GROW",
                        principalTable: "Households",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HouseholdNotifications_Notifications_NotificationID",
                        column: x => x.NotificationID,
                        principalSchema: "GROW",
                        principalTable: "Notifications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    FirstName = table.Column<string>(nullable: false),
                    MiddleName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: false),
                    DOB = table.Column<DateTime>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Notes = table.Column<string>(nullable: true),
                    GenderID = table.Column<int>(nullable: false),
                    HouseholdID = table.Column<int>(nullable: false),
                    IncomeSituationID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Members_Genders_GenderID",
                        column: x => x.GenderID,
                        principalSchema: "GROW",
                        principalTable: "Genders",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Members_Households_HouseholdID",
                        column: x => x.HouseholdID,
                        principalSchema: "GROW",
                        principalTable: "Households",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Members_IncomeSituations_IncomeSituationID",
                        column: x => x.IncomeSituationID,
                        principalSchema: "GROW",
                        principalTable: "IncomeSituations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UploadedFile",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    FileName = table.Column<string>(maxLength: 255, nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    HouseholdID = table.Column<int>(nullable: true),
                    DocumentTypeID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFile", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UploadedFile_DocumentTypes_DocumentTypeID",
                        column: x => x.DocumentTypeID,
                        principalSchema: "GROW",
                        principalTable: "DocumentTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UploadedFile_Households_HouseholdID",
                        column: x => x.HouseholdID,
                        principalSchema: "GROW",
                        principalTable: "Households",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DietaryRestrictionMembers",
                schema: "GROW",
                columns: table => new
                {
                    MemberID = table.Column<int>(nullable: false),
                    DietaryRestrictionID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietaryRestrictionMembers", x => new { x.MemberID, x.DietaryRestrictionID });
                    table.ForeignKey(
                        name: "FK_DietaryRestrictionMembers_DietaryRestrictions_DietaryRestrictionID",
                        column: x => x.DietaryRestrictionID,
                        principalSchema: "GROW",
                        principalTable: "DietaryRestrictions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DietaryRestrictionMembers_Members_MemberID",
                        column: x => x.MemberID,
                        principalSchema: "GROW",
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Subtotal = table.Column<int>(nullable: false),
                    Taxes = table.Column<int>(nullable: false),
                    Total = table.Column<int>(nullable: false),
                    MemberID = table.Column<int>(nullable: false),
                    PaymentTypeID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Orders_Members_MemberID",
                        column: x => x.MemberID,
                        principalSchema: "GROW",
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_PaymentTypes_PaymentTypeID",
                        column: x => x.PaymentTypeID,
                        principalSchema: "GROW",
                        principalTable: "PaymentTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileContent",
                schema: "GROW",
                columns: table => new
                {
                    FileContentID = table.Column<int>(nullable: false),
                    Content = table.Column<byte[]>(nullable: true),
                    MimeType = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileContent", x => x.FileContentID);
                    table.ForeignKey(
                        name: "FK_FileContent_UploadedFile_FileContentID",
                        column: x => x.FileContentID,
                        principalSchema: "GROW",
                        principalTable: "UploadedFile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                schema: "GROW",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Quantity = table.Column<int>(nullable: false),
                    OrderID = table.Column<int>(nullable: false),
                    ItemID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OrderItems_Items_ItemID",
                        column: x => x.ItemID,
                        principalSchema: "GROW",
                        principalTable: "Items",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderID",
                        column: x => x.OrderID,
                        principalSchema: "GROW",
                        principalTable: "Orders",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DietaryRestrictionMembers_DietaryRestrictionID",
                schema: "GROW",
                table: "DietaryRestrictionMembers",
                column: "DietaryRestrictionID");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdNotifications_NotificationID",
                schema: "GROW",
                table: "HouseholdNotifications",
                column: "NotificationID");

            migrationBuilder.CreateIndex(
                name: "IX_Households_ProvinceID",
                schema: "GROW",
                table: "Households",
                column: "ProvinceID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_GenderID",
                schema: "GROW",
                table: "Members",
                column: "GenderID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_HouseholdID",
                schema: "GROW",
                table: "Members",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_Members_IncomeSituationID",
                schema: "GROW",
                table: "Members",
                column: "IncomeSituationID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_MessageID",
                schema: "GROW",
                table: "Notifications",
                column: "MessageID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotificationTypeID",
                schema: "GROW",
                table: "Notifications",
                column: "NotificationTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ItemID",
                schema: "GROW",
                table: "OrderItems",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderID",
                schema: "GROW",
                table: "OrderItems",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_MemberID",
                schema: "GROW",
                table: "Orders",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentTypeID",
                schema: "GROW",
                table: "Orders",
                column: "PaymentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_DocumentTypeID",
                schema: "GROW",
                table: "UploadedFile",
                column: "DocumentTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFile_HouseholdID",
                schema: "GROW",
                table: "UploadedFile",
                column: "HouseholdID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DietaryRestrictionMembers",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "FileContent",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "HouseholdNotifications",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "OrderItems",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "DietaryRestrictions",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "UploadedFile",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "Items",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "DocumentTypes",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "Messages",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "NotificationTypes",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "Members",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "PaymentTypes",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "Genders",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "Households",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "IncomeSituations",
                schema: "GROW");

            migrationBuilder.DropTable(
                name: "Provinces",
                schema: "GROW");
        }
    }
}
