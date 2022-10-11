// /////////////////////////////////////////////////////////////////////////////
// PLEASE DO NOT RENAME OR REMOVE ANY OF THE CODE BELOW. 
// YOU CAN ADD YOUR CODE TO THIS FILE TO EXTEND THE FEATURES TO USE THEM IN YOUR WORK.
// /////////////////////////////////////////////////////////////////////////////

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using WebApi.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// add services to DI container
{
  builder.WebHost.UseUrls("http://localhost:3000");
  builder.WebHost.ConfigureLogging((context, logging) =>
  {
    var config = context.Configuration.GetSection("Logging");
    logging.AddConfiguration(config);
    logging.AddConsole();
    logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    logging.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Warning);
    logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
  }); 

    var services = builder.Services;
    services.AddAutoMapper(typeof(ApiProfile));
    services.AddSwaggerGen();
    services.AddControllers();
    services.AddSqlite<DataContext>("DataSource=webApi.db");    
    services.AddDataProtection().UseCryptographicAlgorithms(
      new AuthenticatedEncryptorConfiguration
      {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
      });

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Football", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer' [space] and then your valid token in the input below. \r\n\r\n Example :'Bearer 124fsfs' "
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
    });
    builder.Services.AddAuthentication(auth =>
    {
        auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
            .AddJwtBearer(auth =>
            {
                auth.TokenValidationParameters = new TokenValidationParameters()
                {
                    ClockSkew = TokenValidationParameters.DefaultClockSkew,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = "http://www.security.org",
                    ValidIssuer = "http://www.security.org",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("onyinye007onyinye"))
                };
            });

}
var app = builder.Build();

// migrate any database changes on startup (includes initial db creation)
using (var scope = app.Services.CreateScope())
{
  var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
  dataContext.Database.EnsureCreated();
}
// configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
{
  app.MapControllers();
}

app.UseAuthentication();
app.UseAuthorization();


app.Run();

public partial class Program { }
