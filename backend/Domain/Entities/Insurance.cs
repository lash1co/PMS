using System;

namespace Domain.Entities
{
    /// <summary>
    /// Represents the health insurance coverage information for a patient.
    /// This entity manages policy details, pharmacy routing, and eligibility verification.
    /// </summary>
    public class Insurance
    {
        /// <summary>
        /// Gets or sets the unique identifier for the Insurance record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the associated Patient.
        /// </summary>
        public int PatientId { get; set; }

        /// <summary>
        /// Navigation property for the Patient associated with this insurance.
        /// </summary>
        public Patient Patient { get; set; } = null!;

        /// <summary>
        /// Indicates whether this is the patient's primary insurance coverage.
        /// Crucial for Coordination of Benefits (COB) during the billing process.
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets the classification of the insurance (e.g., Medical, Pharmacy, Dental).
        /// </summary>
        public InsuranceCategory Category { get; set; } = InsuranceCategory.Medical;

        // --- Insurance Payer Information ---

        /// <summary>
        /// Gets or sets the name of the insurance company (e.g., BlueCross, Aetna).
        /// </summary>
        public string PayerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Electronic Payer ID. 
        /// Used for routing electronic claims to the correct insurance carrier via a clearinghouse.
        /// </summary>
        public string? PayerId { get; set; }

        // --- Policy Details ---

        /// <summary>
        /// Gets or sets the Member ID (Policy Number) found on the insurance card.
        /// This is the primary identifier used by the payer to locate the member in their system.
        /// </summary>
        public string MemberId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Group Number. 
        /// Identifies the specific employer-sponsored plan or benefit package.
        /// </summary>
        public string? GroupNumber { get; set; }

        /// <summary>
        /// Gets or sets the type of insurance plan (e.g., HMO, PPO, EPO).
        /// This determines how the clinic is reimbursed and if referrals are required.
        /// </summary>
        public string PlanType { get; set; } = string.Empty;

        // --- Operational Financial Information ---

        /// <summary>
        /// Gets or sets the fixed amount the patient is required to pay at the time of an office visit.
        /// Used by front-desk staff during the check-in process.
        /// </summary>
        public decimal? OfficeVisitCopay { get; set; }

        // --- Pharmacy (Rx) Information ---

        /// <summary>
        /// Gets or sets the Bank Identification Number (BIN) for pharmacy claims routing.
        /// Used by the Pharmacy Benefit Manager (PBM) to process medication claims.
        /// </summary>
        public string? RxBIN { get; set; }

        /// <summary>
        /// Gets or sets the Processor Control Number (PCN) for pharmacy claims routing.
        /// A secondary identifier used by PBMs to define the specific pharmacy plan.
        /// </summary>
        public string? RxPCN { get; set; }

        /// <summary>
        /// Gets or sets the Rx Group number, which may differ from the medical group number.
        /// </summary>
        public string? RxGroup { get; set; }

        // --- Subscriber (Policyholder) Information ---

        /// <summary>
        /// Gets or sets the patient's relationship to the policyholder (e.g., Self, Spouse, Child).
        /// </summary>
        public string RelationshipToSubscriber { get; set; } = "Self";

        /// <summary>
        /// Gets or sets the full name of the subscriber (the person who owns the policy).
        /// Required for billing if the patient is a dependent.
        /// </summary>
        public string SubscriberName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Subscriber's Date of Birth. 
        /// Essential for verifying the policyholder's identity with the insurance carrier.
        /// </summary>
        public DateTime? SubscriberDOB { get; set; }

        // --- Eligibility and Validity ---

        /// <summary>
        /// Gets or sets the status of the insurance eligibility check (e.g., Pending, Verified, Denied).
        /// </summary>
        public string VerificationStatus { get; set; } = "Pending";

        /// <summary>
        /// Gets or sets the date and time when the insurance eligibility was last verified with the payer.
        /// </summary>
        public DateTime? LastVerifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the insurance coverage begins.
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the date when the insurance coverage ends. Null if the policy is currently active indefinitely.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        // --- Audit and Control ---

        /// <summary>
        /// Gets or sets a value indicating whether this insurance record is active in the system.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the timestamp when the record was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="Insurance"/> class.
        /// </summary>
        public Insurance() { }
    }
}