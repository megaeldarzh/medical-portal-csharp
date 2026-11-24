using System.ComponentModel.DataAnnotations;

namespace CollegeManagement.Models;

public interface IPayable
{
    decimal CalculatePayment();
}

public enum AppointmentStatus
{
    Scheduled,
    Confirmed,
    Completed,
    Cancelled
}

public enum PaymentStatus
{
    Pending,
    Paid,
    Refunded
}

public abstract class Person
{
    private string _fullName;
    private DateTime _birthDate;

    protected Person(int id, string fullName, DateTime birthDate)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Идентификатор должен быть положительным.");
        }

        Id = id;
        _fullName = string.Empty;
        _birthDate = default;
        FullName = fullName;
        BirthDate = birthDate;
    }

    public int Id { get; }

    public string FullName
    {
        get => _fullName;
        set => _fullName = Required(value, nameof(FullName));
    }

    public DateTime BirthDate
    {
        get => _birthDate;
        set
        {
            if (value.Date > DateTime.Today)
            {
                throw new ArgumentOutOfRangeException(nameof(BirthDate), "Дата рождения не может быть в будущем.");
            }

            _birthDate = value.Date;
        }
    }

    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - BirthDate.Year;
        if (BirthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    public abstract void DisplayInfo();

    internal static string Required(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Значение не может быть пустым.", propertyName);
        }

        return value.Trim();
    }
}

public sealed class Patient : Person
{
    private string _phoneNumber;
    private string _email;
    private string _address;
    private readonly List<MedicalRecord> _medicalRecords = [];
    private readonly List<Appointment> _appointments = [];

    public Patient(int patientId, string fullName, DateTime birthDate, string phoneNumber, string email, string address)
        : base(patientId, fullName, birthDate)
    {
        _phoneNumber = string.Empty;
        _email = string.Empty;
        _address = string.Empty;
        PhoneNumber = phoneNumber;
        Email = email;
        Address = address;
    }

    public int PatientId => Id;

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => _phoneNumber = Required(value, nameof(PhoneNumber));
    }

    public string Email
    {
        get => _email;
        set
        {
            var email = Required(value, nameof(Email));
            if (!new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException("Укажите корректный адрес электронной почты.", nameof(Email));
            }

            _email = email;
        }
    }

    public string Address
    {
        get => _address;
        set => _address = Required(value, nameof(Address));
    }

    public IReadOnlyCollection<MedicalRecord> MedicalRecords => _medicalRecords.AsReadOnly();
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    public void UpdateProfile(string fullName, DateTime birthDate, string phoneNumber, string email, string address)
    {
        var validatedName = Required(fullName, nameof(FullName));
        var validatedPhone = Required(phoneNumber, nameof(PhoneNumber));
        var validatedEmail = Required(email, nameof(Email));
        var validatedAddress = Required(address, nameof(Address));
        if (birthDate.Date > DateTime.Today)
        {
            throw new ArgumentOutOfRangeException(nameof(BirthDate), "Дата рождения не может быть в будущем.");
        }

        if (!new EmailAddressAttribute().IsValid(validatedEmail))
        {
            throw new ArgumentException("Укажите корректный адрес электронной почты.", nameof(Email));
        }

        FullName = validatedName;
        BirthDate = birthDate;
        PhoneNumber = validatedPhone;
        Email = validatedEmail;
        Address = validatedAddress;
    }

    internal void AddAppointment(Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        _appointments.Add(appointment);
    }

    internal void AddMedicalRecord(MedicalRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _medicalRecords.Add(record);
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"Пациент #{PatientId}: {FullName}, возраст: {GetAge()}, телефон: {PhoneNumber}, email: {Email}");
    }
}

public sealed class Doctor : Person, IPayable
{
    private string _specialization;
    private int _experience;
    private string _cabinetNumber;
    private decimal _salary;
    private readonly List<Appointment> _appointments = [];

