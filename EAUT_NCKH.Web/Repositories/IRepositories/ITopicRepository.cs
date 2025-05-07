using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.Repositories.IRepositories {
    public interface ITopicRepository {

        Task<List<TopicModelView>>? GetDataTable(TopicIndexViewPage options, int userId);
        Task<double> GetCountDataTable(TopicIndexViewPage options, int userId);
        Task<ResponseData<Account>> GetSecondTeacherForTopicRegister(int senderId, string email);
        Task<ResponseData<Student>> GetStudentTopic(int senderId, string studentCode);
        Task<Response> RegisterOrTopic(int senderId, NewTopicDto newTopic);
        Task<Response> Delete(int id, int senderId);
        Task<ResponseData<TopicRegisterDetailDto>> GetTopicRegisteredDetail (int id, int senderId);
        Task<ResponseData<TopicModelView>> GetTopicAllDetails(string id, int senderId);
    }
}
