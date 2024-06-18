using System;
using System.ComponentModel.DataAnnotations;

namespace Back_pixlpark.Models
{
    public class VerificationCode
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public string EmailAddress { get; set; }
        public DateTime Expiration { get; set; }
    }

}
