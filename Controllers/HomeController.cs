using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CollegeManagement.Models;
using CollegeManagement.Services;

namespace CollegeManagement.Controllers;

public class HomeController(PortalService portal) : Controller
{
    public IActionResult Index(string? search, string? notice)
    {
        var patient = string.IsNullOrWhiteSpace(search) ? null : portal.FindPatient(search);
        var model = new PortalDashboardViewModel
        {
            Statistics = portal.GetStatistics(),
            Patients = portal.Patients,
            Doctors = portal.Doctors,
            Appointments = portal.Appointments,
            Services = portal.Services,
            SearchResult = patient,
            SearchMessage = string.IsNullOrWhiteSpace(search) ? null : patient is null ? "Пациент не найден." : null,
            Notice = notice ?? TempData["Notice"] as string
        };

        return View("Dashboard", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BookAppointment(int patientId, int doctorId, DateTime appointmentDate)
    {
        try
        {
            portal.CreateAppointment(patientId, doctorId, appointmentDate);
            TempData["Notice"] = "Запись на приём создана.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["Notice"] = exception.Message;
        }
        catch (KeyNotFoundException exception)
        {
            TempData["Notice"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CompleteAppointment(int appointmentId)
    {
        try
        {
            portal.CompleteAppointment(appointmentId);
            TempData["Notice"] = "Приём завершён.";
        }
        catch (InvalidOperationException exception)
        {
            TempData["Notice"] = exception.Message;
        }
        catch (KeyNotFoundException exception)
        {
            TempData["Notice"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
