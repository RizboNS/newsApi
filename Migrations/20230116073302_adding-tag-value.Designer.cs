﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using newsApi.Data;

#nullable disable

namespace newsApi.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20230116073302_adding-tag-value")]
    partial class addingtagvalue
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("newsApi.Models.ImageDb", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LocationPath")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("StoryId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("StoryId");

                    b.ToTable("ImageDbs");
                });

            modelBuilder.Entity("newsApi.Models.Story", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Category")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("HtmlData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Publish")
                        .HasColumnType("bit");

                    b.Property<DateTime>("PublishTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TitleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Stories");
                });

            modelBuilder.Entity("newsApi.Models.StoryTag", b =>
                {
                    b.Property<Guid>("StoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("TagName")
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("StoryId", "TagName");

                    b.HasIndex("TagName");

                    b.ToTable("StoryTag");
                });

            modelBuilder.Entity("newsApi.Models.Tag", b =>
                {
                    b.Property<string>("TagName")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnOrder(0);

                    b.Property<string>("TagValue")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("TagName");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("newsApi.Models.ImageDb", b =>
                {
                    b.HasOne("newsApi.Models.Story", null)
                        .WithMany("ImageDbs")
                        .HasForeignKey("StoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("newsApi.Models.StoryTag", b =>
                {
                    b.HasOne("newsApi.Models.Story", "Story")
                        .WithMany("StoryTags")
                        .HasForeignKey("StoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("newsApi.Models.Tag", "Tag")
                        .WithMany("StoryTags")
                        .HasForeignKey("TagName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Story");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("newsApi.Models.Story", b =>
                {
                    b.Navigation("ImageDbs");

                    b.Navigation("StoryTags");
                });

            modelBuilder.Entity("newsApi.Models.Tag", b =>
                {
                    b.Navigation("StoryTags");
                });
#pragma warning restore 612, 618
        }
    }
}