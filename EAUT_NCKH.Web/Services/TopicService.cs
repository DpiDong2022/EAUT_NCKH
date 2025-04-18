using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace EAUT_NCKH.Web.Services {
    public class TopicService {
        private readonly ITopicRepository _repository;

        public TopicService(ITopicRepository repository) {
            _repository = repository;
        }

        public async Task<double> GetCountDataTable(TopicDataTableOptions options, int userId) {
            return await _repository.GetCountDataTable(options, userId);
        }

        public async Task<List<Topic>>? GetDataTable(TopicDataTableOptions options, int userId) {
            return await _repository.GetDataTable(options, userId);
        }

        public async Task<ResponseData> GetSecondTeacherForTopicRegister(int senderId, string email) {
            return await _repository.GetSecondTeacherForTopicRegister(senderId, email);
        }

        public async Task<ResponseData> GetStudentTopic(int senderId, int studentCode) {
            return await _repository.GetStudentTopic(senderId, studentCode);
        }
    }
}
