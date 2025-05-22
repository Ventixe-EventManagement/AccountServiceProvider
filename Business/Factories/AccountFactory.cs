using Business.Models;
using Data.Entities;

namespace Business.Factories;

public static class AccountFactory
{
    public static ApplicationUser FromRegisterRequest(RegisterRequest request)
    {
        return new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };
    }
}