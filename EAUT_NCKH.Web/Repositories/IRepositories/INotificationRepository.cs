using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Models;
using Org.BouncyCastle.Asn1.Ocsp;

namespace EAUT_NCKH.Web.Repositories.IRepositories {
    public interface INotificationRepository {

        public Task<ResponseData<List<Notificationaccount>>> GetNotifications(int senderId, int startNotificationId, int len);
    }
}
