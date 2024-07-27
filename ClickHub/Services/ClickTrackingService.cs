using ClickHub.Interfaces;
using ClickHub.Models;
using Dapper;
using DevExpress.Xpo.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace ClickHub.Services
{
    public class ClickTrackingService : IClickTrackingService
    {
        private readonly IDomainService _domainService;
        private readonly IDomainDatabase _domainDatabase;
        public ClickTrackingService(IDomainService domainService, IDomainDatabase domainDatabase)
        {
            _domainService = domainService;
            _domainDatabase = domainDatabase;
        }

        public async Task TrackClickAsync(HttpContext context)
        {
            var query = context.Request.Query;

            if (!int.TryParse(query["id"], out int id) || string.IsNullOrEmpty(query["ccpturl"]))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid request parameters");
                return;
            }
            if (!_domainService.TryGetDomain(id, out var domainConfig))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid domain" + id);
                return;
            }
            var clickData = new ClickData
            {
                Id = id,
                Ccpturl = query["ccpturl"],
                AdPosition = query["adpos"],
                LocationPhysical = query["locphisical"],
                LocationInterest = query["locinterest"],
                AdGroup = query["adgrp"],
                Keyword = query["kw"],
                Network = query["nw"],
                LandingPageUrl = query["url"],
                Campaign = query["cpn"],
                Device = query["device"],
                Placement = query["pl"],
                Timestamp = DateTime.UtcNow,
                UserAgent = context.Request.Headers["User-Agent"],
                IpAddress = context.Connection.RemoteIpAddress?.ToString()
            };
            if (!string.IsNullOrEmpty(clickData.LandingPageUrl) && clickData.LandingPageUrl.Contains("google.com"))
            {
                // For Google Ads, return 204 immediately and process click data asynchronously
                context.Response.StatusCode = 204;
                ThreadPool.QueueUserWorkItem(async _ =>
                {
                    ProcessClickDataAsync(clickData);
                });
            }
            else
            {
                if (clickData.LandingPageUrl == "{lpurl}")
                {
                    var url = "https://" + clickData.Ccpturl;
                    context.Response.Redirect(url, false);
                }
                else
                {
                    context.Response.Redirect(clickData.LandingPageUrl, false);
                }


                ThreadPool.QueueUserWorkItem(async _ =>
                {
                     ProcessClickDataAsync(clickData);
                });
            }
        }

        private async Task ProcessClickDataAsync(ClickData clickData)
        {
            _domainDatabase.ProcessClickDataAsync(clickData);
        }

    }
}