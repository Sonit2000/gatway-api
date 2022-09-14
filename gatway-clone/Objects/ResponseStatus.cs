namespace gatway_clone.Objects
{
    public enum ResponseStatus : int
    {
        Success = 200,
        BadRequest = 400,
        Unauthorized = 401,
        UserForbidden = 403,
        DuplicationRequest = 409,
        TransactionNotExist = 470,
        InternalServerError = 500,
        ExternalServerError = 505,
        RequestTimeout = 504,
        TerminalDisabledOrNotExist = 001,
        TerminalBusy = 002,
        PINTranslationFailed = 003,
        JPOSTimeout = 004,
        JPOSConnectionError = 005,
        InvalidSignature = 006,
        InvalidDateTime = 007,
        InvalidCardBin = 008,
        InvalidBankCode = 009,
        InsertDBFailure = 010,
        FullBatch = 011,
        DeviceNotActivated = 012,
        DeviceDisabledOrNotExist = 013,
        BankMDRNotFound = 014,
        OverLimit = 015,
        TransactionFailed = 016,
        VoidNotAccept = 017,
        NotAcceptPartialRefund = 018,
        MerchantDoNotSupportInstallment = 019,
        MerchantHasNotSetupInstallmentFee = 020,
        BankDoNotSupportInstallment = 021,
        TenorNotSupport = 022,
        InstallmentAmountDoNotConformity = 023,
        CardBINDoNotSupportInstallment = 024,
        BankDoesNotSupportPaymentBeforeTheStatementDate = 025,
        InvalidUsernameOrPassword = 026,
        InvalidSerialNumberOrPassword = 027,
        InvalidReceipt = 028,
        EmptyPhoneNumber = 029,

        //MM
        IDAlreadyExists = 101,
        IDNotExist = 102,
        DeviceInvalid = 103,
        MIDTIDBelongsToAnotherDevice = 104,
        MerchantNotExist = 105,
        BranchNotExist = 106,
        PointOfSaleInvalid = 107,
        MIDTIDInvalid = 108,
        DeviceNotYetSettlement = 109,
        CurrencyInvalid = 110,
        InvalidPointOfSale = 111,
        MCCNotExist = 112,
        BranchIsNotUnderTheScopeOfMerchant = 113,
        DeviceNotyetMigrated = 114,
        UserNotExist = 999,
    }
    public static class ResponseStatusExtension{
        private static readonly Dictionary<string, string> _descriptions = new(StringComparer.CurrentCultureIgnoreCase);
        //public static string GetDescription(this ResponseStatus response, string language = "en")
        //{
        //    _descriptions.TryGetValue($"{response}_{language}", out string description);
        //    return description;
        //}
        public static string GetResponseCode(this ResponseStatus response) => ((int)response).ToString("D3");
        public static string GetDescription(this ResponseStatus response, string language = "en")
        {
            _descriptions.TryGetValue($"{response}_{language}", out string description);
            return description;
        }
    }

}
