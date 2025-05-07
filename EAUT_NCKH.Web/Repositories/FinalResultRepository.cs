using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Wordprocessing;
using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.DTOs.Request;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Helpers;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using EAUT_NCKH.Web.Services;
using HashidsNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EAUT_NCKH.Web.Repositories {
    public class FinalResultRepository: IFinalResultRepository {

        private readonly EntityDataContext _context;
        private readonly IConfiguration _configuration;
        private readonly Hashids _hashids;
        private readonly EmailService _emailService;

        public FinalResultRepository(EntityDataContext context, IConfiguration configuration, EmailService emailService) {
            _context = context;
            _configuration = configuration;
            _hashids = new Hashids(_configuration["EncryptKey:key"], _configuration.GetValue<int>("EncryptKey:min"));
            _emailService = emailService;
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

        public async Task<double> GetCountDataTableAssignment(FinalResultAssignmentIndexViewPage options, int userId) {
            var query = BuildDataTableAssignmentQuery(options, userId);
            return await query.CountAsync();
        }

        public async Task<List<TopicModelView>> GetDataTableAssignment(FinalResultAssignmentIndexViewPage options, int userId) {
            var query = BuildDataTableAssignmentQuery(options, userId);

            // Apply pagination
            var data = await query
                .Include(c => c.CreatedbyNavigation)
                .Include(c => c.StatusNavigation)
                .Skip(options.Pagination.GetStartRow())
                .Take(options.Pagination.PageLength)
                .Select(c => new TopicModelView(c, _hashids.Encode(c.Id), false))
                .ToListAsync();

            return data;
        }

        public Response FinalResultRequest(int senderId, string encodedTopicId, DateTime datetime) {

            var senderAcc = _context.Accounts
                .Include(c => c.Students)
                .FirstOrDefault(a => a.Id == senderId);

            if (senderAcc == null) {
                return new Response("Tài khoản không hợp lệ");
            }

            var topicId = _hashids.Decode(encodedTopicId).FirstOrDefault();
            var topic = _context.Topics
                .Include(c => c.CreatedbyNavigation)
                .Include(c => c.Student)
                .Include(c => c.Secondteacher)
                .Include(c => c.Proposalevaluations)
                .Include(c => c.CreatedbyNavigation)
                .FirstOrDefault(c => c.Id == topicId);
            if (topic == null) {
                return new Response("Đề tài không tồn tại");
            }

            if (senderAcc.Departmentid != topic.Departmentid) {
                return new Response("Bạn không có quyền yêu cầu nộp báo cáo cho đề tài này");
            }

            var validStatus1 = (int)TopicStatusEnumId.WAITING_FOR_FINAL_NOTICE;
            var validStatus2 = (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_UNIVERSITY;
            if (topic.Status != validStatus1 && topic.Status != validStatus2) {
                return new Response("Đề tài không đủ điều kiện để yêu cầu nộp báo cáo");
            }


            using (var transaction = _context.Database.BeginTransaction()) {
                try {

                    var finalResult = _context.Finalresults
                        .Include(c => c.Topic)
                        .FirstOrDefault(c => c.Topicid == topicId);

                    if (finalResult != null && topic.Status == validStatus1) {

                        finalResult.Submitdeadline = datetime;
                    } else {

                        var newFinalResult = new Finalresult {
                            Topicid = topicId,
                            Submitdeadline = datetime,
                        };
                        _context.Finalresults.Add(newFinalResult);
                    }

                    topic.Status = validStatus1;

                    var deadlineVN = MyHelper.FormatDateTimeVietnameseString(datetime);
                    var emails = new List<string>();
                    var emailTemplateTeacher = _emailService.GetEmailTemplate("FinalRequest.html");
                    var emailTemplateStudent = _emailService.GetEmailTemplate("FinalRequestStudent.html");

                    var notificationId = (int)NotificationType.REQUEST_SUBMIT_FINAL_RESULT;
                    // send app here
                    CreateNotification(topic.Createdby, notificationId, topic.Title, deadlineVN, encodedTopicId);
                    CreateNotification(topic.Studentid??0, notificationId, topic.Title, deadlineVN, encodedTopicId);
                    if (topic.Secondteacher != null) {
                        CreateNotification(topic.Secondteacherid??0, notificationId, topic.Title, deadlineVN, encodedTopicId);
                        emails.Add(topic.Secondteacher.Email);
                    }
                    emails.Add(topic.CreatedbyNavigation.Email);

                    _context.SaveChanges();

                    // send email here
                    _emailService.SendEmailAsync(
                        emails,
                        "[Thông báo][Yêu cầu nộp kết quả cuối cùng]",
                        emailTemplateTeacher.Replace("{title}", topic.Title).Replace("{deadline}", deadlineVN));

                    // send email here
                    _emailService.SendEmailAsync(
                        new List<string>() { topic.Student.Email },
                        "[Thông báo][Yêu cầu nộp kết quả cuối cùng]",
                        emailTemplateStudent.Replace("{title}", topic.Title).Replace("{deadline}", deadlineVN));
                    transaction.Commit();

                    return new Response(0, "Gửi yêu cầu thành công");
                }
                catch(Exception e) {
                    transaction.Rollback();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
        }

        private async Task CreateNotification(int receiverId, int notificationId, string topicName, string deadline, string topicId) {
            var notificationTemplate = await _context.Notifications
            .Where(n => n.Id == notificationId)
            .Select(n => n.Notificationtemplate)
            .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(notificationTemplate))
                return;

            string content = notificationTemplate
            .Replace("{title}", topicName)
            .Replace("{deadline}", deadline);

            var notificationAccount = new Notificationaccount
            {
                Receiverid = receiverId,
                Notificationid = notificationId,
                Notificationcontent = content,
                Link = $"{RouterName.GET_TOPIC_DETAIL}?target={topicId}"
            };

            await _context.Notificationaccounts.AddAsync(notificationAccount);
        }

        private IQueryable<Topic> BuildDataTableAssignmentQuery(FinalResultAssignmentIndexViewPage options, int userId) {
            var senderAccount = _context.Accounts
                .Include(c => c.Students)
                .FirstOrDefault(a => a.Id == userId);

            if (senderAccount == null) {
                return new List<Topic>().AsQueryable();
            }

            if (senderAccount.Roleid != (int)RoleEnumId.SCIENTIFIC_RESEARCH_OFFICE) {
                options.DepartmentId = senderAccount.Departmentid??-1;
            }

            // Start building the query for topics
            var query = _context.Topics
            .Include(t => t.Proposalevaluations)
            .Where(c =>
                (options.DepartmentId == -1 || c.Departmentid == options.DepartmentId)
                && (options.Year == -1 || c.Year == options.Year)
                && (string.IsNullOrEmpty(options.Search) || EF.Functions.FreeText(c.Title, options.Search))
            );

            var minStatus = (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_UNIVERSITY;
            var maxStatus = (int)TopicStatusEnumId.NOMINATED_FOR_UNIVERSITY_DEFENSE;
            var proposalApproved1 = (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_FACULTY;
            var proposalApproved2 = (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_UNIVERSITY;

            var deniedProposalStatus = (int)ProposalStatusId.REJECTED;
            if (options.Status == -1) {
                query = query.Where(c => c.Status >= minStatus && c.Status <= maxStatus);
            } else if(options.Status == proposalApproved1 || options.Status == proposalApproved2) {
                query = query.Where(c => 
                c.Status == options.Status
                && c.Proposalevaluations.Any(s => s.Statusid != deniedProposalStatus));
            } else {
                query = query.Where(c => c.Status == options.Status);
            }

            query = query.OrderByDescending(t => t.Proposalevaluations.Max(pe => pe.Createddate));

            return query;
        }

        public async Task<Response> SubmitFinal(int senderId, string topicId, IFormFile file) {
            var senderAccount = _context.Accounts
                .Include(c => c.Students)
                .FirstOrDefault(a => a.Id == senderId);

            if (senderAccount == null) {
                return new Response("Lỗi tài khoản");
            }

            if (senderAccount.Roleid != (int)RoleEnumId.STUDENT) {
                return new Response("Bạn không có quyền nộp kết quả");
            }

            if (topicId == null) {
                return new Response("Không tìm thấy Đề tài");
            }


            var decodeIdTopic = _hashids.Decode(topicId).FirstOrDefault();
            var topic = _context.Topics.Include(c => c.Proposals).FirstOrDefault(c => c.Id == decodeIdTopic);

            if (topic == null) {
                return new Response("Không tìm thấy Đề tài");
            }

            var finalResult = _context.Finalresults.FirstOrDefault(c => c.Topicid == decodeIdTopic);
            if (finalResult == null) {
                return new Response("Không thể nộp file khi chưa có thông báo yêu cầu nộp");
            }

            // validate file here
            // 1. Check if file is null or empty
            if (file == null || file.Length == 0)
                return new Response("Vui lòng chọn file để nộp.");

            // 2. Validate extension
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".zip")
                return new Response("Chỉ chấp nhận file nén có định dạng *.zip.");

            // 3. Validate size ≤ 150MB
            const long maxSize = 150 * 1024 * 1024;
            if (file.Length > maxSize) {
                var fileSizeInMB = (file.Length / (1024.0 * 1024.0)).ToString("0.##");
                return new Response($"Dung lượng file của bạn là {fileSizeInMB}MB, vượt quá 150MB.");
            }

            // 4. Save file temporarily
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ext);
            using (var tempStream = new FileStream(tempPath, FileMode.Create)) {
                await file.CopyToAsync(tempStream);
            }

            // 5. Check allowed file types inside the .zip
            string[] allowedInnerExtensions = { ".pdf", ".ppt", ".pptx", ".xls", ".xlsx", ".mp4", ".zip" };
            if (!MyHelper.ValidateZipContents(tempPath, allowedInnerExtensions)) {
                File.Delete(tempPath);
                return new Response("File nén chứa tệp không hợp lệ. Chỉ cho phép: pdf, ppt(x), xls(x), mp4, zip.");
            }

            // Clean up temp file
            File.Delete(tempPath);

            using (var transaction = _context.Database.BeginTransaction()) {
                try {
                    var isFirstSubmit = string.IsNullOrEmpty(finalResult.Filepath);
                    int notificationId = (int)NotificationType.SUBMIT_FINAL;

                    var filePath = $"D:/UploadedFiles/FinalResult/{topicId}.zip";

                    // Cập nhật final result 
                    finalResult.Filepath = filePath;
                    finalResult.Submitdate = DateTime.Now;
                    _context.Finalresults.Update(finalResult);

                    // cập nhật trạng thái topic sang đã nộp đề tài
                    topic.Status = (int)TopicStatusEnumId.FINAL_SUBMITTED;
                    _context.Topics.Update(topic);

                    // gưi thông báo app đến 2 giáo viên và tổ nghiên cứu khoa học về sự submit
                    await CreateNotification(topic.Createdby, notificationId, topic.Title, "", topicId);
                    if (topic.Secondteacherid != null && topic.Secondteacherid != 0) {
                        await CreateNotification(topic.Secondteacherid??0, notificationId, topic.Title, "", topicId);
                    }
                    var toCNKHs = _context.Accounts.Where(c => c.Departmentid == topic.Departmentid && c.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM).ToList();
                    foreach (var tonckh in toCNKHs) {
                        await CreateNotification(tonckh.Id, notificationId, topic.Title, "", topicId);
                    }
                    await _context.SaveChangesAsync();

                    // lưu file thuyết minh (Lưu trên địa chỉ "D:/UploadedFiles/FinalResult/[encodedTopicId].pdf")
                    using (var stream = new FileStream(filePath, FileMode.Create)) {
                        await file.CopyToAsync(stream);
                    }
                    await transaction.CommitAsync();
                    return new Response(0, "Nộp thuyết minh thành công");
                } catch (Exception e) {
                    await transaction.RollbackAsync();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
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

            int chuTichCount = request.members.Count(m => m.roleid == (int)CommitteeRoleEnumId.FINAL_RESULT_CHU_TICH_HD);
            if (chuTichCount != 1) {
                return new Response("Thành phần hội đồng cần bảo đảm có một và chỉ một Chủ tịch.");
            }

            int thuKyCount = request.members.Count(m => m.roleid == (int)CommitteeRoleEnumId.FINAL_RESULT_THU_KY_HD);
            if (thuKyCount > 2) {
                return new Response("Hội đồng không được quá 2 Thư ký");
            }

            int phanBienCout = request.members.Count(m => m.roleid == (int)CommitteeRoleEnumId.FINAL_RESULT_PHAN_BIEN);
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

            var committeeType = (int)CommitteeTypeEnumId.FINAL_COMMITTEE;
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
                    topic.Status = (int)TopicStatusEnumId.FINAL_REVIEW_ASSIGNMENT;
                    topic.Updateddate = DateTime.Now;
                    _context.Topics.Update(topic);

                    var oldCommittee = _context.Committees.FirstOrDefault(c => c.Topicid == topicId && c.Typeid == (int)CommitteeTypeEnumId.FINAL_COMMITTEE);
                    if (oldCommittee != null) {
                        return new Response("Lỗi, đề tài này đã được phân công hội đồng phê duyệt đề tài");
                    }

                    var committee = new Committee{
                        Topicid = topicId,
                        Typeid = (int)CommitteeTypeEnumId.FINAL_COMMITTEE,
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
                    var emails = new List<string>();
                    foreach (var member in request.members) {
                        await CreateNotification(member.accountid, (int)NotificationType.FINAL_RESULT_ASSIGNMENT, topic.Title, "", _hashids.Encode(topic.Id));
                        var memberAcc = _context.Accounts.FirstOrDefault(c => c.Id == member.accountid);
                        if (memberAcc != null) {
                            emails.Add(memberAcc.Email);
                        }
                    }

                    string emailBody = _emailService.GetEmailTemplate("FinalResultAssignment.html");
                    emailBody = emailBody
                        .Replace("{title}", topic.Title)
                        .Replace("{DepartmentName}", "Tổ Nghiên cứu Khoa học" + " " + topic.Department.Name);

                    _emailService.SendEmailAsync(emails, "[Thông báo][Phân công đánh giá đề tài]", emailBody);

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
                    .FirstOrDefault(c => c.Topicid == topicId && c.Typeid == (int)CommitteeTypeEnumId.FINAL_COMMITTEE);

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
                        await CreateNotification(updated.Accountid, (int)NotificationType.UPDATE_FINAL_RESULT_ASSIGNMENT, topic.Title, "", _hashids.Encode(topic.Id));
                    }

                    foreach (var added in membersToAdd) {
                        await CreateNotification(added.accountid, (int)NotificationType.FINAL_RESULT_ASSIGNMENT, topic.Title, "", _hashids.Encode(topic.Id));
                    }

                    foreach (var removed in membersToRemove) {
                        await CreateNotification(removed.Accountid, (int)NotificationType.UPDATE_MEMBER_FINAL_RESULT_ASSIGNMENT, topic.Title, "", _hashids.Encode(topic.Id));
                    }

                    var keepEmails = membersToKeep.Select(c => c.Account.Email).ToList();
                    var removeEmails = membersToRemove.Select(c => c.Account.Email).ToList();
                    var addIds = membersToAdd.Select(m => m.accountid).ToList();
                    var addAccounts = _context.Accounts.Where(c => addIds.Contains(c.Id)).ToList();
                    var addEmails = addAccounts.Select(c => c.Email).ToList();

                    var keepTemplate = _emailService.GetEmailTemplate("UpdateFinalResultAssignment.html");
                    var removeTemplate = _emailService.GetEmailTemplate("UpdateMemberFinalResultAssignment.html");
                    var addTemplate = _emailService.GetEmailTemplate("FinalResultAssignment.html");

                    var footer = "Tổ Nghiên cứu Khoa học" + " " + topic.Department.Name;
                    _emailService.SendEmailAsync(keepEmails, "[Thông báo][Cập nhật phân công đánh giá đề tài]", keepTemplate.Replace("{title}", topic.Title).Replace("{DepartmentName}", footer));
                    _emailService.SendEmailAsync(removeEmails, "[Thông báo][Cập nhật phân công đánh giá đề tài]", removeTemplate.Replace("{title}", topic.Title).Replace("{DepartmentName}", footer));
                    _emailService.SendEmailAsync(addEmails, "[Thông báo][Phân công đánh giá đề tài]", addTemplate.Replace("{title}", topic.Title).Replace("{DepartmentName}", footer));

                    _context.SaveChanges();
                    transaction.Commit();

                    return new Response(0, "Phân công hội đồng và gửi thông báo đến giảng viên đã được thực hiện thành công");
                } catch (Exception e) {
                    transaction.Rollback();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
        }

        public async Task<List<TopicModelView>> GetDataTableApproval(FinalResultApprovalIndexViewPage options, int userId) {
            var query = BuildDataTableApprovalQuery(options, userId);

            var data = await query
                .Skip(options.Pagination.GetStartRow())
                .Take(options.Pagination.PageLength)
                .Select(c => new TopicModelView(c, _hashids.Encode(c.Id), false))
                .ToListAsync();

            foreach(var item in data) {
                item.Finalresultevaluations = item.Finalresultevaluations
                ?.Where(c => c.Createdby == userId)
                .ToList() ?? new List<Finalresultevaluation>();
            }
            return data;
        }

        public async Task<double> GetCountDataTableApproval(FinalResultApprovalIndexViewPage options, int userId) {
            var query = BuildDataTableApprovalQuery(options, userId);
            var count = await query.LongCountAsync();
            return count;
        }
        private IQueryable<Topic> BuildDataTableApprovalQuery(FinalResultApprovalIndexViewPage options, int senderId) {
            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);
            if (senderAcc == null)
                return Enumerable.Empty<Topic>().AsQueryable();

            var minStatus = (int)TopicStatusEnumId.FINAL_REVIEW_ASSIGNMENT;
            var maxStatus = (int)TopicStatusEnumId.FINAL_APPROVED_BY_FACULTY;
            var query = _context.Topics
                .Include(c => c.Finalresults)
                .ThenInclude(c => c.Finalresultevaluations)
                .Include(c => c.Proposals)
                .Include(c => c.Proposalevaluations)
                .ThenInclude(c => c.Approver)
                .Include(c => c.Committees)
                .ThenInclude(c => c.Committeemembers)
                .AsQueryable();

            if (options.StatusId == -1) {

                //query = query.Where(c => c.Status >= minStatus && c.Status <= maxStatus);
                query = query.Where(c => c.Status >= minStatus);
            } else if (options.StatusId == 0) {
                query = query.Where(c => c.Status > maxStatus);
            } else {
                query = query.Where(c => c.Status == options.StatusId);
            }

            if (senderAcc.Roleid != (int)RoleEnumId.SCIENTIFIC_RESEARCH_OFFICE) {
                var type = (int)CommitteeTypeEnumId.FINAL_COMMITTEE;
                query = query.Where(c =>
                    (options.DepartmentId == -1 || options.DepartmentId == -2 || c.Departmentid == options.DepartmentId)
                    && (c.Committees.Any(com => com.Typeid == type && com.Committeemembers.Any(c => c.Accountid == senderId)))
                );
            } else {
                query = query.Where(c =>
                    (options.DepartmentId == -1 || options.DepartmentId == -2 || c.Departmentid == options.DepartmentId)
                );
            }

            return query;
        }

        public async Task<Response> EvaluateFinalResult(FinalResultEvaluation evaluation, int userId) {
            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == userId);
            if (senderAcc == null) {
                return new Response("Lỗi tài khoản");
            }

            var decodedTopicId = _hashids.Decode(evaluation.TopicId).FirstOrDefault();
            var topic = _context.Topics
                .Include(c => c.Proposals)
                .Include(c => c.Finalresults)
                .Include(c => c.Finalresultevaluations)
                .Include(c => c.Committees)
                .ThenInclude(c => c.Committeemembers)
                .FirstOrDefault(c => c.Id == decodedTopicId);

            if (topic == null) {
                return new Response("Không tìm thấy đề tài");
            }

            var minStatus = (int)TopicStatusEnumId.FINAL_SUBMITTED;
            var maxStatus = (int)TopicStatusEnumId.FINAL_APPROVED_BY_FACULTY;
            if (topic.Status < minStatus || topic.Status > maxStatus) {
                return new Response("Đề tài không đủ điều kiện để đánh giá kết quả cuối cùng");
            }

            var curreEvaluation = _context.Finalresultevaluations.Where(c => c.Topicid == decodedTopicId && c.Createdby == userId);
            var isUpdate = true;

            using (var transaction = _context.Database.BeginTransaction()) {
                try {
                    // nếu đã đánh giá thì cập nhật
                    if (curreEvaluation != null && curreEvaluation.Count() != 0) {
                        isUpdate = false;
                        foreach (var item in evaluation.EvaluationItems) {
                            if (item.IsScore && string.IsNullOrEmpty(item.Value)) {
                                item.Value = "0";
                            }
                            else if(!item.IsScore && string.IsNullOrEmpty(item.Value)) {
                                item.Value = "";
                            }
                            var newEvaluation = curreEvaluation.FirstOrDefault(c => c.Criteriaid == item.Id);
                            if (newEvaluation != null) {
                                newEvaluation.Value = item.Value;
                                _context.Finalresultevaluations.Update(newEvaluation);
                            }
                        }
                    } else {
                        foreach (var item in evaluation.EvaluationItems) {
                            if (item.IsScore && string.IsNullOrEmpty(item.Value)) {
                                item.Value = "";
                            }
                            var newEvaluation = new Finalresultevaluation{
                                Topicid = decodedTopicId,
                                Createdby = userId,
                                Criteriaid = item.Id,
                                Value = item.Value,
                                Finalresultid = topic.Finalresults.First().Id
                            };
                            _context.Finalresultevaluations.Add(newEvaluation);
                        }
                    }
                    _context.SaveChanges();

                    // nếu tất cả thành viên hội đồng đã đánh giá thì cập nhật trạng thái đề tài, và gửi thông báo cho giảng viên và sinh viên
                    var committee = topic.Committees
                        .FirstOrDefault(c => c.Typeid == (int)CommitteeTypeEnumId.FINAL_COMMITTEE);

                    var evaluationMmeberCount = _context.Finalresultevaluations
                        .Where(x => x.Topicid == decodedTopicId)
                        .Select(c => c.Createdby)
                        .Distinct()
                        .Count();

                    if (committee.Committeemembers.Count == evaluationMmeberCount) {
                        topic.Status = (int)TopicStatusEnumId.FINAL_APPROVED_BY_FACULTY;
                        topic.Updateddate = DateTime.Now;

                        var notificationId = (int)NotificationType.FINAL_RESULT_APPROVED;
                        await CreateNotification(topic.Createdby, notificationId, topic.Title, "", _hashids.Encode(topic.Id));
                        await CreateNotification(topic.Studentid??0, notificationId, topic.Title, "", _hashids.Encode(topic.Id));

                        if (topic.Secondteacherid != null) {
                            await CreateNotification(topic.Secondteacherid ?? 0, notificationId, topic.Title, "", _hashids.Encode(topic.Id));
                        }
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                    return new Response(0, "Đánh giá đề tài thành công");
                }
                catch(Exception e) {
                    transaction.Rollback();
                    return new Response("Lỗi hệ thống");
                }
            }
        }

        public async Task<ResponseData<List<EvaluationItem>>> GetEvaluationList(string topicId, int userId) {
            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == userId);
            if (senderAcc == null) {
                return new ResponseData<List<EvaluationItem>>("Lỗi tài khoản");
            }

            var decodedTopicId = _hashids.Decode(topicId).FirstOrDefault();
            var topic = _context.Topics
                .Include(c => c.Proposals)
                .Include(c => c.Finalresults)
                .Include(c => c.Finalresultevaluations)
                .ThenInclude(c => c.Criteria)
                .Include(c => c.Committees)
                .ThenInclude(c => c.Committeemembers)
                .FirstOrDefault(c => c.Id == decodedTopicId);

            if (topic == null) {
                return new ResponseData<List<EvaluationItem>>("Lỗi tài khoản");
            }

            var data = topic.Finalresultevaluations
                .Where(c => c.Createdby == userId)
                .OrderBy(c => c.Criteriaid)
                .Select(c => new EvaluationItem(){
                    Id = c.Criteriaid,
                    Value = c.Value,
                    Content = c.Criteria.Content,
                    IsScore = (c.Criteria.Type),
                })
                .ToList();
            return new ResponseData<List<EvaluationItem>>(0, "OK", data);
        }

        public async Task<Response> NominateTopic(int senderId, string topicId) {

            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);

            if (senderAcc == null) {
                return new Response("Lỗi tài khoản");
            }

            var decodedTopicId = _hashids.Decode(topicId).FirstOrDefault();
            var topic = _context.Topics
                .Include(c => c.Proposals)
                .Include(c => c.Finalresults)
                .Include(c => c.Committees)
                .ThenInclude(c => c.Committeemembers)
                .FirstOrDefault(c => c.Id == decodedTopicId);

            if (topic == null) {
                return new Response("Không tìm thấy đề tài");
            }

            if (topic.Status != (int)TopicStatusEnumId.FINAL_APPROVED_BY_FACULTY) {
                return new Response("Đề tài không đủ điều kiện để đề nghị bảo vệ");
            }

            using(var transaction = _context.Database.BeginTransaction()) {
                try {
                    topic.Status = (int)TopicStatusEnumId.NOMINATED_FOR_UNIVERSITY_DEFENSE;
                    topic.Updateddate = DateTime.Now;
                    var notificationId = (int)NotificationType.NOMINATE_FOR_UNIVERSITY_DEFENSE;

                    await CreateNotification(topic.Createdby, notificationId, topic.Title, "", topicId);
                    await CreateNotification(topic.Studentid ?? 0, notificationId, topic.Title, "", topicId);
                    if (topic.Secondteacherid != null) {
                        await CreateNotification(topic.Secondteacherid ?? 0, notificationId, topic.Title, "", topicId);
                    }

                    var PNCKHs = _context.Accounts.Where(c => c.Roleid == (int)RoleEnumId.SCIENTIFIC_RESEARCH_OFFICE).ToList();
                    foreach (var pnckh in PNCKHs) {
                        await CreateNotification(pnckh.Id, notificationId, topic.Title, "", topicId);
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                    return new Response(0, "Đề xuất đề tài thành công");
                }
                catch(Exception e) {
                    transaction.Rollback();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
            
        }
    }
}
