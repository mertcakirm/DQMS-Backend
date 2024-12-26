using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using QDMS.Repositories;
using System.Reflection;
using QDMS.CustomAttributes;
using QDMS.Services;
using QDMS.EmailTemplates;

namespace QDMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            AddJwt(builder);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthorization();

            builder.Services.AddSingleton(sp =>
            {
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger("DefaultLogger");
            });

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddScoped(sp => new MySqlConnection(config["MySql:ConnectionString"]));

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.Namespace != null && type.Namespace.EndsWith("Repositories") && type.Name.EndsWith("Repository"))
                    builder.Services.AddScoped(type);
                else if (type.Namespace != null && type.Namespace.EndsWith("Services") && type.Name.EndsWith("Service"))
                    builder.Services.AddSingleton(type);
            }

            var app = builder.Build();
            
#if DEBUG
            app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseSwagger();
            app.UseSwaggerUI();
#endif

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                scope.ServiceProvider.GetService<MySqlConnection>()?.Open();

                // Servisleri initilize eder
                _ = scope.ServiceProvider.GetService<EmailService>();
            }

            app.MapControllers();
            app.Run();
        }

        static void AddJwt(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
        }
    }
}
