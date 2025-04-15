using EAUT_NCKH.Web.Extensions;
using EAUT_NCKH.Web.Middlewares;
using Serilog;

namespace EAUT_NCKH.Web {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAppServices(builder.Configuration);
            builder.Services.AddAuthentication(builder.Configuration);

            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(
                "Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}"
            ).CreateLogger();
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllersWithViews(options => {
                options.Filters.Add<LogExceptionFilter>();
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
