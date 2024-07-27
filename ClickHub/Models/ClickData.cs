using DevExpress.Pdf.Native.BouncyCastle.Asn1.X509;
using System;

namespace ClickHub.Models
{
    public partial class ClickData
    {
        public ClickData()
        {
        }
        public int Id { get; set; }
        public string Ccpturl { get; set; }
        public string AdPosition { get; set; }
        public string LocationPhysical { get; set; }
        public string LocationInterest { get; set; }
        public string AdGroup { get; set; }
        public string Keyword { get; set; }
        public string Network { get; set; }
        public string LandingPageUrl { get; set; }
        public string Campaign { get; set; }
        public string Device { get; set; }
        public string Placement { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
    }
}