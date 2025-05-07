using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.DTOs.Request;
using Microsoft.Extensions.FileProviders;
using MimeKit;

namespace EAUT_NCKH.Web.Repositories.IRepositories {
    public interface IFinalResultRepository {

        Task<List<TopicModelView>>? GetDataTableAssignment(FinalResultAssignmentIndexViewPage options, int userId);
        Task<double> GetCountDataTableAssignment(FinalResultAssignmentIndexViewPage options, int userId);
        Response FinalResultRequest(int senderId, string encodedTopicId, DateTime datetime);
        Task<Response> SubmitFinal(int senderId, string topicId, IFormFile file);
        Task<Response> AddOrEditCommitteeAssignment(int senderId, CommitteeAddRequest request);
        Task<List<TopicModelView>>? GetDataTableApproval(FinalResultApprovalIndexViewPage options, int userId);
        Task<double> GetCountDataTableApproval(FinalResultApprovalIndexViewPage options, int userId);
        Task<Response> EvaluateFinalResult(FinalResultEvaluation evaluation, int userId);
        Task<ResponseData<List<EvaluationItem>>> GetEvaluationList(string topicId, int userId);
        Task<Response> NominateTopic(int senderId, string topicId);
    }
}
