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

    public async Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        await dbContext.Companies.FirstOrDefaultAsync(c => c.Name.ToLower() == name.Trim().ToLower(), cancellationToken);

    public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Companies.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default) =>
        await dbContext.Companies.AddAsync(company, cancellationToken);
}
