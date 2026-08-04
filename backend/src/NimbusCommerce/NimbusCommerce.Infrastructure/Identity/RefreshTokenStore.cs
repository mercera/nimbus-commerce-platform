using NimbusCommerce.Application.Authentication.Interfaces;
using NimbusCommerce.Infrastructure.Persistence;

namespace NimbusCommerce.Infrastructure.Identity;

internal sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveAsync(string userId, string tokenHash, DateTime expiresAtUtc, string? deviceName)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            DeviceName = deviceName,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expiresAtUtc
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();
    }
}
