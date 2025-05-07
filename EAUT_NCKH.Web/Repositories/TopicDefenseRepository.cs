using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Office2019.Excel.RichData2;
using DocumentFormat.OpenXml.Validation;
using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Request;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using HashidsNet;
using Microsoft.EntityFrameworkCore;

namespace EAUT_NCKH.Web.Repositories {
    public class TopicDefenseRepository: ITopicDefenseRepository {
        private readonly EntityDataContext _context;
        private readonly IConfiguration _configuration;
        private readonly Hashids _hashids;

        public TopicDefenseRepository(EntityDataContext context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
            _hashids = new Hashids(_configuration["EncryptKey:key"], _configuration.GetValue<int>("EncryptKey:min"));
            _configuration = configuration;
        }

        public async Task<double> GetCountDataTable(TopicDefenseIndexViewPage options, int userId) {
            var query = BuildDataTableQuery(options, userId);
            return await query.CountAsync();
        }

        public async Task<List<TopicDefenseModelView>>? GetDataTable(TopicDefenseIndexViewPage options, int userId) {
            var query = BuildDataTableQuery(options, userId);

            // Apply pagination
            var data = await query
            .Include(c => c.CreatedbyNavigation)
            .Include(c => c.StatusNavigation)
            .Skip(options.Pagination.GetStartRow())
            .Take(options.Pagination.PageLength)
            .Select(c => new TopicDefenseModelView(c,_hashids.Encode(c.Id)))
            .ToListAsync();

            foreach(var topic in data) {
                var sum = topic.Finalresultevaluations.Where(c => c.Criteria.Type).Sum(c => int.Parse(c.Value));
                if (sum == 0) {
                    topic.TongDiem = 0;
                    continue;
                }
                else {
                    topic.TongDiem = sum / topic.Finalresultevaluations.Select(c => c.Createdby).Distinct().Count();
                }
            }

            return data;
        }
        
        private IQueryable<Topic> BuildDataTableQuery(TopicDefenseIndexViewPage options, int userId) {
            var senderAccount = _context.Accounts
            .Include(c => c.Students)
            .FirstOrDefault(a => a.Id == userId);

            if (senderAccount == null) {
                return new List<Topic>().AsQueryable();
            }

            // Start building the query for topics
            var query = _context.Topics
            .Include(t => t.Finalresults)
            .Include(t => t.Finalresultevaluations)
            .ThenInclude(t => t.Criteria)
            .Include(t => t.Proposalevaluations)
            .Where(c =>
                (options.DepartmentId == -1 || c.Departmentid == options.DepartmentId)
                && (options.Year == -1 || c.Year == options.Year)
                && (string.IsNullOrEmpty(options.Search) || EF.Functions.FreeText(c.Title, options.Search))
            );

            var minStatus = (int)TopicStatusEnumId.NOMINATED_FOR_UNIVERSITY_DEFENSE;

            if (options.Status == -1) {
                query = query.Where(c => c.Status >= minStatus);
            }
            else {
                query = query.Where(c => c.Status == options.Status);
            }

            query = query.OrderByDescending(t => t.Updateddate);

            return query;
        }
         public async Task<Response> AddOrEditCommitteeAssignment(int senderId, CommitteeAddRequest request) {
            var topicId = _hashids.Decode(request.topicid).FirstOrDefault();
            var check = GeneralCheckCommitteeAssign(request, topicId);
            if (check.code != 0) {
                return check;
            }
            if (request.id == 0) {
                return await AddCommitteeAssignment(request, topicId);
            } 
            return await EditCommitteeAssignment(request, topicId);
        }

