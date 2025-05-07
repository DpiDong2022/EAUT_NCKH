using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace EAUT_NCKH.Web.Services {
    public class TopicService {
        private readonly ITopicRepository _repository;

        public TopicService(ITopicRepository repository) {
            _repository = repository;
        }

        public async Task<double> GetCountDataTable(TopicIndexViewPage options, int userId) {
            return await _repository.GetCountDataTable(options, userId);
        }

        public async Task<List<TopicModelView>>? GetDataTable(TopicIndexViewPage options, int userId) {
            return await _repository.GetDataTable(options, userId);
        }

        public async Task<ResponseData<Account>> GetSecondTeacherForTopicRegister(int senderId, string email) {
            return await _repository.GetSecondTeacherForTopicRegister(senderId, email);
        }

        public async Task<ResponseData<Student>> GetStudentTopic(int senderId, string studentCode) {
            return await _repository.GetStudentTopic(senderId, studentCode);
        }

        public async Task<Response> RegisterNewTopic(int senderId, NewTopicDto newTopic) {
            return await _repository.RegisterOrTopic(senderId, newTopic);
        }

        public async Task<Response> Delete(int id, int senderId) {
            return await _repository.Delete(id, senderId);
        }

        public async Task<ResponseData<TopicRegisterDetailDto>> GetTopicRegisteredDetail(int id, int senderId) {
            return await _repository.GetTopicRegisteredDetail(id, senderId);
        }

        public async Task<ResponseData<TopicModelView>> GetTopicAllDetails(string id, int senderId) {
            return await _repository.GetTopicAllDetails(id, senderId);
        }
    }
}
