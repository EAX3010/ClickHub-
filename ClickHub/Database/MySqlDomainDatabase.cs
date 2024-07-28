using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Dapper;
using ClickHub.Interfaces;
using ClickHub.Models;
using ClickHub.Data;

namespace ClickHub.Database
{
    public class MySqlDomainDatabase : IDomainDatabase
    {
        private readonly string _connectionString;
        private readonly ILogger<MySqlDomainDatabase> _logger;

        public MySqlDomainDatabase(string connectionString, ILogger<MySqlDomainDatabase> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<Dictionary<int, DomainConfig>> LoadDomainsAsync()
        {
            const string sql = "SELECT Id, LandingPageUrl FROM domains";
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var domains = await connection.QueryAsync<DbDomainConfig>(sql);
                return domains.ToDictionary(
                    d => d.Id,
                    d => new DomainConfig { Id = d.Id.ToString(), LandingPageUrl = d.LandingPageUrl }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading domains");
                throw;
            }
        }

        public async Task<bool> AddDomainAsync(string landingPageUrl)
        {
            const string sql = "INSERT INTO domains (LandingPageUrl) VALUES (@LandingPageUrl)";
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var result = await connection.ExecuteAsync(sql, new { LandingPageUrl = landingPageUrl });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding domain");
                return false;
            }
        }

        public async Task<bool> RemoveDomainAsync(int id)
        {
            const string sql = "DELETE FROM domains WHERE Id = @Id";
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var result = await connection.ExecuteAsync(sql, new { Id = id });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing domain");
                return false;
            }
        }

        public async Task<bool> UpdateDomainAsync(int id, string landingPageUrl)
        {
            const string sql = "UPDATE domains SET LandingPageUrl = @LandingPageUrl WHERE Id = @Id";
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var result = await connection.ExecuteAsync(sql, new { Id = id, LandingPageUrl = landingPageUrl });
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating domain");
                return false;
            }
        }

        public async Task ProcessClickDataAsync(ClickData clickData)
        {
            const string sql = @"
                INSERT INTO click_data (
                    Id, Ccpturl, AdPosition, LocationPhysical, LocationInterest,
                    AdGroup, Keyword, Network, LandingPageUrl, Campaign,
                    Device, Placement, Timestamp, UserAgent, IpAddress
                ) VALUES (
                    @Id, @Ccpturl, @AdPosition, @LocationPhysical, @LocationInterest,
                    @AdGroup, @Keyword, @Network, @LandingPageUrl, @Campaign,
                    @Device, @Placement, @Timestamp, @UserAgent, @IpAddress
                )";

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.ExecuteAsync(sql, clickData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing click data");
                throw;
            }
        }

        public async Task<List<ClickData>> GetRecentClicksAsync(int limit = 100)
        {
            const string sql = @"
                SELECT * FROM click_data 
                ORDER BY Timestamp DESC
                LIMIT @Limit";

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var clicks = await connection.QueryAsync<ClickData>(sql, new { Limit = limit });
                return clicks.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent clicks");
                throw;
            }
        }

        public async Task<List<ClickData>> GetClicksInRangeAsync(DateTime startDate, DateTime endDate)
        {
            const string sql = @"
                SELECT * FROM click_data 
                WHERE Timestamp BETWEEN @StartDate AND @EndDate
                ORDER BY Timestamp DESC";

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var clicks = await connection.QueryAsync<ClickData>(sql, new { StartDate = startDate, EndDate = endDate });
                return clicks.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clicks in range");
                throw;
            }
        }
    }
}