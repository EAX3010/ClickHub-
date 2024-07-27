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

        public MySqlDomainDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Dictionary<int, DomainConfig>> LoadDomainsAsync()
        {
            try
            {
                using var connection = new MySqlConnection("Server=185.177.126.211;Database=clicktracker;Uid=clickhub;Pwd=65536653dd;");
                await connection.OpenAsync();
                var domains = await connection.QueryAsync<DbDomainConfig>("SELECT Id, LandingPageUrl FROM domains");
                var domainDictionary = new Dictionary<int, DomainConfig>();
                foreach (var domain in domains)
                {
                    domainDictionary[domain.Id] = new DomainConfig
                    {
                        Id = domain.Id.ToString(),
                        LandingPageUrl = domain.LandingPageUrl
                    };
                }
                return domainDictionary;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> AddDomainAsync(string landingPageUrl)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.ExecuteAsync(
                    "INSERT INTO domains (LandingPageUrl) VALUES (@LandingPageUrl)",
                    new { LandingPageUrl = landingPageUrl });
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RemoveDomainAsync(string landingPageUrl)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.ExecuteAsync(
                    "DELETE FROM domains WHERE LandingPageUrl = @LandingPageUrl",
                    new { LandingPageUrl = landingPageUrl });
                return result > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateDomainAsync(int id, string landingPageUrl)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.ExecuteAsync(
                    "UPDATE domains SET LandingPageUrl = @LandingPageUrl WHERE Id = @Id",
                    new { Id = id, LandingPageUrl = landingPageUrl });
                return result > 0;
            }
            catch (Exception ex)
            {
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
                await connection.OpenAsync();

                await connection.ExecuteAsync(sql, clickData);

            }
            catch (MySqlException ex)
            {
            }
        }
        public async Task<List<ClickData>> GetRecentClicksAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                SELECT * FROM click_data 
                ORDER BY Timestamp DESC";

                var clicks = await connection.QueryAsync<ClickData>(sql);
                return clicks.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<ClickData>> GetClicksInRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                var sql = @"
            SELECT * FROM click_data 
            WHERE Timestamp BETWEEN @StartDate AND @EndDate
            ORDER BY Timestamp DESC";
                var parameters = new { StartDate = startDate, EndDate = endDate };
                var clicks = await connection.QueryAsync<ClickData>(sql, parameters);
                return clicks.ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}