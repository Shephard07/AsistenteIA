using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public class AsignarRolesUsuarioRequestDto
{
    public List<int> IdsRoles { get; set; } = [];
}