    public Doctor(int doctorId, string fullName, DateTime birthDate, string specialization, int experience, string cabinetNumber, decimal salary)
        : base(doctorId, fullName, birthDate)
    {
        _specialization = string.Empty;
        _cabinetNumber = string.Empty;
        Specialization = specialization;
        Experience = experience;
        CabinetNumber = cabinetNumber;
        Salary = salary;
    }

    public int DoctorId => Id;

    public string Specialization
    {
        get => _specialization;
        private set => _specialization = Required(value, nameof(Specialization));
    }

    public int Experience
    {
        get => _experience;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Experience), "Стаж не может быть отрицательным.");
            }

            _experience = value;
        }
    }

    public string CabinetNumber
    {
        get => _cabinetNumber;
        set => _cabinetNumber = Required(value, nameof(CabinetNumber));
    }

    public decimal Salary
    {
        get => _salary;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Salary), "Оклад не может быть отрицательным.");
            }

            _salary = value;
        }
    }

    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    public void UpdateSpecialization(string specialization) => Specialization = specialization;

    public decimal CalculateSalary() => Salary;

    public decimal CalculatePayment() => CalculateSalary();

    public bool IsDoctorAvailable(DateTime date)
    {
        return !_appointments.Any(appointment =>
            appointment.Status != AppointmentStatus.Cancelled &&
            appointment.AppointmentDate.Date == date.Date &&
            appointment.AppointmentTime == date.TimeOfDay);
    }

    internal void AddAppointment(Appointment appointment)
    {
        ArgumentNullException.ThrowIfNull(appointment);
        _appointments.Add(appointment);
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"Врач #{DoctorId}: {FullName}, {Specialization}, стаж: {Experience} лет, кабинет: {CabinetNumber}, оклад: {Salary:C}");
    }
}

public sealed class Department
{
    private string _departmentName;
    private string _phoneNumber;
    private readonly List<Doctor> _doctors = [];

    public Department(int departmentId, string departmentName, int floor, string phoneNumber, Doctor? headDoctor = null)
    {
        if (departmentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(departmentId));
        }

        if (floor < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(floor));
        }

        DepartmentId = departmentId;
        Floor = floor;
        _departmentName = string.Empty;
        _phoneNumber = string.Empty;
        DepartmentName = departmentName;
        PhoneNumber = phoneNumber;
        HeadDoctor = headDoctor;
        if (headDoctor is not null)
        {
            AddDoctor(headDoctor);
        }
    }

    public int DepartmentId { get; }
    public string DepartmentName { get => _departmentName; set => _departmentName = Person.Required(value, nameof(DepartmentName)); }
    public int Floor { get; }
    public string PhoneNumber { get => _phoneNumber; set => _phoneNumber = Person.Required(value, nameof(PhoneNumber)); }
    public Doctor? HeadDoctor { get; private set; }
    public IReadOnlyCollection<Doctor> Doctors => _doctors.AsReadOnly();

    public void AddDoctor(Doctor doctor)
    {
        ArgumentNullException.ThrowIfNull(doctor);
        if (_doctors.All(existing => existing.DoctorId != doctor.DoctorId))
        {
            _doctors.Add(doctor);
        }
    }

    public bool RemoveDoctor(int doctorId)
    {
        var doctor = _doctors.FirstOrDefault(existing => existing.DoctorId == doctorId);
        if (doctor is null)
        {
            return false;
        }

        _doctors.Remove(doctor);
        if (HeadDoctor?.DoctorId == doctorId)
        {
            HeadDoctor = null;
        }

        return true;
    }

    public void DisplayDoctors()
    {
        Console.WriteLine($"Отделение {DepartmentName}, этаж {Floor}");
        foreach (var doctor in _doctors)
        {
            doctor.DisplayInfo();
        }
    }
}

public sealed class MedicalService
{
    private string _serviceName;
    private string _description;
    private decimal _price;
    private TimeSpan _duration;

    public MedicalService(int serviceId, string serviceName, string description, decimal price, TimeSpan duration)
    {
        if (serviceId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(serviceId));
        }