        private Response GeneralCheckCommitteeAssign(CommitteeAddRequest request, int topicId) {

            var topic = _context.Topics
                .Include(c => c.Proposals)
                .FirstOrDefault(c => c.Id == topicId);

            if (topic == null) {
                return new Response("Đề tài không tồn tại");
            }

            if (string.IsNullOrWhiteSpace(request.committeename)) {
                return new Response("Vui lòng nhập tên Hội đồng");
            }

            if (request.committeename.Length > 120) {
                return new Response("Tên Hội đồng quá dài, chiều dài tối đa là 120 kí tự");
            }

            if (request.members == null || !request.members.Any()) {
                return new Response("Vui lòng chọn thành viên Hội đồng");
            }

            if (request.members.Count > 7) {
                return new Response("Số lượng thành viên trong Hội đồng tối đa là 7 người");
            }

            foreach (var member in request.members) {
                var account = _context.Accounts.FirstOrDefault(c => c.Id == member.accountid);
                if (account == null) {
                    return new Response($"Người dùng {member.fullname} không tồn tại");
                }
                if ((topic.Createdby == account.Id) || (topic.Secondteacherid != null && topic.Secondteacherid == account.Id)) {
                    return new Response("Giảng viên hướng dẫn không được phép tham gia với vai trò là thành viên hội đồng của đề tài");
                }
            }

            int chuTichCount = request.members.Count(m => m.roleid == (int)CommitteeRoleEnumId.DEFENSE_CHU_TICH_HD);
            if (chuTichCount != 1) {
                return new Response("Thành phần hội đồng cần bảo đảm có một và chỉ một Chủ tịch.");
            }

            int thuKyCount = request.members.Count(m => m.roleid == (int)CommitteeRoleEnumId.DEFENSE_RESULT_THU_KY_HD);
            if (thuKyCount > 2) {
                return new Response("Hội đồng không được quá 2 Thư ký");
            }

            int phanBienCout = request.members.Count(m => m.roleid == (int)CommitteeRoleEnumId.DEFENSE_RESULT_PHAN_BIEN);
            if (phanBienCout <= 0) {
                return new Response("Thành phần hội đồng cần bảo đảm có Thành viên phản biện");
            }

            if (request.members.Any(m => m.roleid == -1)) {
                return new Response("Vui lòng chọn vai trò cho tất cả thành viên hội đồng");
            }

            if (request.buildingid == -1) {
                return new Response("Vui lòng chọn địa điểm");
            }

            if (request.roomid == -1) {
                return new Response("Vui lòng chọn phòng họp");
            }

            DateTime inputDateTime = request.datetime;
            var now = DateTime.Now;
            var minDate = now.AddDays(1).Date;
            var maxDate = now.AddMonths(2).Date;

            if (inputDateTime.Date < minDate || inputDateTime.Date > maxDate)
                return new Response("Thời gian phải từ 1 ngày sau hiện tại đến trong vòng 2 tháng tới");

            var committeeType = (int)CommitteeTypeEnumId.DEFENSE_COMMITTEE;
            var committee = _context.Committees
                .Include(c => c.Committeemembers)
                .FirstOrDefault(c => c.Topicid == topicId && c.Typeid == committeeType);
            if (committee == null)
                return new ResponseOk();

            var currentMembers = committee.Committeemembers
                .Select(member => new { accountid = member.Accountid, roleid = member.Roleid })
                .OrderBy(c => c.accountid)
                .ToList();
            var inputMembers = request.members
                .Select(c => new { accountid = c.accountid, roleid = c.roleid })
                .OrderBy(c => c.accountid)
                .ToList();

            var isSameMembers = currentMembers.SequenceEqual(inputMembers);

            if (request.committeename == committee.Name
                && request.buildingid == committee.Buildingid
                && request.roomid == committee.Roomid
                && request.datetime == committee.Eventdate
                && isSameMembers) {
                return new Response("Thông tin phân công hội đồng không có sự thay đổi");
            }

            return new ResponseOk();
        }

