using ClickHub.Interfaces;
using ClickHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace ClickHub.Services
{
    public class ClickTrackingService : IClickTrackingService
    {
        private readonly IDomainService _domainService;
        private readonly Channel<ClickData> _channel;
        private readonly ILogger<ClickTrackingService> _logger;

        public ClickTrackingService(IDomainService domainService, Channel<ClickData> channel, ILogger<ClickTrackingService> logger)
        {
            _domainService = domainService;
            _channel = channel;
            _logger = logger;
        }

        public Task TrackClickAsync(HttpContext context)
        {
            var query = context.Request.Query;

            if (!int.TryParse(query["id"], out int id) || string.IsNullOrEmpty(query["ccpturl"]))
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Invalid request parameters");
            }

            if (!_domainService.TryGetDomain(id, out var domainConfig))
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Invalid domain" + id);
            }

            // Determine the final URL
            string finalUrl = query["url"];
            if (string.IsNullOrEmpty(finalUrl) || finalUrl == "{lpurl}")
            {
                finalUrl = query["ccpturl"];
            }

            // Perform redirect
            if (!string.IsNullOrEmpty(finalUrl) && finalUrl.Contains("google.com/asnc"))
            {
                context.Response.StatusCode = 204;
            }
            else
            {
                context.Response.Redirect(finalUrl);
            }

            // Enqueue click data for background processing
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
                LandingPageUrl = finalUrl,
                Campaign = query["cpn"],
                Device = query["device"],
                Placement = query["pl"],
                Timestamp = DateTime.UtcNow,
                UserAgent = context.Request.Headers["User-Agent"],
                IpAddress = context.Connection.RemoteIpAddress?.ToString()
            };

            // Use TryWrite to avoid blocking if the channel is full
            if (!_channel.Writer.TryWrite(clickData))
            {
                _logger.LogWarning("Failed to enqueue click data for ID: {Id}. Channel might be full.", id);
            }

            return Task.CompletedTask;
        }
    }
}