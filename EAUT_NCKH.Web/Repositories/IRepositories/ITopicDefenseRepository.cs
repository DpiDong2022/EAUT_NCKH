using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Request;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.Repositories.IRepositories {
    public interface ITopicDefenseRepository {
        Task<List<TopicDefenseModelView>> GetDataTable(TopicDefenseIndexViewPage options, int userId);
        Task<double> GetCountDataTable(TopicDefenseIndexViewPage options, int userId);
        Task<Response> AddOrEditCommitteeAssignment(int senderId, CommitteeAddRequest request);
        Task<Response> ModifyScore(int senderId, int prizeId, string topicId, int score, List<IFormFile> files, string removedImages);
        Task<ResponseData<Defenseassignment>> GetScoreDetails(int senderId, string encodedTopicId);
    }
}
