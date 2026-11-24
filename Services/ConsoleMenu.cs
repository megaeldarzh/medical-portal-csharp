using CollegeManagement.Models;

namespace CollegeManagement.Services;

public static class ConsoleMenu
{
    public static void Run(PortalService portal)
    {
        ArgumentNullException.ThrowIfNull(portal);
        while (true)
        {
            Console.WriteLine("\nМЕДИЦИНСКИЙ ПОРТАЛ");
            Console.WriteLine("1. Список пациентов");
            Console.WriteLine("2. Добавить пациента");
            Console.WriteLine("3. Поиск пациента (ID, ФИО, телефон)");
            Console.WriteLine("4. Список врачей");
            Console.WriteLine("5. Добавить врача");
            Console.WriteLine("6. Отделения и врачи");
            Console.WriteLine("7. Записать пациента на приём");
            Console.WriteLine("8. Подтвердить приём");
            Console.WriteLine("9. Отменить приём");
            Console.WriteLine("10. Список приёмов");
            Console.WriteLine("11. Статистика портала");
            Console.WriteLine("12. Услуги и оплата услуги");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите пункт: ");

            var choice = Console.ReadLine();
            if (choice == "0")
            {
                return;
            }

            try
            {
                switch (choice)
                {
                    case "1": ListPatients(portal); break;
                    case "2": AddPatient(portal); break;
                    case "3": SearchPatient(portal); break;
                    case "4": ListDoctors(portal); break;
                    case "5": AddDoctor(portal); break;
                    case "6": ListDepartments(portal); break;
                    case "7": BookAppointment(portal); break;
                    case "8": UpdateAppointment(portal, confirm: true); break;
                    case "9": UpdateAppointment(portal, confirm: false); break;
                    case "10": ListAppointments(portal); break;
                    case "11": ShowStatistics(portal); break;
                    case "12": ShowServicesAndPayment(portal); break;
                    default: Console.WriteLine("Выберите пункт от 0 до 12."); break;
                }
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine(exception.Message);
            }
            catch (KeyNotFoundException exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }

    private static void ListPatients(PortalService portal)
    {
        foreach (var patient in portal.Patients) patient.DisplayInfo();
        if (portal.Patients.Count == 0) Console.WriteLine("Пациентов пока нет.");
    }

    private static void AddPatient(PortalService portal)
    {
        Console.Write("ФИО: "); var fullName = Console.ReadLine() ?? string.Empty;
        Console.Write("Дата рождения (дд.мм.гггг): ");
        if (!DateTime.TryParse(Console.ReadLine(), out var birthDate)) { Console.WriteLine("Некорректная дата рождения."); return; }
        Console.Write("Телефон: "); var phone = Console.ReadLine() ?? string.Empty;
        Console.Write("Email: "); var email = Console.ReadLine() ?? string.Empty;
        Console.Write("Адрес: "); var address = Console.ReadLine() ?? string.Empty;
        Console.WriteLine($"Добавлен пациент #{portal.AddPatient(fullName, birthDate, phone, email, address).PatientId}.");
    }

    private static void SearchPatient(PortalService portal)
    {
        Console.Write("Введите ID, ФИО или телефон: ");
        var query = Console.ReadLine() ?? string.Empty;
        var patient = portal.FindPatient(query);
        if (patient is null) Console.WriteLine("Пациент не найден."); else patient.DisplayInfo();
    }

    private static void ListDoctors(PortalService portal)
    {
        foreach (var doctor in portal.Doctors) doctor.DisplayInfo();
        if (portal.Doctors.Count == 0) Console.WriteLine("Врачей пока нет.");
    }

    private static void AddDoctor(PortalService portal)
    {
        Console.Write("ФИО: "); var name = Console.ReadLine() ?? string.Empty;
        Console.Write("Дата рождения (дд.мм.гггг): ");
        if (!DateTime.TryParse(Console.ReadLine(), out var birthDate)) { Console.WriteLine("Некорректная дата рождения."); return; }
        Console.Write("Специализация: "); var specialization = Console.ReadLine() ?? string.Empty;
        Console.Write("Стаж (лет): ");
        if (!int.TryParse(Console.ReadLine(), out var experience)) { Console.WriteLine("Некорректный стаж."); return; }
        Console.Write("Кабинет: "); var cabinet = Console.ReadLine() ?? string.Empty;
        Console.Write("Оклад: ");
        if (!decimal.TryParse(Console.ReadLine(), out var salary)) { Console.WriteLine("Некорректный оклад."); return; }
        Console.WriteLine($"Добавлен врач #{portal.AddDoctor(name, birthDate, specialization, experience, cabinet, salary).DoctorId}.");
    }

    private static void ListDepartments(PortalService portal)
    {
        foreach (var department in portal.Departments) department.DisplayDoctors();
        if (portal.Departments.Count == 0) Console.WriteLine("Отделений пока нет.");
    }

    private static void BookAppointment(PortalService portal)
    {
        Console.Write("ID пациента: "); if (!int.TryParse(Console.ReadLine(), out var patientId)) { Console.WriteLine("Некорректный ID пациента."); return; }
        Console.Write("ID врача: "); if (!int.TryParse(Console.ReadLine(), out var doctorId)) { Console.WriteLine("Некорректный ID врача."); return; }
        Console.Write("Дата и время (дд.мм.гггг чч:мм): ");
        if (!DateTime.TryParse(Console.ReadLine(), out var date)) { Console.WriteLine("Некорректная дата и время."); return; }
        portal.CreateAppointment(patientId, doctorId, date).DisplayAppointment();
    }

    private static void UpdateAppointment(PortalService portal, bool confirm)
    {
        Console.Write("ID приёма: ");
        if (!int.TryParse(Console.ReadLine(), out var appointmentId)) { Console.WriteLine("Некорректный ID приёма."); return; }
        var appointment = portal.FindAppointment(appointmentId);
        if (appointment is null) { Console.WriteLine("Приём не найден."); return; }
        if (confirm) appointment.ConfirmAppointment(); else appointment.CancelAppointment();
        appointment.DisplayAppointment();
    }

    private static void ListAppointments(PortalService portal)
    {
        foreach (var appointment in portal.Appointments) appointment.DisplayAppointment();
        if (portal.Appointments.Count == 0)
        {
            Console.WriteLine("Записей на приём пока нет.");
            return;
        }

        Console.Write("ID приёма для завершения (Enter — пропустить): ");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input)) return;
        if (int.TryParse(input, out var appointmentId)) portal.CompleteAppointment(appointmentId);
        else Console.WriteLine("Некорректный ID приёма.");
    }

    private static void ShowStatistics(PortalService portal)
    {
        var statistics = portal.GetStatistics();
        Console.WriteLine($"Пациентов: {statistics.PatientCount}; врачей: {statistics.DoctorCount}; приёмов: {statistics.AppointmentCount}; завершено: {statistics.CompletedAppointmentCount}; оплачено: {statistics.PaidServiceTotal:C}");
    }

    private static void ShowServicesAndPayment(PortalService portal)
    {
        foreach (var service in portal.Services) service.DisplayService();
        Console.Write("Оплатить услугу? (д/н): ");
        if (!string.Equals(Console.ReadLine(), "д", StringComparison.OrdinalIgnoreCase)) return;
        Console.Write("ID пациента: "); if (!int.TryParse(Console.ReadLine(), out var patientId)) { Console.WriteLine("Некорректный ID пациента."); return; }
        Console.Write("Сумма: "); if (!decimal.TryParse(Console.ReadLine(), out var amount)) { Console.WriteLine("Некорректная сумма."); return; }
        portal.AddPaidPayment(patientId, amount).DisplayPayment();
    }
}
