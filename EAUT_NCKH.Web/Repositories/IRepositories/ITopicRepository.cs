using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.Repositories.IRepositories {
    public interface ITopicRepository {

        Task<List<Topic>>? GetDataTable(TopicDataTableOptions options, int userId);
        Task<double> GetCountDataTable(TopicDataTableOptions options, int userId);
    }
}
