using System;
using System.Collections.Generic;

namespace consoleSAMOIG.Models;

public partial class Contact
{
    public int ContactIdpk { get; set; }

    public string AspnetUserIdFk { get; set; } = null!;

    public string? NameLast { get; set; }

    public string? MidInitial { get; set; }

    public string? NameFirst { get; set; }

    public string? IndividualId { get; set; }

    public string? Ssn { get; set; }

    public string Email { get; set; } = null!;

    public string? AltEmail { get; set; }

    public string? Address1 { get; set; }

    public string? Address2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public int? TypeContactIdfk { get; set; }

    public string? EmergencyContact { get; set; }

    public string? EmergencyPhone { get; set; }

    public string? CellPhone { get; set; }

    public bool? CellPhoneisText { get; set; }

    public string? Smscarrier { get; set; }

    public string? PrimaryPhone { get; set; }

    public DateOnly? Dob { get; set; }

    public byte[]? Photo { get; set; }

    public int? Race { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Created { get; set; }

    public DateOnly? Archived { get; set; }

    public string? Title { get; set; }

    public bool? PassportVerified { get; set; }

    public string? Comments { get; set; }

    public bool? Validated { get; set; }

    public int? RejectionIdfk { get; set; }

    public string? AccessCode { get; set; }

    public string? Discriminator { get; set; }

    public bool? Agreed2Confidentiality { get; set; }

    public string? RegistrationStatus { get; set; }

    public int? BusinessUnitIdfk { get; set; }

    public DateOnly? LastContact { get; set; }

    public int? ProgressionIdfk { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public string? PhotoUrl { get; set; }

    public int? SubmissionStatusId { get; set; }

    public bool ContactBackgroundCheckEnabled { get; set; }

    public string? VerifiedByType { get; set; }

    public bool NeedsReviewCpnw { get; set; }

    public int NeedsReviewEd { get; set; }

    public int NeedsReviewHc { get; set; }
}
