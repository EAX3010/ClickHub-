using DevExpress.XtraSpellChecker.Parser;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace ClickHub.Models
{
    public class DomainConfig
    {
        public string Id { get; set; }
        public string LandingPageUrl { get; set; }
        public string? TrakingUrl { get; set; }
    }
}