using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.Interfaces;

public interface IPasswordService
{
    string GenerarHash(string password);

    bool Verificar(string passwordHash, string password);
}