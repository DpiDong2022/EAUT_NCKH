using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

namespace EAUT_NCKH.Web.Extensions {
    public static class AuthenticationServices {

        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration) {

            services.AddHttpContextAccessor();
            services.AddSession();
            // JWT
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });
            //services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            //.AddCookie(options => {
            //    options.LoginPath = "/Login/Index";
            //    options.SlidingExpiration = true;
            //    options.Cookie.IsEssential = true;
            //    options.Cookie.HttpOnly = true;
            //    options.ExpireTimeSpan = TimeSpan.FromHours(1);
            //    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //});

            services.AddAuthorization();
            // Rate limiter
            services.AddRateLimiter(options => {
                options.AddFixedWindowLimiter("login", otp => {
                    otp.PermitLimit = configuration.GetValue<int>("RateLimit");
                    otp.Window = TimeSpan.FromMinutes(5);
                    otp.QueueLimit = 0;
                });
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) => {

                    context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
                    await context.HttpContext.Response.WriteAsync("Bạn đã gửi request quá nhiều lần, hãy thử lại sau!", cancellationToken);
                };
            });
            return services;
        }
    }
}
