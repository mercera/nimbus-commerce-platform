using Microsoft.EntityFrameworkCore;
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

    public async Task<StoredRefreshToken?> FindByHashAsync(string tokenHash) =>
        await _dbContext.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.TokenHash == tokenHash)
            .Select(rt => new StoredRefreshToken(rt.UserId, rt.ExpiresAtUtc, rt.RevokedAtUtc))
            .SingleOrDefaultAsync();

    public async Task RotateAsync(string currentTokenHash, string newTokenHash, DateTime newExpiresAtUtc, string? deviceName)
    {
        var current = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == currentTokenHash);

        if (current is null)
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;

        current.RevokedAtUtc = nowUtc;
        current.ReplacedByTokenHash = newTokenHash;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = current.UserId,
            TokenHash = newTokenHash,
            DeviceName = deviceName,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = newExpiresAtUtc
        });

        // Single SaveChangesAsync: EF Core wraps the UPDATE and the INSERT in one implicit
        // transaction, so the old token is never revoked without its replacement existing.
        await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeAllActiveForUserAsync(string userId)
    {
        var nowUtc = DateTime.UtcNow;

        // RefreshToken has no mapped "is active" property; the definition is expanded here so the
        // predicate is translatable to SQL.
        await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > nowUtc)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.RevokedAtUtc, nowUtc));
    }

    public async Task<bool> RevokeActiveByHashAsync(string tokenHash)
    {
        var nowUtc = DateTime.UtcNow;

        // Same hand-expanded "active" predicate as RevokeAllActiveForUserAsync, so "active" means
        // one thing across the store. Single atomic statement: cannot half-apply, and never
        // touches an already-revoked row's RevokedAtUtc (see IRefreshTokenStore for why that
        // timestamp must not be overwritten by logout).
        var rowsAffected = await _dbContext.RefreshTokens
            .Where(rt => rt.TokenHash == tokenHash && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > nowUtc)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.RevokedAtUtc, nowUtc));

        return rowsAffected == 1;
    }
}
