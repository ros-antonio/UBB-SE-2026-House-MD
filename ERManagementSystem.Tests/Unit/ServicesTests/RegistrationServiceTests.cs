using System;
using ERManagementSystem.Core.Models;
using ERManagementSystem.Core.Repositories;
using ERManagementSystem.Core.Services;
using Moq;
using Xunit;

namespace ERManagementSystem.Tests.Unit.ServicesTests
{
    public class RegistrationServiceTests
    {
        [Fact]
        public void RegisterPatientAndVisit_InvalidPatient_ThrowsInvalidOperationException()
        {
            // Arrange
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            var invalidPatient = new Patient
            {
                Patient_ID = "",
                First_Name = "",
                Last_Name = "",
                Date_of_Birth = default,
                Gender = "",
                Phone = "",
                Emergency_Contact = ""
            };

            var service = new RegistrationService(
                patientRepositoryMock.Object,
                visitRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => service.RegisterPatientAndVisit(invalidPatient, "Headache"));

            // Assert
            Assert.Contains("Patient data is invalid:", exception.Message);

            patientRepositoryMock.Verify(repository => repository.Add(It.IsAny<Patient>()), Times.Never);
            patientRepositoryMock.Verify(repository => repository.GetById(It.IsAny<string>()), Times.Never);
            visitRepositoryMock.Verify(repository => repository.Add(It.IsAny<ER_Visit>()), Times.Never);
        }

        [Fact]
        public void RegisterPatientAndVisit_NewPatient_AddsPatientAndVisit()
        {
            // Arrange
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            var patient = CreateValidPatient();
            ER_Visit? addedVisit = null;

            patientRepositoryMock
                .Setup(repository => repository.GetById(patient.Patient_ID))
                .Returns((Patient?)null);

            visitRepositoryMock
                .Setup(repository => repository.Add(It.IsAny<ER_Visit>()))
                .Callback<ER_Visit>(visit => addedVisit = visit);

            var service = new RegistrationService(
                patientRepositoryMock.Object,
                visitRepositoryMock.Object);

            // Act
            var result = service.RegisterPatientAndVisit(patient, "Chest pain");

            // Assert
            patientRepositoryMock.Verify(repository => repository.Add(patient), Times.Once);
            visitRepositoryMock.Verify(repository => repository.Add(It.IsAny<ER_Visit>()), Times.Once);

            Assert.NotNull(addedVisit);
            Assert.Equal(patient.Patient_ID, addedVisit!.Patient_ID);
            Assert.Equal("Chest pain", addedVisit.Chief_Complaint);
            Assert.Equal(ER_Visit.VisitStatus.REGISTERED, addedVisit.Status);

            Assert.Equal(patient.Patient_ID, result.Patient_ID);
            Assert.Equal("Chest pain", result.Chief_Complaint);
            Assert.Equal(ER_Visit.VisitStatus.REGISTERED, result.Status);
        }

        [Fact]
        public void RegisterPatientAndVisit_ExistingPatient_AddsOnlyVisit()
        {
            // Arrange
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            var patient = CreateValidPatient();

            patientRepositoryMock
                .Setup(repository => repository.GetById(patient.Patient_ID))
                .Returns(patient);

            var service = new RegistrationService(
                patientRepositoryMock.Object,
                visitRepositoryMock.Object);

            // Act
            service.RegisterPatientAndVisit(patient, "Fever");

            // Assert
            patientRepositoryMock.Verify(repository => repository.Add(It.IsAny<Patient>()), Times.Never);
            visitRepositoryMock.Verify(repository => repository.Add(It.IsAny<ER_Visit>()), Times.Once);
        }

        [Fact]
        public void RegisterPatientAndVisit_ValidInput_ReturnsVisitWithExpectedData()
        {
            // Arrange
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            var patient = CreateValidPatient();

            patientRepositoryMock
                .Setup(repository => repository.GetById(patient.Patient_ID))
                .Returns((Patient?)null);

            var service = new RegistrationService(
                patientRepositoryMock.Object,
                visitRepositoryMock.Object);

            // Act
            var result = service.RegisterPatientAndVisit(patient, "Broken arm");

            // Assert
            Assert.Equal(patient.Patient_ID, result.Patient_ID);
            Assert.Equal("Broken arm", result.Chief_Complaint);
            Assert.Equal(ER_Visit.VisitStatus.REGISTERED, result.Status);
            Assert.NotEqual(default, result.Arrival_date_time);
        }

        [Fact]
        public void RegisterPatientAndVisit_InvalidVisit_ThrowsInvalidOperationException()
        {
            // Arrange
            var patientRepositoryMock = new Mock<IPatientRepository>();
            var visitRepositoryMock = new Mock<IERVisitRepository>();

            var patient = CreateValidPatient();

            patientRepositoryMock
                .Setup(repository => repository.GetById(patient.Patient_ID))
                .Returns((Patient?)null);

            var service = new RegistrationService(
                patientRepositoryMock.Object,
                visitRepositoryMock.Object);

            // Act
            var exception = Assert.Throws<InvalidOperationException>(
                () => service.RegisterPatientAndVisit(patient, ""));

            // Assert
            Assert.Contains("ER Visit data is invalid:", exception.Message);

            patientRepositoryMock.Verify(repository => repository.Add(patient), Times.Once);
            visitRepositoryMock.Verify(repository => repository.Add(It.IsAny<ER_Visit>()), Times.Never);
        }

        private static Patient CreateValidPatient()
        {
            return new Patient
            {
                Patient_ID = "1234567890123",
                First_Name = "John",
                Last_Name = "Doe",
                Date_of_Birth = new DateTime(2000, 1, 1),
                Gender = "Male",
                Phone = "0712345678",
                Emergency_Contact = "Jane Doe"
            };
        }
    }
}