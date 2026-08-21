using Asistente.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Asistente.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string GenerarHash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "La contraseña no puede estar vacía.",
                nameof(password));
        }

        return _passwordHasher.HashPassword(
            new object(),
            password);
    }

    public bool Verificar(string passwordHash, string password)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) ||
            string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            new object(),
            passwordHash,
            password);

        return resultado != PasswordVerificationResult.Failed;
    }
}