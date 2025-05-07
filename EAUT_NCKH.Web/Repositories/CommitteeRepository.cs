using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using HashidsNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace EAUT_NCKH.Web.Repositories {
    public class CommitteeRepository: ICommitteeRepository {
        private readonly EntityDataContext _context;
        private readonly IConfiguration _configuration;
        private readonly Hashids _hashids;
        public CommitteeRepository(EntityDataContext context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
            _hashids = new Hashids(_configuration["EncryptKey:key"], _configuration.GetValue<int>("EncryptKey:min"));
        }

        public ResponseData<Committee> GetProposalCommittee(int senderId, string encodedTopicId) {
            var topicId = _hashids.Decode(encodedTopicId).First();
            var data = _context.Committees
                .Include(c => c.Committeemembers)
                .ThenInclude(c => c.Account)
                .FirstOrDefault(c => c.Topicid == topicId && c.Typeid == (int)CommitteeTypeEnumId.PROPOSAL_COMMITTEE);

            return new ResponseData<Committee>(0, "OK", data);
        }

        public async Task<List<Committeetype>> GetCommitteeTypes() {
            var data = _context.Committeetypes;
            if (data != null) {
                return await data.ToListAsync();
            }
            return new List<Committeetype>();
        }

        public async Task<ResponseData<List<Committee>>> GetDataTable(int senderId, CommitteeIndexViewPage option) {

            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);
            if (senderAcc == null) { 
                return new ResponseData<List<Committee>>("NULL");
            }
            if (senderAcc.Roleid != (int)RoleEnumId.RESEARCH_ADVISOR) {
                option.DepartmentId = senderAcc.Departmentid??-1;
            }

            var query = BuildDataTableQuery(option);

            var data = await query
                .Skip(option.Pagination.GetStartRow())
                .Take(option.Pagination.PageLength)
                .ToListAsync();
            return new ResponseData<List<Committee>>(0, "OK", data);
        }

        public double GetDataTableCount(int senderId, CommitteeIndexViewPage option) {
            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);
            if (senderAcc == null) {
                return 0;
            }
            if (senderAcc.Roleid != (int)RoleEnumId.RESEARCH_ADVISOR) {
                option.DepartmentId = senderAcc.Departmentid ?? -1;
            }

            var query = BuildDataTableQuery(option);
            return query.Count();
        }

        private IQueryable<Committee> BuildDataTableQuery(CommitteeIndexViewPage option) {

            var topicId = (int)TopicStatusEnumId.PROPOSAL_REVIEW_ASSIGNMENT;
            var query = _context.Committees
                .Include(t => t.Type)
                .Include(t => t.Topic)
                .ThenInclude(c => c.Department)
                .Include(t => t.Committeemembers)
                .ThenInclude(t => t.Role)
                .Include(t => t.Committeemembers)
                .ThenInclude(t => t.Account)
                .ThenInclude(t => t.Department)
                .Where(c =>
                    (option.DepartmentId == -1 || c.Topic.Departmentid == option.DepartmentId)
                    && (option.BuildingId == -1 || c.Buildingid == option.BuildingId)
                    && (option.RoomId == -1 || c.Roomid == option.RoomId)
                    && (option.TypeId == -1 || c.Typeid == option.TypeId)
                    && (string.IsNullOrEmpty(option.Search) || EF.Functions.FreeText(c.Name, option.Search) || EF.Functions.FreeText(c.Topic.Title, option.Search))
                    && (c.Eventdate >= option.From && c.Eventdate <= option.To)
                )
                .OrderByDescending(c => c.Eventdate)
                .ThenByDescending(c => c.Topic.Updateddate)
                .AsQueryable();

            return query;
        }

        public ResponseData<Committee> GetFinalCommittee(int senderId, string encodedTopicId) {
            var topicId = _hashids.Decode(encodedTopicId).First();
            var data = _context.Committees
                .Include(c => c.Committeemembers)
                .ThenInclude(c => c.Account)
                .FirstOrDefault(c => c.Topicid == topicId && c.Typeid == (int)CommitteeTypeEnumId.FINAL_COMMITTEE);

            return new ResponseData<Committee>(0, "OK", data);
        }

        public ResponseData<Committee> GetDefenseCommittee(int senderId, string encodedTopicId) {
            var topicId = _hashids.Decode(encodedTopicId).First();
            var data = _context.Committees
                .Include(c => c.Committeemembers)
                .ThenInclude(c => c.Account)
                .FirstOrDefault(c => c.Topicid == topicId && c.Typeid == (int)CommitteeTypeEnumId.DEFENSE_COMMITTEE);

            return new ResponseData<Committee>(0, "OK", data);
        }
    }
}
