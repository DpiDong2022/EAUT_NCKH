using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using EAUT_NCKH.Web.Services;
using HashidsNet;
using Microsoft.EntityFrameworkCore;

namespace EAUT_NCKH.Web.Repositories {
    public class NotificationRepository: INotificationRepository {
        private readonly EntityDataContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly Hashids _hashids;

        public NotificationRepository(EntityDataContext context, EmailService emailService, IConfiguration configuration) {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
            _hashids = _hashids = new Hashids(_configuration["EncryptKey:key"], _configuration.GetValue<int>("EncryptKey:min"));
        }

        public async Task<ResponseData<List<Notificationaccount>>> GetNotifications(int id, int startNotificationId, int len) {

            var senderAcc = await _context.Accounts.FirstOrDefaultAsync(c => c.Id == id);
            if (senderAcc == null) {
                return new ResponseData<List<Notificationaccount>>("Người dùng không tồn tại");
            }

            IQueryable<Notificationaccount> query;
            if (senderAcc.Roleid == (int)RoleEnumId.SCIENTIFIC_RESEARCH_OFFICE) {
                query = _context.Notificationaccounts
                .Include(c => c.Notification);
            } else {
                query = _context.Notificationaccounts
                .Include(c => c.Notification)
                .Where(c => c.Receiverid == id);
            }

            

            if (startNotificationId > 0) {
                query = query.Where(c => c.Id < startNotificationId);
            }

            var notifications = await query
            .OrderByDescending(c => c.Id)
            .Take(len) // Optional: lấy tối đa 20 cái mỗi lần load thêm
            .ToListAsync();

            return new ResponseData<List<Notificationaccount>>(0, "OK", notifications);
        }

    }
}
