using System;

namespace AuthenticationAPI.Domain
{
    public class Register
    {
        public string Email { get; set; }
        public long PhoneNumber { get; set; }
        public string Name { get; set; }
        public byte[] Password { get; set; }
        public byte[] Salt { get; set; }
        public DateTime Time { get; set; }
        public int Attempts { get; set; }
         public string resetToken{get;set;}
        public DateTime resetTokenTime{ get; set; }

       
    }
}