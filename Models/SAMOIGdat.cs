using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace consoleSAMOIG.Models;

public partial class SAMOIGdat : DbContext
{
    public SAMOIGdat()
    {

    }

    public SAMOIGdat(DbContextOptions<SAMOIGdat> options)
        : base(options)
    {
    }

    public virtual DbSet<ApiKey> ApiKeys { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<ExclusionHit> ExclusionHits { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(Globals.conString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.ApiKeyIdpk);

            entity.ToTable("API_Keys");

            entity.Property(e => e.ApiKeyIdpk).HasColumnName("API_KeyIDpk");
            entity.Property(e => e.Api)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("API");
            entity.Property(e => e.ApiKey1)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("API_KEY");
            entity.Property(e => e.DateUpdated).HasColumnType("datetime");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContactIdpk);

            entity.HasIndex(e => e.NameLast, "iContactsNameLast");

            entity.HasIndex(e => e.RegistrationStatus, "ind_RegistrationStatus");

            entity.Property(e => e.ContactIdpk).HasColumnName("ContactIDPK");
            entity.Property(e => e.Archived).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Discriminator)
                .HasMaxLength(128)
                .IsUnicode(false);
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.MidInitial)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.NameFirst)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NameLast)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RegistrationStatus)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RejectionIdfk).HasColumnName("RejectionIDFK");
            entity.Property(e => e.Ssn)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SSN");
            entity.Property(e => e.TypeContactIdfk).HasColumnName("TypeContactIDFK");
            entity.Property(e => e.Validated).HasDefaultValue(false);
        });

        modelBuilder.Entity<ExclusionHit>(entity =>
        {
            entity.HasKey(e => e.ExclusionHitIdpk);

            entity.HasIndex(e => e.ContactIdfk, "IX_ContactID");

            entity.HasIndex(e => e.DateRun, "IX_DateRun");

            entity.Property(e => e.ExclusionHitIdpk).HasColumnName("ExclusionHitIDPK");
            entity.Property(e => e.Address)
                .HasMaxLength(75)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ContactIdfk).HasColumnName("ContactIDfk");
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.LikelyMatch)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Message)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.NameHit).IsUnicode(false);
            entity.Property(e => e.NameInput)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TableHit)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("TableHIT");
            entity.Property(e => e.Zip)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

       


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
