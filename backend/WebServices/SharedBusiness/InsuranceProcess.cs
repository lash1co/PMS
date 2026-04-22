using Domain.Entities;
using Domain.Exceptions;
using System;
using System.Linq;

namespace WebServices.SharedBusiness
{
    public class InsuranceProcess
    {
        /// <summary>
        /// Validates the input parameters and creates an Insurance entity if all validations pass.
        /// </summary>
        public Insurance CreateInsurance(string payerName, string memberId, string? planType, string relationshipToSubscriber)
        {
            payerName = payerName?.Trim() ?? string.Empty;
            memberId = memberId?.Trim().ToUpper() ?? string.Empty;
            planType = string.IsNullOrWhiteSpace(planType) ? null : planType.Trim().ToUpper();
            relationshipToSubscriber = relationshipToSubscriber?.Trim() ?? string.Empty;

            // Validate that required fields are not null or empty
            if (string.IsNullOrWhiteSpace(payerName))
                throw new DomainException("Payer Name (Provider) is required.");

            if (string.IsNullOrWhiteSpace(memberId))
                throw new DomainException("Member ID is required.");

            // Validate that Plan Type is not null or empty
            var validRelationships = new[] { "Self", "Spouse", "Child", "Other" };
            if (!validRelationships.Contains(relationshipToSubscriber))
                throw new DomainException("Invalid relationship to subscriber. Must be Self, Spouse, Child, or Other.");

            string cleanRelationship = validRelationships.First(r => r.Equals(relationshipToSubscriber, System.StringComparison.OrdinalIgnoreCase));

            // Return a new Insurance entity with the provided values
            return new Insurance
            {
                PayerName = payerName,
                MemberId = memberId,
                PlanType = planType,
                RelationshipToSubscriber = cleanRelationship
            };
        }
    }
}