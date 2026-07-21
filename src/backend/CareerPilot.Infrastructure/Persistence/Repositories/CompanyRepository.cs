using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Domain.Jobs.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerPilot.Infrastructure.Persistence.Repositories;

internal sealed class CompanyRepository(ApplicationDbContext dbContext) : ICompanyRepository
{
    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Company?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        await dbContext.Companies.FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

    /// <remarks>
    /// Checks the change tracker before the database. Job synchronization resolves-or-creates
    /// a company per job and saves once at the end of the batch, so a company added for an
    /// earlier job in the same batch is not yet queryable. Going straight to the database
    /// returns null for it, a second <see cref="Company"/> is created, and the batch dies on
    /// the unique slug index — which is what happens when several jobs share an employer,
    /// i.e. the normal case.
    /// </remarks>
    public async Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim();

        var pending = dbContext.Companies.Local
            .FirstOrDefault(c => string.Equals(c.Name, normalized, StringComparison.OrdinalIgnoreCase));

        return pending ?? await dbContext.Companies
            .FirstOrDefaultAsync(c => c.Name.ToLower() == normalized.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Companies.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default) =>
        await dbContext.Companies.AddAsync(company, cancellationToken);
}
