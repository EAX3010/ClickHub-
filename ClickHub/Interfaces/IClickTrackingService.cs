using Microsoft.AspNetCore.Http;

namespace ClickHub.Interfaces
{
    public interface IClickTrackingService
    {
        Task TrackClickAsync(HttpContext context);
    }
}