        ServiceId = serviceId;
        _serviceName = string.Empty;
        _description = string.Empty;
        _price = 0;
        _duration = TimeSpan.Zero;
        ServiceName = serviceName;
        Description = description;
        Price = price;
        Duration = duration;
    }

    public int ServiceId { get; }
    public string ServiceName { get => _serviceName; set => _serviceName = Person.Required(value, nameof(ServiceName)); }
    public string Description { get => _description; set => _description = Person.Required(value, nameof(Description)); }
    public decimal Price { get => _price; private set { if (value < 0) throw new ArgumentOutOfRangeException(nameof(Price)); _price = value; } }
    public TimeSpan Duration { get => _duration; set { if (value < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(Duration)); _duration = value; } }

    public decimal CalculatePrice() => Price;
    public void UpdatePrice(decimal price) => Price = price;
    public void DisplayService() => Console.WriteLine($"Услуга #{ServiceId}: {ServiceName} — {Price:C}, {Duration}");
}

public sealed class Appointment
{
    public Appointment(int appointmentId, Patient patient, Doctor doctor, DateTime appointmentDate, TimeSpan appointmentTime)
    {
        if (appointmentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(appointmentId));
        }

        AppointmentId = appointmentId;
        Patient = patient ?? throw new ArgumentNullException(nameof(patient));
        Doctor = doctor ?? throw new ArgumentNullException(nameof(doctor));
        AppointmentDate = appointmentDate.Date;
        if (appointmentTime < TimeSpan.Zero || appointmentTime >= TimeSpan.FromDays(1))
        {
            throw new ArgumentOutOfRangeException(nameof(appointmentTime));
        }

        AppointmentTime = appointmentTime;
        Status = AppointmentStatus.Scheduled;
    }

    public int AppointmentId { get; }
    public Patient Patient { get; }
    public Doctor Doctor { get; }
    public DateTime AppointmentDate { get; }
    public TimeSpan AppointmentTime { get; }
    public AppointmentStatus Status { get; private set; }

    public void CreateAppointment() => Status = AppointmentStatus.Scheduled;
    public void CancelAppointment()
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
        {
            throw new InvalidOperationException("Отменить можно только активный приём.");
        }

        Status = AppointmentStatus.Cancelled;
    }

    public void ConfirmAppointment()
    {
        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException("Подтвердить можно только запланированный приём.");
        }

        Status = AppointmentStatus.Confirmed;
    }

    public void CompleteAppointment()
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
        {
            throw new InvalidOperationException("Завершить можно только активный приём.");
        }

        Status = AppointmentStatus.Completed;
    }

    public void DisplayAppointment()
    {
        Console.WriteLine($"Приём #{AppointmentId}: {AppointmentDate:dd.MM.yyyy} {AppointmentTime:hh\\:mm}, пациент {Patient.FullName}, врач {Doctor.FullName}, статус: {Status}");
    }
}

public sealed class MedicalRecord
{
    private string _diagnosis;
    private string _medicalHistory;

    public MedicalRecord(int recordId, Patient patient, string diagnosis, string medicalHistory, DateTime creationDate)
    {
        if (recordId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recordId));
        }

        RecordId = recordId;
        Patient = patient ?? throw new ArgumentNullException(nameof(patient));
        _diagnosis = string.Empty;
        _medicalHistory = string.Empty;
        Diagnosis = diagnosis;
        MedicalHistory = medicalHistory;
        CreationDate = creationDate;
        patient.AddMedicalRecord(this);
    }

    public int RecordId { get; }
    public Patient Patient { get; }
    public string Diagnosis { get => _diagnosis; private set => _diagnosis = Person.Required(value, nameof(Diagnosis)); }
    public string MedicalHistory { get => _medicalHistory; private set => _medicalHistory = Person.Required(value, nameof(MedicalHistory)); }
    public DateTime CreationDate { get; }

    public void AddDiagnosis(string diagnosis) => Diagnosis = diagnosis;
    public void UpdateHistory(string medicalHistory) => MedicalHistory = medicalHistory;
    public void DisplayRecord() => Console.WriteLine($"Карта #{RecordId}: {Patient.FullName}, диагноз: {Diagnosis}, дата: {CreationDate:d}");
}

