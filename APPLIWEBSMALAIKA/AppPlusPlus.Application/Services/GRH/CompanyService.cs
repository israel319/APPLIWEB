using AppPlusPlus.Application.Repositories.GRH;
using AppPlusPlus.Domain.Entities.GRH.Organization;

namespace AppPlusPlus.Application.Services.GRH;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Company> CreateCompanyAsync(string companyCode, string companyName)
    {
        var company = new Company
        {
            CompanyCode = companyCode,
            CompanyName = companyName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _companyRepository.AddAsync(company);
        return company;
    }

    public async Task<Company> GetCompanyAsync(int id)
    {
        return await _companyRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
    {
        return await _companyRepository.GetAllAsync();
    }

    public async Task UpdateCompanyAsync(Company company)
    {
        company.UpdatedAt = DateTime.UtcNow;
        await _companyRepository.UpdateAsync(company);
    }

    public async Task DeleteCompanyAsync(int id)
    {
        await _companyRepository.DeleteAsync(id);
    }
}
