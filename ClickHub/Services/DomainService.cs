using ClickHub.Interfaces;
using ClickHub.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ClickHub.Services
{
    public class DomainService : IDomainService
    {
        private readonly IDomainDatabase _domainDatabase;
        private readonly ILogger<DomainService> _logger;
        private ConcurrentDictionary<int, DomainConfig> _domains;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        public DomainService(IDomainDatabase domainDatabase, ILogger<DomainService> logger)
        {
            _domainDatabase = domainDatabase;
            _logger = logger;
            _domains = new ConcurrentDictionary<int, DomainConfig>();
        }

        public async Task InitializeAsync()
        {
            var domains = await _domainDatabase.LoadDomainsAsync();
            _domains = new ConcurrentDictionary<int, DomainConfig>(domains);
        }

        public bool TryGetDomain(int id, out DomainConfig domainConfig)
        {
            return _domains.TryGetValue(id, out domainConfig);
        }

        public async Task RefreshDomainsAsync()
        {
            try
            {
                await _refreshLock.WaitAsync();
                var newDomains = await _domainDatabase.LoadDomainsAsync();
                _domains = new ConcurrentDictionary<int, DomainConfig>(newDomains);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing domains");
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        public Task<IEnumerable<DomainConfig>> GetDomainsAsync()
        {
            return Task.FromResult(_domains.Values.AsEnumerable());
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
                if (_domains.TryGetValue(id, out var domain))
                {
                    domain.LandingPageUrl = landingPageUrl;
                    _domains[id] = domain;
                }
                else
                {
                    await RefreshDomainsAsync();
                }
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
                    _domains.TryRemove(id, out _);
                }
                return result;
            }
            return false;
        }
    }
}