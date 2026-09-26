using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data
{
    /// <summary>
    /// PostgreSQL database context. Table and column names are mapped to snake_case
    /// (see UseSnakeCaseNamingConvention in Startup) and must match Database/student_event_db.sql.
    /// </summary>
    public partial class AsiBasecodeDBContext : DbContext
    {
        public AsiBasecodeDBContext()
        {
        }

        public AsiBasecodeDBContext(DbContextOptions<AsiBasecodeDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<OrganizationMember> OrganizationMembers { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<EventRegistration> EventRegistrations { get; set; }
        public virtual DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasIndex(e => e.StudentNumber).IsUnique();

                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.StudentNumber).HasMaxLength(20);
                entity.Property(e => e.Course).HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20).HasConversion<string>();
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();

                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Acronym).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Adviser).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<OrganizationMember>(entity =>
            {
                entity.HasIndex(e => new { e.OrganizationId, e.MemberId }).IsUnique();

                entity.Property(e => e.Position).HasMaxLength(50);

                entity.HasOne(e => e.Organization)
                    .WithMany(o => o.Members)
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Member)
                    .WithMany(u => u.Memberships)
                    .HasForeignKey(e => e.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasIndex(e => e.StartTime);

                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Venue).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasConversion<string>();
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UpdatedBy).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Organization)
                    .WithMany(o => o.Events)
                    .HasForeignKey(e => e.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<EventRegistration>(entity =>
            {
                entity.HasIndex(e => new { e.EventId, e.StudentId }).IsUnique();
                entity.HasIndex(e => e.RegistrationCode).IsUnique();

                entity.Property(e => e.RegistrationCode).IsRequired().HasMaxLength(32);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasConversion<string>();

                entity.HasOne(e => e.Event)
                    .WithMany(ev => ev.Registrations)
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                    .WithMany(u => u.Registrations)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(e => e.RegistrationId).IsUnique();

                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasConversion<string>();
                entity.Property(e => e.Remarks).HasMaxLength(250);
                entity.Property(e => e.RecordedBy).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Registration)
                    .WithOne(r => r.Attendance)
                    .HasForeignKey<Attendance>(e => e.RegistrationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // The app stores local times (DateTime.Now), so map every DateTime to "timestamp without time zone".
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(t => t.GetProperties())
                         .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
            {
                property.SetColumnType("timestamp without time zone");
            }

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
