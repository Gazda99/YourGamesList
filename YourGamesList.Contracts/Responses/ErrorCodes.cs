namespace YourGamesList.Contracts.Responses;

public static class ErrorCodes
{
    public static class General
    {
        public const string InternalServerError = "InternalServerError";
    }

    public static class UserAuth
    {
        public const string InvalidUsername = "InvalidUsername";
        public const string UsernameIsTooShort = "UsernameIsTooShort";
        public const string UsernameIsTooLong = "UsernameIsTooLong";
        public const string UsernameAlreadyTaken = "UsernameAlreadyTaken";

        public const string InvalidEmail = "InvalidEmail";
        public const string EmailIsTooLong = "EmailIsTooLong";
        public const string EmailAlreadyTaken = "EmailAlreadyTaken";
        public const string EmailVerificationInvalid = "EmailVerificationInvalid";
        public const string EmailVerificationExpired = "EmailVerificationExpired";

        public const string UserNotActiveEmailActivation = "UserNotActiveEmailActivation";

        public const string InvalidPassword = "InvalidPassword";
        public const string PasswordIsTooShort = "PasswordIsTooShort";
        public const string PasswordIsTooLong = "PasswordIsTooLong";

        public const string InvalidDateOfBirth = "InvalidDateOfBirth";
        public const string UnderMinimumAge = "UnderMinimumAge";
        public const string ExceedsMaximumAge = "ExceedsMaximumAge";
    }

    public static class Users
    {
        public const string UserNotFound = "UserNotFound";
    }

    public static class Lists
    {
        public const string UserNotFound = "UserNotFound";
        public const string ListAlreadyExists = "ListAlreadyExists";
        public const string ListNameTooLong = "ListNameTooLong";
        public const string InvalidListName = "InvalidListName";
        public const string InvalidListDescription = "InvalidListDescription";
        public const string InvalidListPrivacy = "InvalidListPrivacy";
        public const string ListNotFound = "ListNotFound";
        public const string ListCannotBeDeleted = "ListCannotBeDeleted";
        public const string ListCannotBeEdited = "ListCannotBeEdited";
        public const string GameListEntryAlreadyExists = "GameListEntryAlreadyExists";
        public const string GameListEntryNotFound = "GameListEntryNotFound";
        public const string InvalidGameListEntryCompletionStatus = "InvalidGameListEntryCompletionStatus";
        public const string InvalidGameListEntryIgdbId = "InvalidGameListEntryIgdbId";
        public const string InvalidGameListEntryDescription = "InvalidGameListEntryDescription";
        public const string InvalidListSearchCriteria = "InvalidListSearchCriteria";
    }

    public static class Validation
    {
        public const string RequestBodyRequired = "RequestBodyRequired";
    }
}