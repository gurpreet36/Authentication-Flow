using System;
using AuthenticationAPI.Domain;

namespace AuthenticationAPI.BusinessLogicLayer
{
    public interface IAuthenticationRepository
    {
        Register Registeration(Register register, string password);
        bool emailExists(string email);
        bool autoEmailExistsCheck(string email);

        string Login(string Email, string Password);
        string SendMail(string url, string Email);

        void SendToken(string email, string token);
        bool changePassword(string token, string password);
        bool resetTokenDateTime(Register data);
        bool resetUserAttempt(Register Userdata);
        bool DateTimeCheck(Register user);
        string CheckingPassword(string password, byte[] passwordHash, byte[] salt, Register Userdata);
        Register UserNotFound(string Email);
        Logs createLogs(string e, DateTime t);
    }
}