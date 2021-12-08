using AuthenticationAPI.Domain;
using AutoMapper;
namespace AuthenticationAPI.Automapper
{
    public class AuthenticationMapper:Profile
    {
        public AuthenticationMapper()
        {
             CreateMap<RegisterDTO,Register>()
            .ForMember(dest => dest.Password,opt => opt.MapFrom(src => System.Text.Encoding.Unicode.GetBytes(src.Password)));
        }
    }
}