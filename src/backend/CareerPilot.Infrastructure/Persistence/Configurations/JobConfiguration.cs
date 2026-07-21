using CareerPilot.Domain.Jobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations;

internal sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");

        builder.HasKey(j => j.Id);

        builder.HasQueryFilter(j => !j.IsDeleted);

        builder.Property(j => j.ExternalJobId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(j => j.Source)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(j => new { j.ExternalJobId, j.Source })
            .IsUnique();

        builder.Property(j => j.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(j => j.Slug)
            .HasMaxLength(280)
            .IsRequired();

        builder.HasIndex(j => j.Title);
        builder.HasIndex(j => j.CompanyId);
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.PostedAt);

        builder.Property(j => j.Description)
            .IsRequired();

        builder.OwnsOne(j => j.Location, loc =>
        {
            loc.Property(l => l.Country).HasColumnName("location_country").HasMaxLength(100);
            loc.Property(l => l.State).HasColumnName("location_state").HasMaxLength(100);
            loc.Property(l => l.City).HasColumnName("location_city").HasMaxLength(100);
            loc.Property(l => l.RemoteType).HasColumnName("location_remote_type").HasConversion<int>();
        });

        builder.OwnsOne(j => j.Salary, sal =>
        {
            sal.Property(s => s.MinSalary).HasColumnName("salary_min").HasPrecision(18, 2);
            sal.Property(s => s.MaxSalary).HasColumnName("salary_max").HasPrecision(18, 2);
            sal.Property(s => s.Currency).HasColumnName("salary_currency").HasMaxLength(10);
            sal.Property(s => s.PayPeriod).HasColumnName("salary_pay_period").HasMaxLength(20);
        });

        builder.Property(j => j.EmploymentType)
            .HasConversion<int>();

        builder.Property(j => j.ExperienceLevel)
            .HasConversion<int>();

        builder.Property(j => j.Status)
            .HasConversion<int>();

        builder.Property(j => j.ContentHash)
            .HasMaxLength(128)
            .IsRequired();

        // Optimistic concurrency via PostgreSQL's system column rather than a mapped
        // byte[]. IsRowVersion() is a SQL Server idiom: on Npgsql it produces a NOT NULL
        // bytea that the provider never populates, so every insert violates the
        // constraint. xmin is maintained by the database itself and needs no column.
        builder.Property<uint>("xmin").IsRowVersion();
        builder.Ignore(j => j.RowVersion);

        builder.HasOne(j => j.Company)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(j => j.Skills)
            .WithOne()
            .HasForeignKey(s => s.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(j => j.Tags)
            .WithOne()
            .HasForeignKey(t => t.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
