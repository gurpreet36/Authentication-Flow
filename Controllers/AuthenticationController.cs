using System;
using Microsoft.AspNetCore.Mvc;
using AuthenticationAPI.BusinessLogicLayer;
using AuthenticationAPI.Domain;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace AuthenticationAPI.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class AuthenticationController : ControllerBase
    {
        IAuthenticationRepository authenticationRepository;
        IMapper mapper;
        IConfiguration config;
        public AuthenticationController(IAuthenticationRepository _authenticationRepository, IMapper _mapper, IConfiguration _config)
        {
            authenticationRepository = _authenticationRepository;
            mapper = _mapper;
            config = _config;
        }
        [HttpPost("register")]
        public ActionResult<Register> signUp(RegisterDTO registerDTO)
        {
            if (authenticationRepository.emailExists(registerDTO.Email))
                return BadRequest("Email already exists");

            var signUp = mapper.Map<Register>(registerDTO);
            var accountCreated = authenticationRepository.Registeration(signUp, registerDTO.Password);
            return StatusCode(201, new { email = accountCreated.Email, fullname = accountCreated.Name });
        }

        [HttpGet("CheckMail/{Email}")]
        public IActionResult CheckMail(string Email)
        {
            var data = authenticationRepository.autoEmailExistsCheck(Email);
            return Ok(data);
        }

        [HttpPost("login")]
        public LoginResponse accountLogin(LoginDTO loginDTO)
        {
            var data = authenticationRepository.Login(loginDTO.email, loginDTO.password);
            LoginResponse obj1 = new LoginResponse();
            obj1.Message = data;
            return obj1;
        }

        [HttpGet("sendResetLink")]

        public bool sendResetLink(string Email)
        {
            var userFromRepo = authenticationRepository.emailExists(Email);
            if (userFromRepo == false)
                return false;
            else
            {
                var Url = "http://localhost:4200/resetPassword";
                UrlResponse response = new UrlResponse();
                string temp = Convert.ToString(System.Guid.NewGuid());
                Url = Url + "/" + temp;
                authenticationRepository.SendMail(Url, Email);
                authenticationRepository.SendToken(Email, temp);
                return true;
            }
        }

        [HttpPost("resetPassword")]
        public bool resetPassword(ResetPassword resetPassword)
        {
            bool check = authenticationRepository.changePassword(resetPassword.Token, resetPassword.Password);
            ResetPassword resetPasswordMessage = new ResetPassword();
            if (check)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public class LoginResponse
    {
         public string Message { get; set; }
    }
    public class UrlResponse
    {
        public string URL { get; set; }
    }
    public class ResetPassword
    {
        public string Password { get; set; }
        public string Token { get; set; }
    }

}
