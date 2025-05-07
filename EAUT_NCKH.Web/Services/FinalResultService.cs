using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.DTOs.Request;
using EAUT_NCKH.Web.Repositories.IRepositories;

namespace EAUT_NCKH.Web.Services {
    public class FinalResultService {

        private readonly IFinalResultRepository _finalResultRepository;

        public FinalResultService(IFinalResultRepository finalResultRepository) {
            _finalResultRepository = finalResultRepository;
        }

        public async Task<List<TopicModelView>> GetDataTableAssignment(int senderId, FinalResultAssignmentIndexViewPage options) {
            return await _finalResultRepository.GetDataTableAssignment(options, senderId);
        }

        public async Task<double> GetCountDataTableAssignment(int senderId, FinalResultAssignmentIndexViewPage options) {
            return await _finalResultRepository.GetCountDataTableAssignment(options, senderId);
        }

        public Response FinalResultRequest(int senderId, string encodedTopicId, DateTime datetime) {
            return _finalResultRepository.FinalResultRequest(senderId, encodedTopicId, datetime);
        }
        public async Task<Response> SubmitFinal(int senderId, string topicId, IFormFile file) {
            return await _finalResultRepository.SubmitFinal(senderId, topicId, file);
        }

        public async Task<Response> AddOrEditCommitteeAssignment(int senderId, CommitteeAddRequest request) {
            return await _finalResultRepository.AddOrEditCommitteeAssignment(senderId, request);
        }

        public async Task<List<TopicModelView>> GetDataTableApproval(int senderId, FinalResultApprovalIndexViewPage options) {
            return await _finalResultRepository.GetDataTableApproval(options, senderId);
        }

        public async Task<double> GetCountDataTableApproval(int senderId, FinalResultApprovalIndexViewPage options) {
            return await _finalResultRepository.GetCountDataTableApproval(options, senderId);
        }

        public async Task<Response> EvaluateFinalResult(FinalResultEvaluation evaluation, int userId) {
            return await _finalResultRepository.EvaluateFinalResult(evaluation, userId);
        }
        public async Task<ResponseData<List<EvaluationItem>>> GetEvaluationList(string topicId, int userId) {
            return await _finalResultRepository.GetEvaluationList(topicId, userId);
        }
        public async Task<Response> NominateTopic(int senderId, string topicId) {
            return await _finalResultRepository.NominateTopic(senderId, topicId);
        }
    }
}
