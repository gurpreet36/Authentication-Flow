using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using AuthenticationAPI.DataAcessLayer;
using AuthenticationAPI.BusinessLogicLayer;
using AuthenticationAPI.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;





namespace AuthenticationAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors();
            services.AddDbContext<AuthenticationContext>(t => t.UseNpgsql(Configuration.GetConnectionString("PostgreSqlConnectionString")));
            services.AddControllers();
            services.AddTransient<IAuthenticationRepository, AuthenticationRepository>();
            services.AddAutoMapper(typeof(Startup));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)  //it will take the token(key) from json file
            .AddGoogle(options =>
           {
               IConfigurationSection googleAuthNSection =
                Configuration.GetSection("Authentication:Google");

               options.ClientId = googleAuthNSection["ClientId"];
               options.ClientSecret = googleAuthNSection["ClientSecret"];
           })
                .AddJwtBearer(options =>
                  {
                      options.TokenValidationParameters = new TokenValidationParameters //by this class(TokenValidationParameters) we are validating the token
                       {
                          ValidateIssuerSigningKey = true,
                          IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII
                              .GetBytes(Configuration.GetSection("AppSettings:Token").Value)),
                          ValidateIssuer = false,
                          ValidateAudience = false
                      };

                  });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthenticationAPI", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(t => t.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthenticationAPI v1"));
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
