using System.ComponentModel.DataAnnotations;

namespace ClickHub.Data
{
    public class DbDomainConfig
    {
        [Key]
        public int Id { get; set; }
        public string LandingPageUrl { get; set; }
    }
}