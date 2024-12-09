using System;
using System.Collections.Generic;

namespace glamping_addventure3.Models;

public partial class Cliente
{
    private string? _nombre;
    private string? _apellido;
    public string NroDocumento { get; set; } = null!;

    public string? Nombre
    {
        get => _nombre;
        set => _nombre = string.IsNullOrEmpty(value)
            ? value
            : char.ToUpper(value[0]) + value.Substring(1);
    }

    public string? Apellido
    {
        get => _apellido;
        set => _apellido = string.IsNullOrEmpty(value)
            ? value
            : char.ToUpper(value[0]) + value.Substring(1);
    }

    public string? Direccion { get; set; }

    public string? TipoDocumento { get; set; }

    public string? Pais { get; set; }

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public bool? Estado { get; set; }

    public int? Idrol { get; set; }

    public virtual Role? IdrolNavigation { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