        private async Task<Response> AddCommitteeAssignment(CommitteeAddRequest request, int topicId) {
            using (var transaction = _context.Database.BeginTransaction()) {
                try {

                    var topic = _context.Topics
                        .Include(c => c.Department)
                        .Include(c => c.Proposals)
                        .FirstOrDefault(c => c.Id == topicId);
                    topic.Status = (int)TopicStatusEnumId.SELECTED_FOR_UNIVERSITY_DEFENSE;
                    topic.Updateddate = DateTime.Now;
                    _context.Topics.Update(topic);

                    var oldCommittee = _context.Committees.FirstOrDefault(c => c.Topicid == topicId && c.Typeid == (int)CommitteeTypeEnumId.DEFENSE_COMMITTEE);
                    if (oldCommittee != null) {
                        return new Response("Lỗi, đề tài này đã được phân công hội đồng bảo vệ");
                    }

                    var committee = new Committee{
                        Topicid = topicId,
                        Typeid = (int)CommitteeTypeEnumId.DEFENSE_COMMITTEE,
                        Name = request.committeename,
                        Buildingid = request.buildingid,
                        Roomid = request.roomid,
                        Eventdate = request.datetime,
                    };
                    _context.Committees.Add(committee);
                    _context.SaveChanges();

                    foreach (var member in request.members) {
                        var committeeMember = new Committeemember{
                            Committeeid = committee.Id,
                            Accountid = member.accountid,
                            Roleid = member.roleid
                        };
                        _context.Committeemembers.Add(committeeMember);
                    }

                    // Gửi thông báo app và email
                    // Gửi thông báo cho thành viên hội đồng
                    foreach (var member in request.members) {
                        await CreateNotification(member.accountid, (int)NotificationType.DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                        var memberAcc = _context.Accounts.FirstOrDefault(c => c.Id == member.accountid);
                        if (memberAcc != null) {
                        }
                    }
                    await CreateNotification(topic.Studentid??0, (int)NotificationType.DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                    await CreateNotification(topic.Createdby, (int)NotificationType.DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                    if (topic.Secondteacherid != null) {
                        await CreateNotification(topic.Secondteacherid??0, (int)NotificationType.DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    return new Response(0, "Phân công hội đồng và gửi thông báo đến giảng viên đã được thực hiện thành công");
                } catch (Exception e) {
                    transaction.Rollback();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
        }
        private async Task<Response> EditCommitteeAssignment(CommitteeAddRequest request, int topicId) {
            using (var transaction = _context.Database.BeginTransaction()) {
                try {

                    var topic = _context.Topics
                        .Include(c => c.Department)
                        .Include(c => c.Proposals)
                        .FirstOrDefault(c => c.Id == topicId);

                    var committee = _context.Committees
                    .FirstOrDefault(c => c.Topicid == topicId && c.Typeid == (int)CommitteeTypeEnumId.DEFENSE_COMMITTEE);

                    if (committee == null)
                        return new Response("Hội đồng chưa được tạo để chỉnh sửa");

                    // Cập nhật thông tin hội đồng
                    committee.Updateddate = DateTime.Now;
                    committee.Name = request.committeename;
                    committee.Eventdate = request.datetime;
                    committee.Buildingid = request.buildingid;
                    committee.Roomid = request.roomid;
                    _context.Committees.Update(committee);

                    //Old members
                    var oldMembers = _context.Committeemembers
                        .Include(c => c.Account)
                        .Where(c => c.Committeeid == committee.Id).ToList();

                    // IDs thành viên mới và cũ;
                    var oldMemberIds = oldMembers.Select(c => c.Accountid).ToList();
                    var newMemberIds = request.members.Select(c => c.accountid).ToList();

                    // Thành viên xóa, thêm, cập nhật
                    var membersToAdd = request.members.Where(c => !oldMemberIds.Contains(c.accountid)).ToList();
                    var membersToRemove = oldMembers.Where(c => !newMemberIds.Contains(c.Accountid)).ToList();
                    var membersToKeep = oldMembers.Where(c => newMemberIds.Contains(c.Accountid)).ToList();

                    // action
                    _context.Committeemembers.RemoveRange(membersToRemove);

                    foreach (var update in membersToKeep) {
                        update.Roleid = request.members.FirstOrDefault(c => c.accountid == update.Accountid).roleid;
                        _context.Committeemembers.Update(update);
                    }
                    foreach (var add in membersToAdd) {
                        var committeeMember = new Committeemember{
                            Committeeid = committee.Id,
                            Accountid = add.accountid,
                            Roleid = add.roleid
                        };
                        _context.Committeemembers.Add(committeeMember);
                    }

                    // thông báo update
                    foreach (var updated in membersToKeep) {
                        await CreateNotification(updated.Accountid, (int)NotificationType.UPDATE_DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                    }

                    foreach (var added in membersToAdd) {
                        await CreateNotification(added.accountid, (int)NotificationType.UPDATE_DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                    }

                    foreach (var removed in membersToRemove) {
                        await CreateNotification(removed.Accountid, (int)NotificationType.UPDATE_DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                    }
                    await CreateNotification(topic.Studentid ?? 0, (int)NotificationType.UPDATE_DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                    await CreateNotification(topic.Createdby, (int)NotificationType.UPDATE_DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                    if (topic.Secondteacherid != null) {
                        await CreateNotification(topic.Secondteacherid ?? 0, (int)NotificationType.UPDATE_DEFENSE_ASSIGNMENT, topic.Title, _hashids.Encode(topic.Id));
                    }


                    _context.SaveChanges();
                    transaction.Commit();

                    return new Response(0, "Phân công hội đồng và gửi thông báo đến giảng viên đã được thực hiện thành công");
                } catch (Exception e) {
                    transaction.Rollback();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
        }

        private async Task CreateNotification(int receiverId, int notificationId, string topicName, string topicId) {
            var notificationTemplate = await _context.Notifications
            .Where(n => n.Id == notificationId)
            .Select(n => n.Notificationtemplate)
            .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(notificationTemplate))
                return;

            string content = notificationTemplate
            .Replace("{title}", topicName);

            var notificationAccount = new Notificationaccount
            {
                Receiverid = receiverId,
                Notificationid = notificationId,
                Notificationcontent = content,
                Link = $"{RouterName.GET_TOPIC_DETAIL}?target={topicId}"
            };

            await _context.Notificationaccounts.AddAsync(notificationAccount);
        }

        public async Task<Response> ModifyScore(int senderId, string topicId, int score, IFormFile files) {
            var senderAcc = _context.Accounts.FirstOrDefault(c =>c.Id == senderId);
            if(senderAcc == null) {
                return new Response("Lỗi tài khoản");
            }

            var decodedTopicId = _hashids.Decode(topicId).First();
            var topic = _context.Topics
                .Include(c => c.Defenseassignments)
                .FirstOrDefault(c => c.Id == decodedTopicId);

            if(topic == null) {
                return new Response("Đề tài không tồn tại");
            }

            if (topic.Status != (int)TopicStatusEnumId.SELECTED_FOR_UNIVERSITY_DEFENSE && topic.Status != (int)TopicStatusEnumId.COMPLETED) {
                return new Response("Đề tài không đủ điều kiện để đánh giá");
            }

            using (var transaction = _context.Database.BeginTransaction()) {
                try {
                    var defensescores = topic.Defenseassignments.FirstOrDefault(c => c.Topicid == topic.Id);
                    if (defensescores == null) {
                        var newDefense = new Defenseassignment{
                            Topicid = topic.Id,
                            Finalscore = score
                        };
                        _context.Defenseassignments.Add(newDefense);
                    } else {
                        defensescores.Finalscore = score;
                        defensescores.Updateddate = DateTime.Now;
                        _context.Defenseassignments.Update(defensescores);
                    }
                    
                    topic.Status = (int)TopicStatusEnumId.COMPLETED;
                    topic.Updateddate = DateTime.Now;

                    _context.Topics.Update(topic);

                    _context.SaveChanges();
                    await transaction.CommitAsync();

                    return new Response(0, "Nhập điểm thành công");
                }
                catch(Exception e) {
                    transaction.Rollback();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
        }
    }
}
