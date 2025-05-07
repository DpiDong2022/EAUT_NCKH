using DocumentFormat.OpenXml.Spreadsheet;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Request;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using MimeKit;

namespace EAUT_NCKH.Web.Services {
    public class ProposalService {
        private readonly IProposalRepository _repository;
        public ProposalService(IProposalRepository repository) {
            _repository = repository;
        }

        public async Task<Response> RequireToSubmitProposal(string id, DateTime deadline, int senderId) {
            return await _repository.RequireToSubmitProposal(id, deadline, senderId);
        }

        public async Task<Response> SubmitProposal(int senderId, string topicId, IFormFile file, string note) {
            return await _repository.SubmitProposal(senderId, topicId, file, note);
        }

        public ResponseData<byte[]> GetProposalFile(string topicIdEncoded) {
            return _repository.GetProposalFile(topicIdEncoded);
        }

        public Response AddOrEditCommitteeAssignment(int senderId, CommitteeAddRequest request) {
            return _repository.AddOrEditCommitteeAssignment(senderId, request);
        }

        public ResponseData<List<TopicModelView>> GetTopicProposalAssignedList(int senderId, ProposalAssignedIndexViewPage options) {
            return _repository.GetTopicProposalAssignment(senderId, options);
        }

        public double GetTopicProposalAssignedListCount(int senderId, ProposalAssignedIndexViewPage options) {
            return _repository.GetTopicProposalAssignmentCount(senderId, options);
        }

        public Response ApproveProposal(int senderId, string topicId, int statusId, string feedback) {
            return _repository.ApproveProposal(senderId, topicId, statusId, feedback);
        }
        public Response CancelProposalApproval(int senderId, string topicId) {
            return _repository.CancelProposalApproval(senderId, topicId);
        }
    }
}
