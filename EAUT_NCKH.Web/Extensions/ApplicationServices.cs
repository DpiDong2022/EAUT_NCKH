using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using EAUT_NCKH.Web.Repositories;
using EAUT_NCKH.Web.Services;
using Microsoft.EntityFrameworkCore;
using EAUT_NCKH.Web.IRepositories;
using EAUT_NCKH.Web.Data;

namespace EAUT_NCKH.Web.Extensions
{
    public static class ApplicationServices {
        public static IServiceCollection AddAppServices(this IServiceCollection services, IConfiguration configuration) {
            string connectionString = configuration.GetValue<string>("ConnectionString:DefaultConnection");
            services.AddDbContext<EntityDataContext>(options =>
            options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

            services.AddScoped<ICommitteeRepository, CommitteeRepository>();
            services.AddScoped<CommitteeService>();

            services.AddScoped(typeof(ICategoryRepository<>),typeof(CategoryRepository<>));
            services.AddScoped<CategoryService>();

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<AccountService>();

            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddSingleton<AuthService>();

            services.AddScoped(typeof(LogService<>));

            services.AddScoped<ITopicRepository, TopicRepository>();
            services.AddScoped<TopicService>();

            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<StudentService>();

            services.AddScoped<LayoutFilter>();

            services.AddScoped<ReportService>();

            services.AddScoped<IProposalRepository, ProposalRepository>();
            services.AddScoped<ProposalService>();

            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<NotificationService>();


            services.AddScoped<IFinalResultRepository, FinalResultRepository>();
            services.AddScoped<FinalResultService>();

            services.AddScoped<ITopicDefenseRepository, TopicDefenseRepository>();
            services.AddScoped<TopicDefenseService>();

            services.AddScoped<EmailService>();
            return services;
        }
    }
}
