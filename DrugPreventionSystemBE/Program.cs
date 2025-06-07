
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;
using DotNetEnv;
using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen; 
using System.Linq; 
using System.Text.Json.Serialization;

namespace DrugPreventionSystemBE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<DrugPreventionDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));
            builder.Services.AddScoped<DrugPreventionSystem.Service.Interface.IAuthenticationService, DrugPreventionSystemBE.DrugPreventionSystem.Service.AuthenticationService>();
            builder.Services.AddControllers() 
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DrugPreventionSystem API", Version = "v1" });

                // Định nghĩa bảo mật
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập 'Bearer ' VÀ sau đó là token của bạn vào ô bên dưới.\r\n\r\nVí dụ: 'Bearer 12345abcdef'",
                });

                // Áp dụng bảo mật cho tất cả endpoint
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                        new string[] {}
                    }
                });
                c.SchemaFilter<EnumSchemaFilter>();
            });

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IdServices>();

            Env.Load();

            // Kiểm tra cấu hình Facebook
            if (string.IsNullOrEmpty(builder.Configuration["Authentication:Facebook:AppId"]) || string.IsNullOrEmpty(builder.Configuration["Authentication:Facebook:AppSecret"]))
            {
                throw new InvalidOperationException("Cấu hình Facebook AppId hoặc AppSecret không được định nghĩa.");
            }

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = FacebookDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
                facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
            });






            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins(
                                            "http://localhost:3000",
                                            "https://drugpreventionnow.io.vn",
                                            "https://drugpreventionsystem-bzfxb7cndxdtdjbr.eastasia-01.azurewebsites.net") // CHỈ URL CHÍNH THỨC CỦA BẠN
                                        .AllowAnyMethod()
                                        .AllowAnyHeader()
                                        .AllowCredentials());
            });
            var app = builder.Build();
            app.UseForwardedHeaders();

            app.UseCors("AllowSpecificOrigin");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            
            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DrugPreventionSystem API v1 (Local/Current)");
                c.SwaggerEndpoint("https://drugpreventionsystem-bzfxb7cndxdtdjbr.eastasia-01.azurewebsites.net/swagger/v1/swagger.json", "DrugPreventionSystem API v1 (Deployed to Azure)");
                c.RoutePrefix = string.Empty;
            });


           
            app.MapControllers();
            app.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Server API is running...");
            });

            

            app.Run();
        }
        public class EnumSchemaFilter : ISchemaFilter
        {
            public void Apply(OpenApiSchema schema, SchemaFilterContext context)
            {
                if (context.Type.IsEnum)
                {
                    schema.Type = "string";
                    schema.Enum = context.Type.GetEnumNames()
                                        .Select(name => new OpenApiString(name) as IOpenApiAny) //A
                                        .ToList();
                    schema.Properties = null;
                    schema.Format = null;
                }
            }
        }
    }
}
