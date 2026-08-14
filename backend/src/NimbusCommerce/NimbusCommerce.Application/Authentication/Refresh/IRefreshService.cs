namespace NimbusCommerce.Application.Authentication.Refresh;

public interface IRefreshService
{
    Task<RefreshResult> RefreshAsync(string refreshToken, string? deviceName);
}
