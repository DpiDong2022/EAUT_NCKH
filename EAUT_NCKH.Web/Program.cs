using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Middlewares;
using Microsoft.AspNetCore.Http.Features;
using Serilog;
using System.Text.Json.Serialization;

namespace EAUT_NCKH.Web {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAppServices(builder.Configuration);
            builder.Services.AddAuthentication(builder.Configuration);

            // maximum file size
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 150 * 1024 * 1024; // 150 MB
            });
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 150 * 1024 * 1024; // 150 MB
            });

            Log.Logger = new LoggerConfiguration()
            //.WriteTo.File(
            //    "Logs/log-.txt",
            //    rollingInterval: RollingInterval.Day,
            //    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllersWithViews(options => {
                options.Filters.Add<LogExceptionFilter>();
            });
            builder.Services.AddControllers().AddJsonOptions(options => {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            var app = builder.Build();

            if (app.Environment.IsDevelopment()) {
                app.UseDeveloperExceptionPage();  // Enable detailed error pages in development
            } else {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }


            // Passwords sent over HTTP can be intercepted.
            app.UseHttpsRedirection();
            app.UseHsts();

            app.UseStaticFiles();

            app.UseRouting();

            // secure
            app.UseRateLimiter();

            app.UseSession();
            app.UseMiddleware<TokenMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            //app.MapControllerRoute(
            //    name: "default",
            //    pattern: "{controller=trang-chu}/{action=Index}");

            app.UseEndpoints(endpoints => {
                endpoints.MapGet("/", context => {
                    context.Response.Redirect("/trang-chu");
                    return Task.CompletedTask;
                });

                endpoints.MapControllers();
            });

            app.Run();
        }
    }
}
