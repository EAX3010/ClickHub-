using ClickHub.Models;

namespace ClickHub.Interfaces
{
    public interface IDomainService
    {
        Task InitializeAsync();
        bool TryGetDomain(int id, out DomainConfig domainConfig);
        Task RefreshDomainsAsync();
        Task<IEnumerable<DomainConfig>> GetDomainsAsync();
        Task<bool> CreateDomainAsync(string landingPageUrl);
        Task<bool> UpdateDomainAsync(int id, string landingPageUrl);
        Task<bool> DeleteDomainAsync(int id);

    }
}