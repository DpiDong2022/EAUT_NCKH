using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;

namespace EAUT_NCKH.Web.Services {
    public class NotificationService {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository) {
            _notificationRepository = notificationRepository;
        }

        public async Task<ResponseData<List<Notificationaccount>>> GetNotification(int senderId, int startNotificationId, int len) {
            return await _notificationRepository.GetNotifications(senderId, startNotificationId, len);
        }
    }
}
