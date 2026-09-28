using JobFinder.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace JobFinder.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tables

        public DbSet<User> Users { get; set; }

        public DbSet<Skill> Skills { get; set; }

        public DbSet<UserSkill> UserSkills { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<ProjectSkill> ProjectSkills { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<JobSkill> JobSkills { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<ApplicationSkill> ApplicationSkills { get; set; }

        public DbSet<JobMatch> JobMatches { get; set; }

        public DbSet<ApplicationDocument> ApplicationDocuments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // ==============================
            // UserSkill
            // ==============================

            modelBuilder.Entity<UserSkill>()
                .HasKey(us => new
                {
                    us.UserId,
                    us.SkillId
                });

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            //Job match constraint

            modelBuilder.Entity<JobMatch>()
                .HasIndex(jm => new { jm.UserId, jm.JobId })
                .IsUnique();



            // ==============================
            // ProjectSkill
            // ==============================

            modelBuilder.Entity<ProjectSkill>()
                .HasKey(ps => new
                {
                    ps.ProjectId,
                    ps.SkillId
                });

            modelBuilder.Entity<ProjectSkill>()
                .HasOne(ps => ps.Project)
                .WithMany(p => p.ProjectSkills)
                .HasForeignKey(ps => ps.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectSkill>()
                .HasOne(ps => ps.Skill)
                .WithMany(s => s.ProjectSkills)
                .HasForeignKey(ps => ps.SkillId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==============================
            // JobSkill
            // ==============================

            modelBuilder.Entity<JobSkill>()
                .HasKey(js => new
                {
                    js.JobId,
                    js.SkillId
                });

            modelBuilder.Entity<JobSkill>()
                .HasOne(js => js.Job)
                .WithMany(j => j.JobSkills)
                .HasForeignKey(js => js.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<JobSkill>()
                .HasOne(js => js.Skill)
                .WithMany(s => s.JobSkills)
                .HasForeignKey(js => js.SkillId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==============================
            // ApplicationSkill
            // ==============================

            modelBuilder.Entity<ApplicationSkill>()
                .HasKey(ask => new
                {
                    ask.ApplicationId,
                    ask.SkillId
                });

            modelBuilder.Entity<ApplicationSkill>()
                .HasOne(ask => ask.Application)
                .WithMany(a => a.ApplicationSkills)
                .HasForeignKey(ask => ask.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationSkill>()
                .HasOne(ask => ask.Skill)
                .WithMany(s => s.ApplicationSkills)
                .HasForeignKey(ask => ask.SkillId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==============================
            // User -> Project
            // ==============================

            modelBuilder.Entity<Project>()
                .HasOne(p => p.User)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==============================
            // Company -> Job
            // ==============================

            modelBuilder.Entity<Job>()
                .HasOne(j => j.Company)
                .WithMany(c => c.Jobs)
                .HasForeignKey(j => j.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==============================
            // User -> Application
            // ==============================

            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==============================
            // Job -> Application
            // ==============================

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==============================
            // User -> JobMatch
            // ==============================

            modelBuilder.Entity<JobMatch>()
                .HasOne(jm => jm.User)
                .WithMany(u => u.JobMatches)
                .HasForeignKey(jm => jm.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==============================
            // Job -> JobMatch
            // ==============================

            modelBuilder.Entity<JobMatch>()
                .HasOne(jm => jm.Job)
                .WithMany(j => j.JobMatches)
                .HasForeignKey(jm => jm.JobId)
                .OnDelete(DeleteBehavior.Cascade);


            // ==============================
            // Application -> Document
            // ==============================

            modelBuilder.Entity<ApplicationDocument>()
                .HasOne(d => d.Application)
                .WithMany(a => a.Documents)
                .HasForeignKey(d => d.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApplicationDocument>()
                .HasIndex(d => new
                {
                    d.ApplicationId,
                    d.DocumentType,
                    d.Version
                })
                .IsUnique();

            // ==============================
            // Indexes
            // ==============================

            modelBuilder.Entity<Skill>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<Company>()
                .HasIndex(c => c.Name);

            // A user shouldn't apply to the same job twice
            modelBuilder.Entity<Application>()
                .HasIndex(a => new
                {
                    a.UserId,
                    a.JobId
                })
                .IsUnique();
        }
    }
}

