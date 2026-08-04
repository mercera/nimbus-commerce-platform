using NimbusCommerce.Application.Authentication.Interfaces;
using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Authentication.Register;

internal sealed class RegisterService : IRegisterService
{
    private readonly IIdentityService _identityService;

    public RegisterService(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<IdentityOperationResult> RegisterAsync(RegisterRequest request)
    {
        // DataAnnotations enforce this via ModelState when invoked through the MVC pipeline;
        // this re-check guards the invariant for any caller that bypasses that pipeline.
        if (request.Password != request.ConfirmPassword)
        {
            return IdentityOperationResult.Failure(["Passwords do not match."]);
        }

        return await _identityService.CreateUserAsync(request.Email, request.Password, request.FirstName, request.LastName);
    }
}