public sealed class MedicalTest
{
    private string _testName;
    private string _result;

    public MedicalTest(int testId, string testName, DateTime testDate, string result, Patient patient)
    {
        if (testId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(testId));
        }

        TestId = testId;
        _testName = string.Empty;
        _result = string.Empty;
        TestName = testName;
        TestDate = testDate;
        Result = result;
        Patient = patient ?? throw new ArgumentNullException(nameof(patient));
    }

    public int TestId { get; }
    public string TestName { get => _testName; set => _testName = Person.Required(value, nameof(TestName)); }
    public DateTime TestDate { get; }
    public string Result { get => _result; private set => _result = Person.Required(value, nameof(Result)); }
    public Patient Patient { get; }

    public void AddResult(string result) => Result = result;
    public void UpdateResult(string result) => Result = result;
    public void DisplayResult() => Console.WriteLine($"Анализ #{TestId}: {TestName}, пациент {Patient.FullName}, результат: {Result}");
}

public sealed class Payment
{
    private decimal _amount;

    public Payment(int paymentId, Patient patient, decimal amount, DateTime paymentDate)
    {
        if (paymentId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(paymentId));
        }

        PaymentId = paymentId;
        Patient = patient ?? throw new ArgumentNullException(nameof(patient));
        Amount = amount;
        PaymentDate = paymentDate;
        Status = PaymentStatus.Pending;
    }

    public int PaymentId { get; }
    public Patient Patient { get; }
    public decimal Amount { get => _amount; private set { if (value < 0) throw new ArgumentOutOfRangeException(nameof(Amount)); _amount = value; } }
    public DateTime PaymentDate { get; }
    public PaymentStatus Status { get; private set; }

    public void MakePayment() { if (Status != PaymentStatus.Pending) throw new InvalidOperationException("Оплата уже обработана."); Status = PaymentStatus.Paid; }
    public void RefundPayment() { if (Status != PaymentStatus.Paid) throw new InvalidOperationException("Возврат возможен только для оплаченного платежа."); Status = PaymentStatus.Refunded; }
    public void DisplayPayment() => Console.WriteLine($"Платёж #{PaymentId}: {Patient.FullName}, {Amount:C}, {Status}");
}

public sealed class Prescription
{
    private string _medicationName;
    private string _dosage;
    private int _duration;

    public Prescription(int prescriptionId, Patient patient, Doctor doctor, string medicationName, string dosage, int duration)
    {
        if (prescriptionId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(prescriptionId));
        }

        PrescriptionId = prescriptionId;
        Patient = patient ?? throw new ArgumentNullException(nameof(patient));
        Doctor = doctor ?? throw new ArgumentNullException(nameof(doctor));
        _medicationName = string.Empty;
        _dosage = string.Empty;
        _duration = 0;
        MedicationName = medicationName;
        Dosage = dosage;
        Duration = duration;
    }

    public int PrescriptionId { get; }
    public Patient Patient { get; }
    public Doctor Doctor { get; }
    public string MedicationName { get => _medicationName; private set => _medicationName = Person.Required(value, nameof(MedicationName)); }
    public string Dosage { get => _dosage; private set => _dosage = Person.Required(value, nameof(Dosage)); }
    public int Duration { get => _duration; private set { if (value < 0) throw new ArgumentOutOfRangeException(nameof(Duration)); _duration = value; } }
    public bool IsCreated { get; private set; }

    public void CreatePrescription() => IsCreated = true;
    public void UpdatePrescription(string medicationName, string dosage, int duration)
    {
        MedicationName = medicationName;
        Dosage = dosage;
        Duration = duration;
    }

    public void DisplayPrescription() => Console.WriteLine($"Рецепт #{PrescriptionId}: {MedicationName}, дозировка {Dosage}, {Duration} дней; пациент {Patient.FullName}, врач {Doctor.FullName}");
}
