using CareerPilot.Domain.Jobs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerPilot.Infrastructure.Persistence.Configurations;

internal sealed class JobSyncLogConfiguration : IEntityTypeConfiguration<JobSyncLog>
{
    public void Configure(EntityTypeBuilder<JobSyncLog> builder)
    {
        builder.ToTable("job_sync_logs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Provider)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(l => l.FailureReason)
            .HasMaxLength(2000);
    }
}
