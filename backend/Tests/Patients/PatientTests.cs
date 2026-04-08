using Domain.Entities;
using Domain.Exceptions;
using Xunit;


namespace Tests.Patients
{
    public class PatientTests
    {
        [Fact]
        public void Should_not_allow_future_birth_date()
        {
            Assert.Throws<DomainException>(() =>
                new Patient(
                    "Ana",
                    "López",
                    DateTime.Today.AddDays(1),
                    "9991234567"
                )
            );
        }

        /*[Fact]
        public void Should_identify_minor_patient()
        {
            var patient = new Patient(
                "Juan",
                "Pérez",
                DateTime.Today.AddYears(-10),
                "9999876543"
            );

            var isMinor = patient.IsMinor();

            Assert.True(isMinor);
        }*/

        [Fact]
        public void Should_create_valid_patient()
        {
            var patient = new Patient(
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
            var patient = new Patient(
                "Juan",
                "Pérez",
                new DateTime(1990, 5, 20),
                "9995554433",
                "juan@email.com"
            );

            patient.UpdateDetails(
                "Carlos",
                "Gómez",
                new DateTime(1992, 8, 15),
                "9811234567",
                "carlos@email.com"
            );

            Assert.Equal("Carlos", patient.FirstName);
            Assert.Equal("Gómez", patient.LastName);
            Assert.Equal(new DateTime(1992, 8, 15), patient.DateOfBirth);
            Assert.Equal("9811234567", patient.Phone);
            Assert.Equal("carlos@email.com", patient.Email);
        }

        [Fact]
        public void Should_not_allow_update_with_future_birth_date()
        {
            // Arrange
            var patient = new Patient(
                "Juan",
                "Pérez",
                new DateTime(1990, 5, 20),
                "9995554433",
                "juan@email.com"
            );

            Assert.Throws<DomainException>(() =>
                patient.UpdateDetails(
                    "Juan",
                    "Pérez",
                    DateTime.Today.AddDays(1), // Fecha inválida
                    "9995554433",
                    "juan@email.com"
                )
            );
        }
    }
}
