using DevInSales.Application.Dtos;

namespace DevInSales.Application.Interfaces.Services
{
    public interface IIdentityService
    {
        Task<RegisterUserResponse> RegisterUser(RegisterUserRequest userData);

        Task<LoginResponse> Login(LoginRequest login);

        // Task<RegisterUserResponse> NoPasswordLogin(string userId);

    }
}