using CollegeManagement.Models;

namespace CollegeManagement.Services;

public sealed class PortalService
{
    private readonly List<Patient> _patients = [];
    private readonly List<Doctor> _doctors = [];
    private readonly List<Department> _departments = [];
    private readonly List<MedicalService> _services = [];
    private readonly List<Appointment> _appointments = [];
    private readonly List<MedicalRecord> _medicalRecords = [];
    private readonly List<MedicalTest> _medicalTests = [];
    private readonly List<Payment> _payments = [];
    private readonly List<Prescription> _prescriptions = [];
    private readonly object _appointmentLock = new();
    private int _nextPatientId = 1;
    private int _nextDoctorId = 1;
    private int _nextAppointmentId = 1;
    private int _nextPaymentId = 1;

    public PortalService()
    {
        var patient = new Patient(_nextPatientId++, "Анна Иванова", new DateTime(1990, 5, 12), "+7 900 000-00-01", "anna@example.com", "Москва");
        var doctor = new Doctor(_nextDoctorId++, "Иван Петров", new DateTime(1980, 3, 5), "Терапевт", 15, "204", 120000m);
        _patients.Add(patient);
        _doctors.Add(doctor);
        _departments.Add(new Department(1, "Терапевтическое отделение", 2, "+7 495 000-00-00", doctor));
        _services.Add(new MedicalService(1, "Консультация терапевта", "Первичный приём врача-терапевта", 2500m, TimeSpan.FromMinutes(30)));
        _services.Add(new MedicalService(2, "Повторная консультация", "Повторный приём врача", 1800m, TimeSpan.FromMinutes(20)));
        _medicalRecords.Add(new MedicalRecord(1, patient, "Плановый осмотр", "Аллергия не указана", DateTime.Today));
    }

    public IReadOnlyList<Patient> Patients => _patients.AsReadOnly();
    public IReadOnlyList<Doctor> Doctors => _doctors.AsReadOnly();
    public IReadOnlyList<Department> Departments => _departments.AsReadOnly();
    public IReadOnlyList<MedicalService> Services => _services.AsReadOnly();
    public IReadOnlyList<Appointment> Appointments => _appointments.AsReadOnly();
    public IReadOnlyList<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();
    public IReadOnlyList<MedicalTest> MedicalTests => _medicalTests.AsReadOnly();
    public IReadOnlyList<Payment> Payments => _payments.AsReadOnly();
    public IReadOnlyList<Prescription> Prescriptions => _prescriptions.AsReadOnly();

    public Patient AddPatient(string fullName, DateTime birthDate, string phoneNumber, string email, string address)
    {
        var patient = new Patient(_nextPatientId++, fullName, birthDate, phoneNumber, email, address);
        _patients.Add(patient);
        return patient;
    }

    public Doctor AddDoctor(string fullName, DateTime birthDate, string specialization, int experience, string cabinetNumber, decimal salary)
    {
        var doctor = new Doctor(_nextDoctorId++, fullName, birthDate, specialization, experience, cabinetNumber, salary);
        _doctors.Add(doctor);
        return doctor;
    }

    public Patient? FindPatient(int patientId) => _patients.FirstOrDefault(patient => patient.PatientId == patientId);

    public Patient? FindPatient(string searchTerm)
    {
        if (int.TryParse(searchTerm, out var patientId))
        {
            var byId = FindPatient(patientId);
            if (byId is not null)
            {
                return byId;
            }
        }

        return _patients.FirstOrDefault(patient =>
            patient.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            patient.PhoneNumber.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    public Appointment CreateAppointment(int patientId, int doctorId, DateTime appointmentDate)
    {
        lock (_appointmentLock)
        {
            var patient = FindPatient(patientId) ?? throw new KeyNotFoundException($"Пациент с ID {patientId} не найден.");
            var doctor = _doctors.FirstOrDefault(item => item.DoctorId == doctorId)
                ?? throw new KeyNotFoundException($"Врач с ID {doctorId} не найден.");

            if (!doctor.IsDoctorAvailable(appointmentDate))
            {
                throw new InvalidOperationException("Врач уже занят на указанное время!");
            }

            var appointment = new Appointment(_nextAppointmentId++, patient, doctor, appointmentDate.Date, appointmentDate.TimeOfDay);
            _appointments.Add(appointment);
            patient.AddAppointment(appointment);
            doctor.AddAppointment(appointment);
            return appointment;
        }
    }

    public Appointment? FindAppointment(int appointmentId) => _appointments.FirstOrDefault(item => item.AppointmentId == appointmentId);

    public void CompleteAppointment(int appointmentId)
    {
        var appointment = FindAppointment(appointmentId)
            ?? throw new KeyNotFoundException($"Приём с ID {appointmentId} не найден.");
        appointment.CompleteAppointment();
    }

    public Payment AddPaidPayment(int patientId, decimal amount)
    {
        var patient = FindPatient(patientId) ?? throw new KeyNotFoundException($"Пациент с ID {patientId} не найден.");
        var payment = new Payment(_nextPaymentId++, patient, amount, DateTime.Now);
        payment.MakePayment();
        _payments.Add(payment);
        return payment;
    }

    public MedicalRecord AddMedicalRecord(int patientId, string diagnosis, string history)
    {
        var patient = FindPatient(patientId) ?? throw new KeyNotFoundException($"Пациент с ID {patientId} не найден.");
        var record = new MedicalRecord(_medicalRecords.Count + 1, patient, diagnosis, history, DateTime.Now);
        _medicalRecords.Add(record);
        return record;
    }

    public MedicalTest AddMedicalTest(int patientId, string name, DateTime date, string result)
    {
        var patient = FindPatient(patientId) ?? throw new KeyNotFoundException($"Пациент с ID {patientId} не найден.");
        var test = new MedicalTest(_medicalTests.Count + 1, name, date, result, patient);
        _medicalTests.Add(test);
        return test;
    }

    public Prescription AddPrescription(int patientId, int doctorId, string medicationName, string dosage, int duration)
    {
        var patient = FindPatient(patientId) ?? throw new KeyNotFoundException($"Пациент с ID {patientId} не найден.");
        var doctor = _doctors.FirstOrDefault(item => item.DoctorId == doctorId)
            ?? throw new KeyNotFoundException($"Врач с ID {doctorId} не найден.");
        var prescription = new Prescription(_prescriptions.Count + 1, patient, doctor, medicationName, dosage, duration);
        prescription.CreatePrescription();
        _prescriptions.Add(prescription);
        return prescription;
    }

    public PortalStatistics GetStatistics()
    {
        return new PortalStatistics
        {
            PatientCount = _patients.Count,
            DoctorCount = _doctors.Count,
            AppointmentCount = _appointments.Count,
            CompletedAppointmentCount = _appointments.Count(item => item.Status == AppointmentStatus.Completed),
            PaidServiceTotal = _payments.Where(item => item.Status == PaymentStatus.Paid).Sum(item => item.Amount)
        };
    }
}
