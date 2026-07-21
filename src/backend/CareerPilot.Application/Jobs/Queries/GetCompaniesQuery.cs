using CareerPilot.Application.Abstractions.Messaging;
using CareerPilot.Application.Abstractions.Persistence;
using CareerPilot.Application.Jobs.Models;

namespace CareerPilot.Application.Jobs.Queries;

public sealed record GetCompaniesQuery : IQuery<IReadOnlyList<CompanyDto>>;

internal sealed class GetCompaniesQueryHandler(ICompanyRepository companyRepository)
    : IQueryHandler<GetCompaniesQuery, IReadOnlyList<CompanyDto>>
{
    public async Task<IReadOnlyList<CompanyDto>> Handle(GetCompaniesQuery query, CancellationToken cancellationToken)
    {
        var companies = await companyRepository.GetAllAsync(cancellationToken);

        return companies.Select(c => new CompanyDto(
            c.Id,
            c.Name,
            c.Slug,
            c.WebsiteUrl,
            c.CareerPageUrl,
            c.LogoUrl,
            c.Industry,
            c.Description)).ToList();
    }
}
