using AppPlusPlus.Domain.Entities.GRH.Organization;

namespace AppPlusPlus.Application.Services.GRH;

/// <summary>
/// Service pour gérer les sociétés du module GRH.
/// </summary>
public interface ICompanyService
{
    Task<Company> CreateCompanyAsync(string companyCode, string companyName);
    Task<Company> GetCompanyAsync(int id);
    Task<IEnumerable<Company>> GetAllCompaniesAsync();
    Task UpdateCompanyAsync(Company company);
    Task DeleteCompanyAsync(int id);
}
