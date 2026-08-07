using Microsoft.AspNetCore.Identity;
using NimbusCommerce.Application.Authentication.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Infrastructure.Identity;

internal sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityOperationResult> CreateUserAsync(string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);

        return result.Succeeded
            ? IdentityOperationResult.Success()
            : IdentityOperationResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<string?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        return result.Succeeded ? user.Id : null;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Array.Empty<string>();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is not null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<ActiveUser?> GetActiveUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || !user.IsActive || string.IsNullOrEmpty(user.Email))
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new ActiveUser(user.Id, user.Email, roles.ToList());
    }
}
