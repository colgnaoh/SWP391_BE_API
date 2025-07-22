using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DotNetEnv;
using DrugPreventionSystemBE.DrugPreventionSystem.Data;
using DrugPreventionSystemBE.DrugPreventionSystem.Filter;
using DrugPreventionSystemBE.DrugPreventionSystem.ModelView.PayOS;
using DrugPreventionSystemBE.DrugPreventionSystem.Service;
using DrugPreventionSystemBE.DrugPreventionSystem.Service.Interface;
using DrugPreventionSystemBE.DrugPreventionSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Stripe;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace DrugPreventionSystemBE
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHttpContextAccessor();
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
                //c.AddSecurityRequirement(new OpenApiSecurityRequirement
                //{
                //    {
                //        new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                //        new string[] {}
                //    }
                //});
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                c.SchemaFilter<EnumSchemaFilter>();
            });

            builder.Services.AddHttpClient("PayOSClient", client =>
            {
                var payOsBaseUrl = builder.Configuration["PayOS:BaseUrl"];
                if (string.IsNullOrEmpty(payOsBaseUrl))
                {
                    throw new InvalidOperationException("PayOS BaseUrl is not configured in appsettings.json.");
                }
                client.BaseAddress = new Uri(payOsBaseUrl);

                client.DefaultRequestHeaders.Add("x-client-id", builder.Configuration["PayOS:ClientId"]);
                client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration["PayOS:ApiKey"]);

            });


            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IdServices>();
            builder.Services.AddScoped<IBlogService, BlogService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICommunityProgramService, CommunityProgramService>();
            builder.Services.AddScoped<IReviewService, DrugPreventionSystem.Service.ReviewService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IConsultantService, ConsultantService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ISessionService, SessionService>();
            builder.Services.AddScoped<ILessonService, LessonService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IPayOSSignatureService, PayOSSignatureService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<ISurveyService, SurveyService>();
            builder.Services.AddScoped<IAnswerOptionService, AnswerOptionService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            Env.Load();

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            // Kiểm tra cấu hình Facebook
            //if (string.IsNullOrEmpty(builder.Configuration["Authentication:Facebook:AppId"]) || string.IsNullOrEmpty(builder.Configuration["Authentication:Facebook:AppSecret"]))
            //{
            //    throw new InvalidOperationException("Cấu hình Facebook AppId hoặc AppSecret không được định nghĩa.");
            //}

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // Đặt DefaultScheme là JWT Bearer
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; // Đặt DefaultChallengeScheme là JWT Bearer
            })
            .AddCookie()
            //.AddFacebook(facebookOptions =>
            //{
            //    facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"];
            //    facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
            //})
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
                };


            });




            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", builder =>
                {
                    builder.WithOrigins(
                        "https://drugpreventionnow.io.vn",
                        "http://localhost:3000",
                        "https://drug-abuse-prevention.vercel.app",
"https://drugpreventionsystem-bzfxb7cndxdtdjbr.eastasia-01.azurewebsites.net" // 
                    )
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            var app = builder.Build();

            var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
            app.Urls.Add($"http://*:{port}");
            app.MapGet("/", () => Results.Redirect("/swagger"));

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                var url = $"http://localhost:{port}/swagger";
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch { }
            });


            app.UseForwardedHeaders();

            app.UseCors("AllowSpecificOrigin");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseHttpsRedirection();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DrugPreventionSystem API v1 (Local/Current)");
                c.RoutePrefix = "swagger";
            });



            app.MapControllers();
            //app.MapGet("/", async context =>
            //{
            //    await context.Response.WriteAsync("Server API is running...");
            //});docker rm $(docker ps -aq)
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