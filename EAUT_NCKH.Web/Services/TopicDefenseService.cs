using DocumentFormat.OpenXml.Presentation;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Request;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories;
using EAUT_NCKH.Web.Repositories.IRepositories;
using System.ComponentModel;

namespace EAUT_NCKH.Web.Services {
    public class TopicDefenseService {
        private readonly ITopicDefenseRepository _topicDefenseRepository;

        public TopicDefenseService(ITopicDefenseRepository topicDefenseRepository) {
            _topicDefenseRepository = topicDefenseRepository;
        }

        public async Task<List<TopicDefenseModelView>> GetDataTable(int senderId, TopicDefenseIndexViewPage options) {
            return await _topicDefenseRepository.GetDataTable(options, senderId);
        }

        public async Task<double> GetCountDataTable(int senderId, TopicDefenseIndexViewPage options) {
            return await _topicDefenseRepository.GetCountDataTable(options, senderId);
        }

        public async Task<Response> AddOrEditCommitteeAssignment(int senderId, CommitteeAddRequest request) {
            return await _topicDefenseRepository.AddOrEditCommitteeAssignment(senderId, request);
        }

        public async Task<Response> ModifyScore(int senderId, int prizeId, string topicId, int score, List<IFormFile> files, string removedImages) {
            return await _topicDefenseRepository.ModifyScore(senderId, prizeId, topicId, score, files, removedImages);
        }

        public async Task<ResponseData<Defenseassignment>> GetScoreDetails(int senderId, string encodedTopicId) {
            return await _topicDefenseRepository.GetScoreDetails(senderId, encodedTopicId);
        }
    }
}
