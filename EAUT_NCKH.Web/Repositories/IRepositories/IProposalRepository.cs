using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Request;
using EAUT_NCKH.Web.Models;
using Org.BouncyCastle.Tls;

namespace EAUT_NCKH.Web.Repositories.IRepositories {
    public interface IProposalRepository {
        Task<Response> RequireToSubmitProposal(string id, DateTime deadline, int senderId);
        Task<Response> SubmitProposal(int senderId, string topicId, IFormFile file, string note);
        ResponseData<byte[]> GetProposalFile(string topicId);
        Response AddOrEditCommitteeAssignment(int senderId, CommitteeAddRequest request);
        ResponseData<List<TopicModelView>> GetTopicProposalAssignment(int senderId, ProposalAssignedIndexViewPage options);
        double GetTopicProposalAssignmentCount(int senderId, ProposalAssignedIndexViewPage options);
        Response ApproveProposal(int senderId, string topicId, int statusId, string feedback);
        Response CancelProposalApproval(int senderId, string topicId);
    }
}
