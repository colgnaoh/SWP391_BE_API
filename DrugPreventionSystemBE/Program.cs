
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
using Microsoft.OpenApi.Models;


namespace DrugPreventionSystemBE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<DrugPreventionDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<DrugPreventionSystem.Service.Interface.IAuthenticationService, DrugPreventionSystemBE.DrugPreventionSystem.Service.AuthenticationService>(); builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DrugPreventionSystem API", Version = "v1" });

                // Chỉ định định nghĩa bảo mật (để có nút Authorize tổng thể)
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Nhập 'Bearer ' VÀ sau đó là token của bạn vào ô bên dưới.\r\n\r\nVí dụ: 'Bearer 12345abcdef'",
                });

               // sử dụng thuộc tính[Authorize] trên các Controller / Action Methods mà bạn muốn bảo vệ.
            });

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IdServices>();
           
            Env.Load(); // Load environment variables from .env file



            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost:3000", "https://yourfrontenddomain.com") // THAY THẾ "https://yourfrontenddomain.com" bằng domain thực tế của frontend bạn
                                      .AllowAnyMethod() // Cho phép tất cả các phương thức HTTP (GET, POST, PUT, DELETE, v.v.)
                                      .AllowAnyHeader()   // Cho phép tất cả các header của request
                                      .AllowCredentials()); // Quan trọng nếu frontend gửi kèm Cookies hoặc Authorization headers (ví dụ: Bearer token)
            });

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


            var app = builder.Build();

            


            app.UseCors("AllowSpecificOrigin");

            app.UseHttpsRedirection();
            app.UseAuthentication();
            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c => // <--- Cấu hình này đã thay đổi và thêm nhiều dòng
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DrugPreventionSystem API v1 (Local/Current)");
                c.SwaggerEndpoint("https://drugpreventionsystem-bzfxb7cndxdtdjbr.eastasia-01.azurewebsites.net/swagger/v1/swagger.json", "DrugPreventionSystem API v1 (Deployed to Azure)");
            });

            app.MapControllers();
            app.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Server API is running...");
            });

            app.UseAuthorization();

            app.Run();
        }
    }
}
