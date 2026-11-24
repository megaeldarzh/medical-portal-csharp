# Медицинский портал — модель классов и изменения

## UML

```mermaid
classDiagram
	class Person {
		<<abstract>>
		-int Id
		-string FullName
		-DateTime BirthDate
		+GetAge() int
		+DisplayInfo()* void
	}
	class Patient {
		-string PhoneNumber
		-string Email
		-string Address
		-List~MedicalRecord~ MedicalRecords
		-List~Appointment~ Appointments
		+UpdateProfile(...) void
		+GetAge() int
		+DisplayInfo() void
	}
	class Doctor {
		-string Specialization
		-int Experience
		-string CabinetNumber
		-decimal Salary
		+UpdateSpecialization(string) void
		+CalculateSalary() decimal
		+CalculatePayment() decimal
		+IsDoctorAvailable(DateTime) bool
		+DisplayInfo() void
	}
	class IPayable {
		<<interface>>
		+CalculatePayment() decimal
	}
	class Department {
		-string DepartmentName
		-int Floor
		-string PhoneNumber
		-Doctor HeadDoctor
		-List~Doctor~ Doctors
		+AddDoctor(Doctor) void
		+RemoveDoctor(int) bool
		+DisplayDoctors() void
	}
	class MedicalService {
		-string ServiceName
		-string Description
		-decimal Price
		-TimeSpan Duration
		+CalculatePrice() decimal
		+UpdatePrice(decimal) void
		+DisplayService() void
	}
	class Appointment {
		-Patient Patient
		-Doctor Doctor
		-DateTime AppointmentDate
		-TimeSpan AppointmentTime
		-AppointmentStatus Status
		+CreateAppointment() void
		+ConfirmAppointment() void
		+CancelAppointment() void
		+DisplayAppointment() void
	}
	class MedicalRecord {
		-Patient Patient
		-string Diagnosis
		-string MedicalHistory
		-DateTime CreationDate
		+AddDiagnosis(string) void
		+UpdateHistory(string) void
		+DisplayRecord() void
	}
	class MedicalTest {
		-string TestName
		-DateTime TestDate
		-string Result
		-Patient Patient
		+AddResult(string) void
		+UpdateResult(string) void
		+DisplayResult() void
	}
	class Payment {
		-Patient Patient
		-decimal Amount
		-DateTime PaymentDate
		-PaymentStatus Status
		+MakePayment() void
		+RefundPayment() void
		+DisplayPayment() void
	}
	class Prescription {
		-Patient Patient
		-Doctor Doctor
		-string MedicationName
		-string Dosage
		-int Duration
		+CreatePrescription() void
		+UpdatePrescription(...) void
		+DisplayPrescription() void
	}
	class AppointmentStatus {
		<<enumeration>>
		Scheduled
		Confirmed
		Completed
		Cancelled
	}
	Person <|-- Patient
	Person <|-- Doctor
	IPayable <|.. Doctor
	Department o-- "0..*" Doctor : aggregates
	Patient "1" --> "0..*" Appointment : books
	Doctor "1" --> "0..*" Appointment : attends
	Patient "1" *-- "0..*" MedicalRecord : owns
	Patient "1" --> "0..*" MedicalTest
	Patient "1" --> "0..*" Payment
	Patient "1" --> "0..*" Prescription
	Doctor "1" --> "0..*" Prescription : prescribes
	Appointment --> AppointmentStatus
```

## Краткий отчёт

- В проекте сохранён ASP.NET Core MVC и добавлен консольный режим запуска `dotnet run -- --console` с меню 1–12 и пунктом 0 для выхода.
- Добавлены сущности пациента, врача, отделения, услуги, приёма, медицинской карты, анализа, платежа и рецепта. `Person` является абстрактным базовым классом; `Doctor` реализует `IPayable`; статусы приёмов ограничены `AppointmentStatus`.
- Сервис портала выполняет поиск пациентов по ID/ФИО/телефону, проверяет конфликты расписания по врачу, дате и времени и агрегирует статистику.
- `Department` хранит ссылки на врачей, не владея их жизненным циклом (агрегация). `Patient` владеет списком медицинских карт; карта создаётся для пациента и регистрируется у него (композиция).
- Данные хранятся в памяти процесса; SQLite не добавлялась, так как для поставленных сценариев постоянное хранение не было обязательным.
