namespace Vitalita.API.IntegrationTests.Infrastructure;

/// <summary>
///     User-facing texts expected from <c>SharedResource.resx</c> (English, the default culture). They are literal on
///     purpose: a changed translation must make a test fail.
/// </summary>
public static class Messages
{
    public const string InvalidModelState = "The request contains invalid values.";
    public const string Unauthorized = "Authentication is required to access this resource.";
    public const string RequiredField = "This field is required.";
    public const string InvalidEmail = "Enter a valid email address.";
    public const string AdminSignUpNotAllowed = "An administrator account cannot be created through the sign-up.";
    public const string EmailAlreadyInUse = "The email is already in use.";
    public const string InvalidCredentials = "The email or password is incorrect.";
    public const string AccountInactive = "The account is inactive.";
    public const string UserAccessForbidden = "You can only access your own user.";
    public const string ForbiddenTitle = "You do not have permission to access this resource.";
    public const string RoleChangeNotAllowed = "You cannot change your own role.";
    public const string CannotDeactivateSelf = "You cannot deactivate your own account.";

    public const string ProfileAccessForbidden = "You do not have access to this resource.";
    public const string NoValidInvitation = "There is no pending invitation for your email.";
    public const string ProfileAlreadyExists = "This user already has a profile.";
    public const string DocumentNumberFormat = "The DNI must have 8 digits.";
    public const string InvitationAlreadyExists = "This email already has a pending or active access to the older adult.";
    public const string OlderAdultCaregiverChangeNotAllowed =
        "The caregiver in charge of an older adult cannot be changed.";

    public const string MonitoringAccessForbidden = "You do not have access to the records of this older adult.";
    public const string MonitoringOlderAdultChangeNotAllowed = "The older adult of a record cannot be changed.";
    public const string VitalSignNotFound = "The vital sign was not found.";
    public const string HeartRateOutOfRange = "The value must be between 20 and 250 bpm.";
    public const string DiastolicRequired = "Blood pressure needs the diastolic value.";
    public const string InstantInFuture = "The date and time cannot be in the future.";
    public const string ScheduleTimesLimit = "Add 1 to 12 schedule times in HH:mm format.";
}
