using System.Linq;
using AuthenticationAPI.DataAcessLayer;
using AuthenticationAPI.Domain;
using System;
using System.Net.Mail;
using System.Net;

namespace AuthenticationAPI.BusinessLogicLayer
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        AuthenticationContext authenticationContext;
        public AuthenticationRepository(AuthenticationContext _authenticationContext)
        {
            authenticationContext = _authenticationContext;
        }
        public Register Registeration(Register register, string password)
        {
            byte[] salt;
            byte[] passwordHash;
            createPasswordHash(password, out passwordHash, out salt);
            register.Password = passwordHash;
            register.Salt = salt;
            authenticationContext.Add(register);
            authenticationContext.SaveChanges();
            return register;
        }
        private void createPasswordHash(string password, out byte[] passwordHash, out byte[] salt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                salt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public bool emailExists(string email)
        {
            if (authenticationContext.registers.Any(x => x.Email == email))
                return true;
            else
                return false;
        }
        public bool autoEmailExistsCheck(string email)
        {
            if (authenticationContext.registers.Any(x => x.Email == email))
                return true;
            else
                return false;
        }
        public Logs createLogs(string e, DateTime t)
        {
            Logs log = new Logs();
            log.Email = e;
            log.TimeLog = t;
            return log;
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] salt)
        //here we are encoding the password which user is passing to login
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                    //here we are comparing the encoded form of password
                }
            }
            return true;
        }
        public string Login(string Email, string Password)
        {
            if (UserNotFound(Email) == null)
            {
                return "user not found";
            }
            else
            {
                var userData = UserNotFound(Email);
                var verifiedPassword = CheckingPassword(Password, userData.Password, userData.Salt, userData);
                return verifiedPassword;
            }
        }

        public Register UserNotFound(string Email)
        {
            var user = authenticationContext.registers.FirstOrDefault(x => x.Email == Email);
            Logs log = new Logs();
            if (user == null)
                return null;
            else
            {
                return user;
            }
        }
        public string CheckingPassword(string password, byte[] passwordHash, byte[] salt, Register Userdata)
        {
            if (VerifyPasswordHash(password, Userdata.Password, Userdata.Salt))
            {
                var nAttempt = Userdata.Attempts;
                if (nAttempt < 5)
                {
                    Userdata.Attempts = 0;
                    Userdata.Time = Convert.ToDateTime("0001-01-01 00:00:00");
                    authenticationContext.Update(Userdata);
                    authenticationContext.SaveChanges();
                    return "Login Successfull";
                }
                else
                {
                    if (resetUserAttempt(Userdata))
                    {
                        Userdata.Attempts = 0;
                        Userdata.Time = Convert.ToDateTime("0001-01-01 00:00:00");
                        authenticationContext.Update(Userdata);
                        authenticationContext.SaveChanges();
                        return "Login Successfull";
                    }
                    else
                    {
                        return "Your Account Has Blocked Try after 30 min";
                    }
                }
            }
            else
            {
                int nAttempt = Userdata.Attempts;
                int AllowAttempt = 5;
                if (nAttempt < 3)
                {
                    Userdata.Attempts++;
                    authenticationContext.logs.Add(createLogs(Userdata.Email, DateTime.Now));
                    authenticationContext.SaveChanges();
                    nAttempt = Userdata.Attempts;
                    return ("Invalid Credentials!! " + Convert.ToString(AllowAttempt - nAttempt) + " Attempt Remaining");
                }
                if (nAttempt == 3)
                {
                    Userdata.Attempts++;
                    authenticationContext.logs.Add(createLogs(Userdata.Email, DateTime.Now));
                    authenticationContext.SaveChanges();
                    return ("Invalid Credentials!! This is your Final Attempt  !! ");
                }
                if (nAttempt == 4)
                {
                    Userdata.Attempts++;
                    Userdata.Time = DateTime.Now;
                    authenticationContext.logs.Add(createLogs(Userdata.Email, DateTime.Now));
                    authenticationContext.SaveChanges();
                    return ("Your Account Has Blocked Try after 30 min");
                }
                else
                {
                    if (resetUserAttempt(Userdata))
                    {
                        Userdata.Attempts++;
                        authenticationContext.registers.Update(Userdata);
                        authenticationContext.SaveChanges();
                        nAttempt = Userdata.Attempts;
                        return ("Invalid Credentials!! " + Convert.ToString(AllowAttempt - nAttempt) + " Attempt Remaining");
                    }
                    else
                    {
                        return "Your Account Has Blocked Try after 30 min";
                    }
                }
            }
        }
        public bool DateTimeCheck(Register user)
        {

            if ((user.Time.Day) != (System.DateTime.Now).Day)
            {
                return true;
            }
            else
            {
                if (((user.Time.Hour) * 60 + user.Time.Minute + ((user.Time.Second) / 60) + 30) <= ((System.DateTime.Now).Hour) * 60 + (System.DateTime.Now).Minute + (((System.DateTime.Now).Second) / 60))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool resetUserAttempt(Register Userdata)
        {
            if (DateTimeCheck(Userdata))
            {
                Userdata.Attempts = 0;
                Userdata.Time = Convert.ToDateTime("0001-01-01 00:00:00");
                authenticationContext.Update(Userdata);
                authenticationContext.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        public string SendMail(string url, string Email)
        {
            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("authenticationflow@gmail.com");
            msg.To.Add(Email);
            msg.Subject = "Reset link";
            msg.Body = url;

            var client = new SmtpClient();

            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("authenticationflow@gmail.com", "Taazaa@1234");
            client.Host = "smtp.gmail.com";
            client.Send(msg);
            client.Dispose();

            return "Send Successfully";
        }

        public void SendToken(string email, string token)
        {
            var data = authenticationContext.registers.Find(email);
            data.resetToken = token;
            data.resetTokenTime = System.DateTime.Now;
            authenticationContext.SaveChanges();
        }

        public bool changePassword(string token, string password)
        {
            var data = authenticationContext.registers.FirstOrDefault(t => t.resetToken == token);
            byte[] salt;
            byte[] passwordHash;
            if (data == null)
                return false;
            else
            {
                if (resetTokenDateTime(data))
                {
                    createPasswordHash(password, out passwordHash, out salt);
                    data.Password = passwordHash;
                    data.Salt = salt;
                    data.resetToken = null;
                    data.resetTokenTime = Convert.ToDateTime("0001-01-01 00:00:00");
                    authenticationContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool resetTokenDateTime(Register data)
        {
            if ((data.resetTokenTime.Day) != (System.DateTime.Now).Day)
            {
                data.resetToken = null;
                data.resetTokenTime = Convert.ToDateTime("0001-01-01 00:00:00");
                authenticationContext.SaveChanges();
                return false;
            }
            else
            {
                if (((data.resetTokenTime.Hour) * 60 + data.resetTokenTime.Minute + ((data.resetTokenTime.Second) / 60) + 60) >= ((System.DateTime.Now).Hour) * 60 + (System.DateTime.Now).Minute + (((System.DateTime.Now).Second) / 60))
                {

                    data.resetToken = null;
                    data.resetTokenTime = Convert.ToDateTime("0001-01-01 00:00:00");
                    authenticationContext.SaveChanges();
                    return true;
                }
                else
                {
                    data.resetToken = null;
                    data.resetTokenTime = Convert.ToDateTime("0001-01-01 00:00:00");
                    authenticationContext.SaveChanges();
                    return false;
                }

            }

        }
    }
}