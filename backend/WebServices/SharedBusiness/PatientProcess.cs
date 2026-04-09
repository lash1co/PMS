using Domain.Entities;
using Domain.Exceptions;
using System;

namespace WebServices.SharedBusiness
{
    public class PatientProcess
    {
        public Invoice CreateInvoice(Patient patient, decimal amount, DateTime dueDate)
        {
            var invoice = new Invoice(patient.Id, amount, dueDate);
                    return invoice;
        }

        public void UpdateDetails(Patient patient, string firstName, string lastName, DateTime dateOfBirth, string phone, string? email)
        {
            if (dateOfBirth > DateTime.Today)
                throw new DomainException("Date of birth cannot be in the future");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new DomainException("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new DomainException("Last name is required");

            patient.FirstName = firstName;
            patient.LastName = lastName;
            patient.DateOfBirth = dateOfBirth;
            patient.Phone = phone;
            patient.Email = email;
        }

        public Patient CreatePatient(string firstName, string lastName, DateTime dateOfBirth, string phone, string? email)
        {
            if (dateOfBirth > DateTime.Today)
                throw new DomainException("Date of birth cannot be in the future");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new DomainException("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new DomainException("Last name is required");

            return new Patient(firstName, lastName, dateOfBirth, phone, email);
        }
    }
}