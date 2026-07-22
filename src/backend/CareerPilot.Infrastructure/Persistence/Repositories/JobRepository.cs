using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Jobs;
using CareerPilot.Domain.Jobs.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.Persistence.Repositories;

internal sealed class JobRepository(ApplicationDbContext dbContext) : IJobRepository
{
    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Jobs
            .Include(j => j.Company)
            .Include(j => j.Skills)
            .Include(j => j.Tags)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

    public async Task<Job?> GetByExternalIdAsync(string externalJobId, JobProviderKind source, CancellationToken cancellationToken = default) =>
        await dbContext.Jobs
            .Include(j => j.Skills)
            .Include(j => j.Tags)
            .FirstOrDefaultAsync(j => j.ExternalJobId == externalJobId && j.Source == source, cancellationToken);

    public async Task<(IReadOnlyList<Job> Items, int TotalCount)> GetPagedAsync(JobFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Jobs
            .Include(j => j.Company)
            .Include(j => j.Skills)
            .Include(j => j.Tags)
            .AsNoTracking()
            // Only live postings. Deactivated jobs — expired, or dropped from their provider's
            // feed — are kept for history and duplicate detection but must not surface in the
            // browse list, or the user clicks Apply on a posting that no longer exists.
            .Where(j => j.Status == JobStatus.Active)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(j =>
                j.Title.ToLower().Contains(term) ||
                j.Description.ToLower().Contains(term) ||
                (j.Company != null && j.Company.Name.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Country))
        {
            query = query.Where(j => j.Location.Country.ToLower() == filter.Country.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(j => j.Location.City != null && j.Location.City.ToLower().Contains(filter.City.Trim().ToLower()));
        }

        if (filter.RemoteType.HasValue)
        {
            query = query.Where(j => j.Location.RemoteType == filter.RemoteType.Value);
        }

        if (filter.ExperienceLevel.HasValue)
        {
            query = query.Where(j => j.ExperienceLevel == filter.ExperienceLevel.Value);
        }

        if (filter.EmploymentType.HasValue)
        {
            query = query.Where(j => j.EmploymentType == filter.EmploymentType.Value);
        }

        if (filter.MinSalary.HasValue)
        {
            query = query.Where(j => j.Salary.MinSalary >= filter.MinSalary.Value || j.Salary.MaxSalary >= filter.MinSalary.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Skill))
        {
            var skillLower = filter.Skill.Trim().ToLower();
            query = query.Where(j => j.Skills.Any(s => s.SkillName.ToLower() == skillLower));
        }

        if (filter.CompanyId.HasValue)
        {
            query = query.Where(j => j.CompanyId == filter.CompanyId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(1, filter.PageNumber);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(j => j.PostedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default) =>
        await dbContext.Jobs.AddAsync(job, cancellationToken);

    public void Update(Job job) =>
        dbContext.Jobs.Update(job);

    public async Task<int> DeactivateExpiredJobsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredJobs = await dbContext.Jobs
            .Where(j => j.Status == JobStatus.Active && j.ExpiresAt.HasValue && j.ExpiresAt.Value <= now)
            .ToListAsync(cancellationToken);

        foreach (var job in expiredJobs)
        {
            job.Deactivate();
        }

        return expiredJobs.Count;
    }

    public async Task<int> DeactivateMissingAsync(
        JobProviderKind source,
        IReadOnlyCollection<string> seenExternalIds,
        CancellationToken cancellationToken = default)
    {
        // Load-then-mutate rather than ExecuteUpdate so the change flows through the same
        // UnitOfWork/SaveChanges as the sync's inserts and updates, keeping the whole
        // synchronization atomic. A provider holds a few hundred rows at most, so materialising
        // the stale ones is cheap. The seen set is checked in memory because it can hold
        // hundreds of ids — larger than is comfortable to inline into a SQL IN clause.
        var activeJobs = await dbContext.Jobs
            .Where(j => j.Source == source && j.Status == JobStatus.Active)
            .ToListAsync(cancellationToken);

        var stale = activeJobs.Where(j => !seenExternalIds.Contains(j.ExternalJobId)).ToList();

        foreach (var job in stale)
        {
            job.Deactivate();
        }

        return stale.Count;
    }
}
