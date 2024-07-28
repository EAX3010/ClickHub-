using ClickHub.Interfaces;
using ClickHub.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
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

            // Validate required parameters
            if (!int.TryParse(query["id"], out int id) || string.IsNullOrEmpty(query["ccpturl"]))
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Invalid request parameters");
            }

            // Validate domain
            if (!_domainService.TryGetDomain(id, out var domainConfig))
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync($"Invalid domain: {id}");
            }

            // Determine the final URL
            string finalUrl = !string.IsNullOrEmpty(query["url"]) && query["url"] != "{lpurl}"
                ? query["url"]
                : query["ccpturl"];

            // Validate and secure the URL
            finalUrl = ValidateAndSecureUrl(finalUrl);

            // Handle Google parallel tracking
            if (!string.IsNullOrEmpty(finalUrl) && finalUrl.Contains("google.com/asnc"))
            {
                context.Response.StatusCode = 204; // No Content
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

            // Use TryWrite for non-blocking enqueue
            if (!_channel.Writer.TryWrite(clickData))
            {
                _logger.LogWarning("Failed to enqueue click data for ID: {Id}. Channel might be full.", id);
            }

            return Task.CompletedTask;
        }

        public static string ValidateAndSecureUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or empty.", nameof(input));
            }

            // Check if the input is already a valid URL
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri? uri))
            {
                // If it's already HTTPS, return as is
                if (uri.Scheme == Uri.UriSchemeHttps)
                {
                    return input;
                }

                // If it's HTTP, update to HTTPS
                if (uri.Scheme == Uri.UriSchemeHttp)
                {
                    return $"https://{uri.Host}{uri.PathAndQuery}{uri.Fragment}";
                }
            }

            // If it's not a valid URL, assume it's a domain and create an HTTPS URL
            string cleanInput = Regex.Replace(input, @"^(https?:)?//", "", RegexOptions.IgnoreCase)
                                     .Split(new[] { '/' }, 2)[0]
                                     .Trim()
                                     .Replace(" ", "");

            return $"https://{cleanInput}";
        }
    }
}