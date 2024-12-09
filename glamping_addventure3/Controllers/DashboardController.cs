using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Data;
using glamping_addventure3.Models.ViewModels;
using glamping_addventure3.Models;

namespace glamping_addventure3.Controllers
{
    [Authorize]
    [Authorize(Policy = "AccederDashboard")]
    public class DashboardController : Controller
    {
        private readonly GlampingAddventure3Context _context;

        public DashboardController(GlampingAddventure3Context context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Métodos

        public IActionResult resumenReservas(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            List<DashReservaVM> Lista = new List<DashReservaVM>();

            if (formatoFecha == 3 || formatoFecha == 4)
            {
                CultureInfo culture = new CultureInfo("es-ES");

                for (DateOnly fecha = new DateOnly(fechaInicio.Year, fechaInicio.Month, 1); fecha <= FechaFin; fecha = fecha.AddMonths(1))
                {
                    int cantidadReservas = _context.Reservas

                     .Where(r => r.FechaReserva.HasValue) // Filtra nulos
                     .Count(r => r.FechaReserva.Value.Year == fecha.Year && r.FechaReserva.Value.Month == fecha.Month);


                    string nombreMes = culture.DateTimeFormat.GetMonthName(fecha.Month);
                    Lista.Add(new DashReservaVM
                    {
                        Fecha = $"{nombreMes} {fecha.Year}",
                        Cantidad = cantidadReservas.ToString()
                    });
                }
            }
            else
            {
                for (DateOnly fecha = fechaInicio; fecha <= FechaFin; fecha = fecha.AddDays(1))
                {
                    int cantidadReservas = _context.Reservas
                        .Count(r => DateOnly.FromDateTime((DateTime)r.FechaReserva) == fecha);

                    Lista.Add(new DashReservaVM
                    {
                        Fecha = fecha.ToString("dd-MM-yyyy"),
                        Cantidad = cantidadReservas.ToString()
                    });
                }
            }

            return Json(Lista);
        }

        public IActionResult resumenPaquetes(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            var resultados = _context.DetalleReservaPaquetes
           .Where(d => d.IdreservaNavigation.FechaReserva >= fechaInicio.ToDateTime(TimeOnly.MinValue) &&
               d.IdreservaNavigation.FechaReserva <= FechaFin.ToDateTime(TimeOnly.MinValue))
           .GroupBy(d => d.Idpaquete)
           .Select(g => new
           {
               NombrePaquete = g.FirstOrDefault().IdreservaNavigation != null ?
                        g.FirstOrDefault().IdpaqueteNavigation.NombrePaquete : "Sin nombre",
               CantidadReservas = g.Count()
           })
            .ToList();




            List<DashPaqueteVM> lista = resultados.Select(r => new DashPaqueteVM
            {
                NombrePaquete = r.NombrePaquete,
                Cantidad = r.CantidadReservas.ToString()
            }).ToList();

            return Json(lista);
        }

        public IActionResult resumenServicios(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            var resultados = _context.DetalleReservaServicios
            .Where(d => DateOnly.FromDateTime((DateTime)d.IdreservaNavigation.FechaReserva) >= fechaInicio &&
               DateOnly.FromDateTime((DateTime)d.IdreservaNavigation.FechaReserva) <= FechaFin)
            .GroupBy(d => d.Idservicio)
            .Select(g => new
            {
                NombreServicio = g.FirstOrDefault() != null && g.FirstOrDefault().IdservicioNavigation != null
                         ? g.FirstOrDefault().IdservicioNavigation.NombreServicio
                         : "Sin nombre", // Reemplazamos el operador ?. por una comprobación explícita
                CantidadReservas = g.Sum(d => d.Cantidad)
            })
             .ToList();



            List<DashServiciosVM> lista = resultados.Select(r => new DashServiciosVM
            {
                NombreServicio = r.NombreServicio,
                Cantidad = r.CantidadReservas.ToString()
            }).ToList();

            return Json(lista);
        }

        public IActionResult resumenTipoHabi(int formatoFecha)
        {
            DateTime fechaDateTime = DateTime.Today;
            DateOnly fechaInicio = DateOnly.FromDateTime(fechaDateTime).AddDays(-2);
            DateOnly FechaFin = DateOnly.FromDateTime(fechaDateTime);

            switch (formatoFecha)
            {
                case 1:
                    fechaInicio = ObtenerFechaInicioUltimosDias(fechaDateTime, 7);
                    break;
                case 2:
                    fechaInicio = ObtenerFechaInicioUltimoMes(fechaDateTime);
                    break;
                case 3:
                    fechaInicio = ObtenerFechaInicioUltimosMeses(fechaDateTime, 3);
                    break;
                case 4:
                    fechaInicio = ObtenerFechaInicioUltimoAnio(fechaDateTime);
                    break;
                case 5:
                    fechaInicio = ObtenerPrimerDiaMesActual(fechaDateTime);
                    break;
                case 6:
                    (fechaInicio, FechaFin) = ObtenerRangoSemanaActual(fechaDateTime);
                    break;
                default:
                    break;
            }

            var resultados = _context.DetalleReservaPaquetes
                .Include(d => d.IdpaqueteNavigation)
                    .ThenInclude(p => p.IdhabitacionNavigation)
                        .ThenInclude(h => h.IdtipodeHabitacionNavigation)
                .Where(d => DateOnly.FromDateTime((DateTime)d.IdreservaNavigation.FechaReserva) >= fechaInicio &&
            DateOnly.FromDateTime((DateTime)d.IdreservaNavigation.FechaReserva) <= FechaFin)
                .GroupBy(d => d.IdpaqueteNavigation.IdhabitacionNavigation.IdtipodeHabitacionNavigation.IdtipodeHabitacion)
                .Select(g => new
                {
                    NombreTipoHabitacion = g.FirstOrDefault() != null && g.FirstOrDefault().IdpaqueteNavigation != null
                                           && g.FirstOrDefault().IdpaqueteNavigation.IdhabitacionNavigation != null
                                           && g.FirstOrDefault().IdpaqueteNavigation.IdhabitacionNavigation.IdtipodeHabitacionNavigation != null
                                           ? g.FirstOrDefault().IdpaqueteNavigation.IdhabitacionNavigation.IdtipodeHabitacionNavigation.NombreTipodeHabitacion
                                           : "Sin nombre",
                    CantidadReservas = g.Count()
                })
                .ToList();

            List<DashTipoHabitacionVM> lista = resultados.Select(r => new DashTipoHabitacionVM
            {
                NombreTipoHabitacion = r.NombreTipoHabitacion,
                Cantidad = r.CantidadReservas.ToString()
            }).ToList();

            return Json(lista);
        }

        // Métodos de utilidad para las fechas

        private DateOnly ObtenerFechaInicioUltimosDias(DateTime fechaDateTime, int dias)
        {
            return DateOnly.FromDateTime(fechaDateTime).AddDays(-dias);
        }

        private DateOnly ObtenerFechaInicioUltimoMes(DateTime fechaDateTime)
        {
            return DateOnly.FromDateTime(fechaDateTime).AddMonths(-1);
        }

        private DateOnly ObtenerFechaInicioUltimosMeses(DateTime fechaDateTime, int meses)
        {
            return DateOnly.FromDateTime(fechaDateTime).AddMonths(-meses);
        }

        private DateOnly ObtenerFechaInicioUltimoAnio(DateTime fechaDateTime)
        {
            return DateOnly.FromDateTime(fechaDateTime).AddYears(-1);
        }

        private DateOnly ObtenerPrimerDiaMesActual(DateTime fechaDateTime)
        {
            return new DateOnly(fechaDateTime.Year, fechaDateTime.Month, 1);
        }

        private (DateOnly, DateOnly) ObtenerRangoSemanaActual(DateTime fechaDateTime)
        {
            var primerDiaSemana = DateOnly.FromDateTime(fechaDateTime).AddDays(-(int)fechaDateTime.DayOfWeek);
            var ultimoDiaSemana = primerDiaSemana.AddDays(6);
            return (primerDiaSemana, ultimoDiaSemana);
        }
    }


}

