using Domain.Entities;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Globalization;
using System.Numerics;
using WebServices.Controllers.Patients;

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
            patient.FirstName = FormatName(firstName);
            patient.LastName = FormatName(lastName);
            patient.DateOfBirth = dateOfBirth;
            patient.Phone = phone?.Trim() ?? string.Empty;
            patient.Email = email?.Trim().ToLower();

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

        private string FormatName(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            string trimmed = input.Trim();
            string lowerCase = trimmed.ToLower();
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(lowerCase);
        }

        public Patient CreatePatient(string firstName, string lastName, DateTime dateOfBirth, string phone, string? email)
        {
            firstName = FormatName(firstName);
            lastName = FormatName(lastName);
            phone = phone?.Trim() ?? string.Empty;
            email = email?.Trim().ToLower();

            if (dateOfBirth > DateTime.Today)
                throw new DomainException("Date of birth cannot be in the future");

            if (string.IsNullOrWhiteSpace(firstName))
                throw new DomainException("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new DomainException("Last name is required");

            return new Patient
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Phone = phone,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };
        }

        public Insurance AddInsurance(Patient patient, string payerName, string memberId, string? planType, string relationship)
        {
            var insurance = new Insurance
            {
                PayerName = payerName,
                MemberId = memberId,
                PlanType = planType,
                RelationshipToSubscriber = relationship,
                 PatientId = patient.Id 
            };

            if (patient.Insurances == null) patient.Insurances = new List<Insurance>();
            patient.Insurances.Add(insurance);

            return insurance;
        }

        public void UpdateFullDetails(Patient patient, PatientRequestRecord request)
        {
            UpdateDetails(patient, request.FirstName, request.LastName, request.DateOfBirth, request.Phone, request.Email);

            if (request.Insurances == null) return;

            if (patient.Insurances == null) patient.Insurances = new List<Insurance>();

            foreach (var insReq in request.Insurances)
            {
                var existingIns = patient.Insurances.FirstOrDefault(i => i.Id == insReq.Id && insReq.Id > 0);

                if (existingIns != null)
                {
                    existingIns.PayerName = insReq.PayerName;
                    existingIns.MemberId = insReq.MemberId;
                    existingIns.PlanType = insReq.PlanType ?? string.Empty;
                    existingIns.RelationshipToSubscriber = insReq.RelationshipToSubscriber;
                    existingIns.IsPrimary = insReq.IsPrimary;
                }
                else
                {
                    patient.Insurances.Add(new Insurance
                    {
                        PayerName = insReq.PayerName,
                        MemberId = insReq.MemberId,
                        PlanType = insReq.PlanType ?? string.Empty,
                        RelationshipToSubscriber = insReq.RelationshipToSubscriber,
                        IsPrimary = insReq.IsPrimary,
                        PatientId = patient.Id
                    });
                }
            }
        }
    }
}