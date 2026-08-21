using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponseDto> IniciarSesionAsync(
        LoginRequestDto request,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task CerrarSesionAsync(
    int idUsuario,
    ContextoClienteDto contextoCliente,
    CancellationToken cancellationToken = default);


}



