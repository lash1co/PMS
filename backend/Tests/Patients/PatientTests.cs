using Domain.Entities;
using Domain.Exceptions;
using WebServices.SharedBusiness;
using Xunit;
using System;

namespace Tests.Patients
{
    public class PatientTests
    {
        private readonly PatientProcess _patientProcess = new PatientProcess();

        [Fact]
        public void Should_not_allow_future_birth_date()
        {
            Assert.Throws<DomainException>(() =>
                _patientProcess.CreatePatient(
                    "Ana",
                    "López",
                    DateTime.Today.AddDays(1),
                    "9991234567",
                    null
                )
            );
        }

        [Fact]
        public void Should_create_valid_patient()
        {
            var patient = _patientProcess.CreatePatient(
                "Carlos",
                "Gómez",
                new DateTime(1990, 5, 20),
                "9995554433",
                "carlos@email.com"
            );

            Assert.NotNull(patient);
        }

        [Fact]
        public void Should_update_patient_details_with_valid_data()
        {
            var patient = new Patient("Juan", "Pérez", new DateTime(1990, 5, 20), "9995554433", "juan@email.com");

            _patientProcess.UpdateDetails(
                patient,
                "Carlos",
                "Gómez",
                new DateTime(1992, 8, 15),
                "9811234567",
                "carlos@email.com"
            );

            Assert.Equal("Carlos", patient.FirstName);
            Assert.Equal("Gómez", patient.LastName);
            Assert.Equal(new DateTime(1992, 8, 15), patient.DateOfBirth);
        }

        [Fact]
        public void Should_not_allow_update_with_future_birth_date()
        {
            var patient = new Patient("Juan", "Pérez", new DateTime(1990, 5, 20), "9995554433", "juan@email.com");

            Assert.Throws<DomainException>(() =>
                _patientProcess.UpdateDetails(patient, "Juan", "Pérez", DateTime.Today.AddDays(1), "9995554433", "juan@email.com")
            );
        }

        [Theory]
        [InlineData("", "Pérez")]
        [InlineData("   ", "Pérez")]
        [InlineData("Juan", "")]
        [InlineData("Juan", "   ")]
        public void Should_not_allow_empty_names_on_creation(string firstName, string lastName)
        {
           
            Assert.Throws<DomainException>(() =>
                _patientProcess.CreatePatient(
                    firstName,
                    lastName,
                    new DateTime(1990, 5, 20),
                    "9995554433",
                    "correo@test.com"
                )
            );
        }

        [Fact]
        public void Should_allow_patient_creation_with_null_email()
        {
            var patient = _patientProcess.CreatePatient(
                "María",
                "López",
                new DateTime(1995, 10, 10),
                "9810001122",
                null
            );

            Assert.NotNull(patient);
            Assert.Null(patient.Email);
        }
    }
}