﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PBL3.Data;

#nullable disable

namespace PBL3.Migrations
{
    [DbContext(typeof(ShopGuitarContext))]
    [Migration("20220628134116_changeRelationship")]
    partial class changeRelationship
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("PBL3.Models.Account", b =>
                {
                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("PasswordResetToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ResetTokenExpires")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TokenCreated")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TokenExpires")
                        .HasColumnType("datetime2");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<string>("VerificationToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("VerifiedAccountAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("VerifiedResetPasswordAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("VerifyTokenExpires")
                        .HasColumnType("datetime2");

                    b.HasKey("UserName");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("PBL3.Models.Commodity", b =>
                {
                    b.Property<string>("CommodityId")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Brand")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("warrantyTime")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CommodityId");

                    b.ToTable("Commodities");
                });

            modelBuilder.Entity("PBL3.Models.Contact", b =>
                {
                    b.Property<string>("ContactId")
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ContactId");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("PBL3.Models.Employee", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<string>("ManagerId")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.HasKey("Id");

                    b.HasIndex("ManagerId");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("PBL3.Models.Notification", b =>
                {
                    b.Property<string>("NotificationId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DatePost")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ManagerIdPost")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<string>("ManagerIdUpdated")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TitleName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("NotificationId");

                    b.HasIndex("ManagerIdPost");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("PBL3.Models.Receipt", b =>
                {
                    b.Property<string>("ReceiptId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ContactId")
                        .IsRequired()
                        .HasColumnType("nvarchar(9)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<string>("EmployeeId")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<bool>("IsSales")
                        .HasColumnType("bit");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("ReceiptId");

                    b.HasIndex("ContactId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("Receipts");
                });

            modelBuilder.Entity("PBL3.Models.ReceiptCommodity", b =>
                {
                    b.Property<string>("ReceiptId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("CommodityId")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int>("CommodityQuantity")
                        .HasColumnType("int");

                    b.HasKey("ReceiptId", "CommodityId");

                    b.HasIndex("CommodityId");

                    b.ToTable("ReceiptCommodities");
                });

            modelBuilder.Entity("PBL3.Models.Salaries", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<DateTime>("FromDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Salary")
                        .HasColumnType("decimal(15,2)");

                    b.Property<DateTime>("ToDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Salaries");
                });

            modelBuilder.Entity("PBL3.Models.Titles", b =>
                {
                    b.Property<string>("EmployeeId")
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<DateTime>("DateIn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateOut")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("EmployeeId");

                    b.ToTable("Titles");
                });

            modelBuilder.Entity("PBL3.Models.User", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(9)
                        .HasColumnType("nvarchar(9)");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Gender")
                        .HasColumnType("bit");

                    b.Property<string>("ImageName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PBL3.Models.Account", b =>
                {
                    b.HasOne("PBL3.Models.User", "User")
                        .WithOne("Account")
                        .HasForeignKey("PBL3.Models.Account", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PBL3.Models.Employee", b =>
                {
                    b.HasOne("PBL3.Models.User", "User")
                        .WithOne("Employee")
                        .HasForeignKey("PBL3.Models.Employee", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PBL3.Models.Employee", "Manager")
                        .WithMany()
                        .HasForeignKey("ManagerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Manager");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PBL3.Models.Notification", b =>
                {
                    b.HasOne("PBL3.Models.Employee", "Manager")
                        .WithMany()
                        .HasForeignKey("ManagerIdPost")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Manager");
                });

            modelBuilder.Entity("PBL3.Models.Receipt", b =>
                {
                    b.HasOne("PBL3.Models.Contact", "Contact")
                        .WithMany("Receipts")
                        .HasForeignKey("ContactId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PBL3.Models.Employee", "Employee")
                        .WithMany("Receipts")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Contact");

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("PBL3.Models.ReceiptCommodity", b =>
                {
                    b.HasOne("PBL3.Models.Commodity", "Commodity")
                        .WithMany("ReceiptCommodities")
                        .HasForeignKey("CommodityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PBL3.Models.Receipt", "Receipt")
                        .WithMany("ReceiptCommodities")
                        .HasForeignKey("ReceiptId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Commodity");

                    b.Navigation("Receipt");
                });

            modelBuilder.Entity("PBL3.Models.Salaries", b =>
                {
                    b.HasOne("PBL3.Models.Employee", "Employee")
                        .WithOne("Salaries")
                        .HasForeignKey("PBL3.Models.Salaries", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("PBL3.Models.Titles", b =>
                {
                    b.HasOne("PBL3.Models.Employee", "Employee")
                        .WithOne("Titles")
                        .HasForeignKey("PBL3.Models.Titles", "EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("PBL3.Models.Commodity", b =>
                {
                    b.Navigation("ReceiptCommodities");
                });

            modelBuilder.Entity("PBL3.Models.Contact", b =>
                {
                    b.Navigation("Receipts");
                });

            modelBuilder.Entity("PBL3.Models.Employee", b =>
                {
                    b.Navigation("Receipts");

                    b.Navigation("Salaries")
                        .IsRequired();

                    b.Navigation("Titles")
                        .IsRequired();
                });

            modelBuilder.Entity("PBL3.Models.Receipt", b =>
                {
                    b.Navigation("ReceiptCommodities");
                });

            modelBuilder.Entity("PBL3.Models.User", b =>
                {
                    b.Navigation("Account")
                        .IsRequired();

                    b.Navigation("Employee")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
