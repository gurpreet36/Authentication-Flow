namespace AuthenticationAPI.Domain
{
    public class RegisterDTO
    {
         public string  Email { get; set; }
        public long PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Password  { get; set; }

    }
}