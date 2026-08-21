using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public class LoginRequestDto
{
    public string Usuario { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}