using NimbusCommerce.Application.Common.Models;

namespace NimbusCommerce.Application.Authentication.Register;

public interface IRegisterService
{
    Task<IdentityOperationResult> RegisterAsync(RegisterRequest request);
}
