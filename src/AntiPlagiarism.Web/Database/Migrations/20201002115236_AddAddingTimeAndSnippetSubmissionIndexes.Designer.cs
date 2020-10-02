﻿// <auto-generated />
using System;
using AntiPlagiarism.Web.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AntiPlagiarism.Web.Migrations
{
    [DbContext(typeof(AntiPlagiarismDb))]
    [Migration("20201002115236_AddAddingTimeAndSnippetSubmissionIndexes")]
    partial class AddAddingTimeAndSnippetSubmissionIndexes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("antiplagiarism")
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.Client", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(200)")
                        .HasMaxLength(200);

                    b.Property<Guid>("Token")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("Token")
                        .IsUnique();

                    b.HasIndex("Token", "IsEnabled");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.Code", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Codes");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.ManualSuspicionLevels", b =>
                {
                    b.Property<Guid>("TaskId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double?>("FaintSuspicion")
                        .HasColumnType("float");

                    b.Property<double?>("StrongSuspicion")
                        .HasColumnType("float");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("TaskId");

                    b.ToTable("ManualSuspicionLevels");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.MostSimilarSubmission", b =>
                {
                    b.Property<int>("SubmissionId")
                        .HasColumnType("int");

                    b.Property<int>("SimilarSubmissionId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<double>("Weight")
                        .HasColumnType("float");

                    b.HasKey("SubmissionId");

                    b.HasIndex("SimilarSubmissionId");

                    b.HasIndex("Timestamp");

                    b.ToTable("MostSimilarSubmissions");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.OldSubmissionsInfluenceBorder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("OldSubmissionsInfluenceBorder");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.Snippet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Hash")
                        .HasColumnType("int");

                    b.Property<short>("SnippetType")
                        .HasColumnType("smallint");

                    b.Property<int>("TokensCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TokensCount", "SnippetType", "Hash")
                        .IsUnique();

                    b.ToTable("Snippets");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.SnippetOccurence", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("FirstTokenIndex")
                        .HasColumnType("int");

                    b.Property<int>("SnippetId")
                        .HasColumnType("int");

                    b.Property<int>("SubmissionId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SnippetId", "SubmissionId");

                    b.HasIndex("SubmissionId", "FirstTokenIndex");

                    b.HasIndex("SubmissionId", "SnippetId");

                    b.ToTable("SnippetsOccurences");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.SnippetStatistics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AuthorsCount")
                        .HasColumnType("int");

                    b.Property<int>("ClientId")
                        .HasColumnType("int");

                    b.Property<int>("SnippetId")
                        .HasColumnType("int");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("SnippetId", "TaskId", "ClientId")
                        .IsUnique();

                    b.ToTable("SnippetsStatistics");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.Submission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("AddingTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("AdditionalInfo")
                        .HasColumnType("nvarchar(500)")
                        .HasMaxLength(500);

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ClientId")
                        .HasColumnType("int");

                    b.Property<string>("ClientSubmissionId")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50);

                    b.Property<short>("Language")
                        .HasColumnType("smallint");

                    b.Property<int>("ProgramId")
                        .HasColumnType("int");

                    b.Property<Guid>("TaskId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TokensCount")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AddingTime");

                    b.HasIndex("ProgramId");

                    b.HasIndex("ClientId", "ClientSubmissionId");

                    b.HasIndex("ClientId", "TaskId");

                    b.HasIndex("ClientId", "TaskId", "AuthorId");

                    b.HasIndex("ClientId", "TaskId", "AddingTime", "AuthorId");

                    b.HasIndex("ClientId", "TaskId", "Language", "AuthorId");

                    b.HasIndex("ClientId", "TaskId", "AddingTime", "Language", "AuthorId");

                    b.ToTable("Submissions");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.TaskStatisticsParameters", b =>
                {
                    b.Property<Guid>("TaskId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Deviation")
                        .HasColumnType("float");

                    b.Property<double>("Mean")
                        .HasColumnType("float");

                    b.Property<int>("SubmissionsCount")
                        .HasColumnType("int");

                    b.Property<DateTime?>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("TaskId");

                    b.ToTable("TasksStatisticsParameters");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.TaskStatisticsSourceData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Submission1Id")
                        .HasColumnType("int");

                    b.Property<int>("Submission2Id")
                        .HasColumnType("int");

                    b.Property<double>("Weight")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("Submission1Id");

                    b.HasIndex("Submission2Id");

                    b.ToTable("TaskStatisticsSourceData");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.WorkQueueItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ItemId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("QueueId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("TakeAfterTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("QueueId", "TakeAfterTime");

                    b.ToTable("WorkQueueItems");
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.MostSimilarSubmission", b =>
                {
                    b.HasOne("AntiPlagiarism.Web.Database.Models.Submission", "SimilarSubmission")
                        .WithMany()
                        .HasForeignKey("SimilarSubmissionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AntiPlagiarism.Web.Database.Models.Submission", "Submission")
                        .WithMany()
                        .HasForeignKey("SubmissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.SnippetOccurence", b =>
                {
                    b.HasOne("AntiPlagiarism.Web.Database.Models.Snippet", "Snippet")
                        .WithMany()
                        .HasForeignKey("SnippetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AntiPlagiarism.Web.Database.Models.Submission", "Submission")
                        .WithMany()
                        .HasForeignKey("SubmissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.SnippetStatistics", b =>
                {
                    b.HasOne("AntiPlagiarism.Web.Database.Models.Client", "Client")
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AntiPlagiarism.Web.Database.Models.Snippet", "Snippet")
                        .WithMany()
                        .HasForeignKey("SnippetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.Submission", b =>
                {
                    b.HasOne("AntiPlagiarism.Web.Database.Models.Client", "Client")
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AntiPlagiarism.Web.Database.Models.Code", "Program")
                        .WithMany()
                        .HasForeignKey("ProgramId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AntiPlagiarism.Web.Database.Models.TaskStatisticsSourceData", b =>
                {
                    b.HasOne("AntiPlagiarism.Web.Database.Models.Submission", "Submission1")
                        .WithMany()
                        .HasForeignKey("Submission1Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AntiPlagiarism.Web.Database.Models.Submission", "Submission2")
                        .WithMany()
                        .HasForeignKey("Submission2Id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
