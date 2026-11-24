namespace CollegeManagement.Models;

public sealed class PortalStatistics
{
    public int PatientCount { get; init; }
    public int DoctorCount { get; init; }
    public int AppointmentCount { get; init; }
    public int CompletedAppointmentCount { get; init; }
    public decimal PaidServiceTotal { get; init; }
}

public sealed class PortalDashboardViewModel
{
    public required PortalStatistics Statistics { get; init; }
    public required IReadOnlyList<Patient> Patients { get; init; }
    public required IReadOnlyList<Doctor> Doctors { get; init; }
    public required IReadOnlyList<Appointment> Appointments { get; init; }
    public required IReadOnlyList<MedicalService> Services { get; init; }
    public Patient? SearchResult { get; init; }
    public string? SearchMessage { get; init; }
    public string? Notice { get; init; }
}
