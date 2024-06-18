namespace SendingEmail.Requests
{
    public class SendVerificationCodeRequest
    {
        public string EmailAddress { get; set; }
    }

    public class VerifyCodeRequest
    {
        public string EmailAddress { get; set; }
        public string Code { get; set; }
    }
}
