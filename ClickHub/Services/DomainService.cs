using ClickHub.Interfaces;
using ClickHub.Models;
using Microsoft.Extensions.Logging;

namespace ClickHub.Services
{
    public class DomainService : IDomainService
    {
        public readonly IDomainDatabase _domainDatabase;
        private Dictionary<int, DomainConfig> _domains;

        public DomainService(IDomainDatabase domainDatabase)
        {
            _domainDatabase = domainDatabase;
            _domains = new Dictionary<int, DomainConfig>();
        }

        public async Task InitializeAsync()
        {
            _domains = await _domainDatabase.LoadDomainsAsync();
        }

        public bool TryGetDomain(int id, out DomainConfig domainConfig)
        {
            return _domains.TryGetValue(id, out domainConfig);
        }

        public async Task RefreshDomainsAsync()
        {
            _domains = await _domainDatabase.LoadDomainsAsync();
        }

        public async Task<IEnumerable<DomainConfig>> GetDomainsAsync()
        {
            return _domains.Values;
        }

        public async Task<bool> CreateDomainAsync(string landingPageUrl)
        {
            var result = await _domainDatabase.AddDomainAsync(landingPageUrl);
            if (result)
            {
                await RefreshDomainsAsync();
            }
            return result;
        }

        public async Task<bool> UpdateDomainAsync(int id, string landingPageUrl)
        {
            var result = await _domainDatabase.UpdateDomainAsync(id, landingPageUrl);
            if (result)
            {
                await RefreshDomainsAsync();
            }
            return result;
        }

        public async Task<bool> DeleteDomainAsync(int id)
        {
            if (_domains.TryGetValue(id, out var domain))
            {
                var result = await _domainDatabase.RemoveDomainAsync(domain.LandingPageUrl);
                if (result)
                {
                    await RefreshDomainsAsync();
                }
                return result;
            }
            return false;
        }
    }
}