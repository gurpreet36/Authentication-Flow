using AuthenticationAPI.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace AuthenticationAPI.Domain
{
    public class RegisterMapper
    {
        public RegisterMapper(EntityTypeBuilder<Register> entityTypeBuilder)
        {
            entityTypeBuilder.HasKey(t=>t.Email);
            entityTypeBuilder.Property(t=>t.Name).IsRequired();
            entityTypeBuilder.Property(t=>t.Password).IsRequired();
            entityTypeBuilder.Property(t=>t.PhoneNumber).IsRequired();
        
        }
    }
}