using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Office2019.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Request;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Helpers;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using EAUT_NCKH.Web.Services;
using HashidsNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Crypto.Engines;
using System.Linq;
using System.Linq.Expressions;
using Topic = EAUT_NCKH.Web.Models.Topic;

namespace EAUT_NCKH.Web.Repositories {
    public class ProposalRepository: IProposalRepository {
        private readonly EntityDataContext _context;
        private readonly IConfiguration _configuration;
        private readonly Hashids _hashids;
        private readonly EmailService _emailService;

        public ProposalRepository(EntityDataContext context, IConfiguration configuration, EmailService emailService) {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _hashids = new Hashids(_configuration["EncryptKey:key"], _configuration.GetValue<int>("EncryptKey:min"));
        }

        public Response AddOrEditCommitteeAssignment(int senderId, CommitteeAddRequest request) {
            var topicId = _hashids.Decode(request.topicid).FirstOrDefault();
            var check = GeneralCheckCommitteeAssign(request, topicId);
            if (check.code != 0) {
                return check;
            }
            if (request.id == 0) {
                return AddCommitteeAssignment(request, topicId);
            }
            return EditCommitteeAssignment(request, topicId);
        }

        public ResponseData<byte[]> GetProposalFile(string topicIdEncoded) {
            var topicId = _hashids.Decode(topicIdEncoded).FirstOrDefault();

            // Find the proposal for the given topic ID
            var proposal = _context.Proposals.FirstOrDefault(p => p.Topicid == topicId);
            if (proposal == null || string.IsNullOrEmpty(proposal.Filepath)) {
                return new ResponseData<byte[]>("File thuyết minh không tồn tại");
            }

            // Get the file path for the proposal
            var filePath = proposal.Filepath;

            // Check if the file exists
            if (!System.IO.File.Exists(filePath)) {
                return new ResponseData<byte[]>("Không tìm thấy file");
            }

            // Read the file as bytes and return it as a PDF file response
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            return new ResponseData<byte[]> {
                code = 0,
                message = "OK",
                data = fileBytes
            };
        }

        public async Task<Response> RequireToSubmitProposal(string id, DateTime deadline, int senderId) {
            var senderAcc = await _context.Accounts.FirstOrDefaultAsync(c => c.Id == senderId);
            if (senderAcc == null || senderAcc.Roleid != (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM) {
                return new Response("Bạn không có quyền truy cập");
            }
            var decodedID = _hashids.Decode(id).First();
            var topic = await _context.Topics
                .Include(c => c.Department)
                .Include(c => c.StatusNavigation)
                .Include(c => c.CreatedbyNavigation)
                .ThenInclude(c => c.Teachers)
                .ThenInclude(c => c.Academictitle)
                .Include(c => c.Secondteacher)
                .ThenInclude(c => c.Teachers)
                .ThenInclude(c => c.Academictitle)
                .Include(c => c.StatusNavigation)
                .Include(c => c.Student)
                .Include(c => c.Topicstudents)
                .ThenInclude(c => c.StudentcodeNavigation)
                .FirstOrDefaultAsync(c => c.Id == decodedID);

            if (topic == null) {
                return new Response("Đề tài không tồn tại");
            }

            if (topic.Status > (int)TopicStatusEnumId.WAITING_FOR_PROPOSAL_NOTICE) {
                return new Response($"Trạng thái đề tài đang là \"{topic.StatusNavigation.Description}\", không thể gửi thông báo yêu cầu nộp thuyết minh");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync()) {
                try {
                    var currentProposal = await _context.Proposals
                        .FirstOrDefaultAsync(c => c.Topicid == topic.Id);

                    var vietnameseDatetimeSpel = MyHelper.FormatDateTimeVietnameseString(deadline);
                    if (currentProposal != null) {

                        // chekc deadline
                        var now = DateTime.Now;
                        now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);

                        if (!(deadline >= now.AddDays(1) && deadline <= now.AddMonths(2))) {
                            return new Response("Gửi yêu cầu không thành công, hạn nộp đề tài không hợp lệ");
                        }

                        // update dealine
                        currentProposal.Submitdeadline = deadline;
                        _context.Proposals.Update(currentProposal);
                        await _context.SaveChangesAsync();

                        // Gửi thông báo đến sinh viên và, giảng viên hướng dẫn, cả email và thông báo, nêu dealine mới, tên đề tài
                        // get noti template
                        var updateNotifiTemplate = await _context.Notifications.FirstOrDefaultAsync(c => c.Id == (int)NotificationType.UPDATE_DEADLINE_SUBMIT_PROPOSAL);
                        var updateEmails = new List<string>(){ topic.CreatedbyNavigation.Email, topic.Student.Email };
                        // send noti
                        await CreateProposalNotification(topic.Createdby, (int)NotificationType.UPDATE_DEADLINE_SUBMIT_PROPOSAL, topic.Title, vietnameseDatetimeSpel, id);
                        await CreateProposalNotification(topic.Studentid ?? 0, (int)NotificationType.UPDATE_DEADLINE_SUBMIT_PROPOSAL, topic.Title, vietnameseDatetimeSpel, id);
                        if (topic.Secondteacher != null) {
                            await CreateProposalNotification(topic.Secondteacherid ?? 0, (int)NotificationType.UPDATE_DEADLINE_SUBMIT_PROPOSAL, topic.Title, vietnameseDatetimeSpel, id);
                            updateEmails.Add(topic.Secondteacher.Email);
                        }
                        await _context.SaveChangesAsync();
                        //send email
                        _emailService.SendEmailAsync(
                            updateEmails,
                            $"[Thông báo][{updateNotifiTemplate?.Title}]",
                            $@"<h3>Xin chào Quý thầy/cô và các bạn sinh viên,</h3>
                               <p>Hạn nộp thuyết minh cho đề tài <strong>""{topic.Title}""</strong> đã được cập nhật.</p>
                               <p>Hạn mới: <strong>{vietnameseDatetimeSpel}</strong>.</p>
                               <p>Quý thầy/cô và các bạn vui lòng chuẩn bị và nộp đúng thời hạn.</p>"
                        );

                        //return new Response(0, "Gửi yêu cầu thành công");
                        // committee
                        await transaction.CommitAsync();
                        return new Response(0, "Đã cập nhật hạn nộp thuyết minh và gửi thông báo thành công");
                    }

                    // create account
                    var leaderStudent = topic.Topicstudents.FirstOrDefault(c => c.Role).StudentcodeNavigation;
                    // create account for leader student
                    var account = new Account{
                        Fullname = leaderStudent.Fullname,
                        Email = leaderStudent.Email,
                        Phonenumber = leaderStudent.Phonenumber,
                        Departmentid = leaderStudent.Departmentid,
                        Roleid = (int)RoleEnumId.STUDENT,
                        Password = "12345678"
                    };
                    await _context.Accounts.AddAsync(account);
                    await _context.SaveChangesAsync();

                    // update topic student account to new created account
                    leaderStudent.Accountid = account.Id;
                    _context.Students.Update(leaderStudent);
                    topic.Studentid = account.Id;
                    topic.Status = (int)TopicStatusEnumId.WAITING_FOR_PROPOSAL_NOTICE;
                    _context.Topics.Update(topic);

                    // create proposal
                    var proposal = new Proposal{
                        Topicid = topic.Id,
                        Submitdeadline = deadline
                    };

                    await _context.Proposals.AddAsync(proposal);
                    await _context.SaveChangesAsync();

                    var randomPass = MyHelper.GenerateRandomPassword(8);
                    // Gửi thông báo đến sinh viên và, giảng viên hướng dẫn, cả email và thông báo, nêu dealine, tên đề tài
                    // Gửi email đến giáo viên về yêu cầu nộp thuyết minh,
                    // sinh viên thì sẽ thêm thông tin tài khoản(name, password)
                    // Gửi thông báo trên app: cũng gồm deadline và tên đề tài, thông tin tài khoản đã được đến nhóm trưởng email
                    // thông báo trên app sẽ kèm theo url để naviage vào trang detail topic

                    var toEmails = new List<string>{
                        leaderStudent.Email
                    };
                    // get notifi template
                    var notifiTemplate = await _context.Notifications.FirstOrDefaultAsync(c => c.Id == (int)NotificationType.REQUEST_SUBMIT_PROPOSAL);
                    // send notification
                    await CreateProposalNotification(topic.Createdby, (int)NotificationType.REQUEST_SUBMIT_PROPOSAL, topic.Title, vietnameseDatetimeSpel, id);
                    await CreateProposalNotification(topic.Studentid ?? 0, (int)NotificationType.REQUEST_SUBMIT_PROPOSAL, topic.Title, vietnameseDatetimeSpel, id);
                    if (topic.Secondteacher != null) {
                        await CreateProposalNotification(topic.Secondteacherid ?? 0, (int)NotificationType.REQUEST_SUBMIT_PROPOSAL, topic.Title, vietnameseDatetimeSpel, id);
                        toEmails.Add(topic.Secondteacher.Email);
                    }
                    await _context.SaveChangesAsync();

                    // send email
                    _emailService.SendEmailAsync(
                        toEmails,
                        $"[Thông báo][{notifiTemplate?.Title}]",
                        $@"<h3>Xin chào Quý Thầy/Cô</h3>
                            <p>Đề tài của Thầy/Cô đã được gửi yêu cầu nộp thuyết minh. Hạn nộp thuyết minh là {vietnameseDatetimeSpel}</p>
                            <p>Tên đề tài: <strong>{topic.Title}.</strong></p>");

                    _emailService.SendEmailAsync(
                        new List<string> { leaderStudent.Email },
                        $"[Thông báo][{notifiTemplate?.Title}]",
                        $@"<h3>Xin chào {account.Fullname},</h3>
                            <p>Đề tài của bạn đã được gửi yêu cầu nộp thuyết minh. Hạn nộp là {vietnameseDatetimeSpel}</p>
                            <p>Tên đề tài: <strong>{topic.Title}</strong></p>
                            <p>Tên đăng nhập: {account.Email}</p>
                            <p>Mật khẩu: {randomPass}</p>
                            <p>Vui lòng đổi mật khẩu sau khi đăng nhập.</p>");

                    //return new Response(0, "Gửi yêu cầu thành công");
                    // commit
                    await transaction.CommitAsync();
                    return new Response(0, "Gửi yêu cầu thành công");
                } catch (Exception e) {
                    await transaction.RollbackAsync();
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    return new Response($"Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
        }

        public async Task<Response> SubmitProposal(int senderId, string topicId, IFormFile file, string note) {
            var senderAcc = await _context.Accounts.FirstOrDefaultAsync(c => c.Id == senderId);
            if (senderAcc?.Roleid != (int)RoleEnumId.STUDENT) {
                return new Response("Bạn không có quyền Nộp thuyết minh");
            }

            var decodeIdTopic = _hashids.Decode(topicId).FirstOrDefault();
            var topic = await _context.Topics.Include(c => c.Proposals).FirstOrDefaultAsync(c => c.Id == decodeIdTopic);

            if (topic == null) {
                return new Response("Không tìm thấy Đề tài");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync()) {
                try {
                    var proposal = await _context.Proposals.FirstOrDefaultAsync(c => c.Topicid == decodeIdTopic);
                    if (proposal == null) {
                        transaction.Rollback();
                        return new Response("Không thể nộp file khi chưa có thông báo yêu cầu nộp thuyết minh");
                    }
                    var isFirstSubmit = string.IsNullOrEmpty(proposal.Filepath);
                    int notificationId;
                    if (isFirstSubmit) {
                        notificationId = (int)NotificationType.SUBMIT_PROPOSAL;
                        proposal.Submitdate = DateTime.Now;
                    } else {
                        notificationId = (int)NotificationType.UPDATE_PROPOSAL;
                        proposal.Updateddate = DateTime.Now;
                    }

                    var filePath = $"D:/UploadedFiles/Proposal/{topicId}.pdf";

                    // Cập nhật proposal 
                    proposal.Filepath = filePath;
                    _context.Proposals.Update(proposal);

                    // cập nhật trạng thái topic sang đã nộp đề tài
                    topic.Status = (int)TopicStatusEnumId.PROPOSAL_SUBMITTED;
                    _context.Topics.Update(topic);

                    // gưi thông báo app đến 2 giáo viên và tổ nghiên cứu khoa học về sự submit
                    await CreateProposalNotification(topic.Createdby, notificationId, topic.Title, "", topicId);
                    if (topic.Secondteacherid != null && topic.Secondteacherid != 0) {
                        await CreateProposalNotification(topic.Secondteacherid ?? 0, notificationId, topic.Title, "", topicId);
                    }
                    var toCNKHs = await _context.Accounts.Where(c => c.Departmentid == topic.Departmentid && c.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM).ToListAsync();
                    foreach (var c in toCNKHs) {
                        await CreateProposalNotification(c.Id, notificationId, topic.Title, "", topicId);
                    }

                    // lưu file thuyết minh (Lưu trên địa chỉ "D:/UploadedFiles/Proposal/[thuyetminh_encodedTopicId].pdf")
                    using (var stream = new FileStream(filePath, FileMode.Create)) {
                        await file.CopyToAsync(stream);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new Response(0, "Nộp thuyết minh thành công");
                } catch (Exception e) {
                    await transaction.RollbackAsync();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
        }

        private async Task CreateProposalNotification(int receiverId, int notificationId, string topicName, string deadline, string topicId) {
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

        private Response AddCommitteeAssignment(CommitteeAddRequest request, int topicId) {
            using (var transaction = _context.Database.BeginTransaction()) {
                try {

                    var topic = _context.Topics
                        .Include(c => c.Department)
                        .Include(c => c.Proposals)
                        .FirstOrDefault(c => c.Id == topicId);
                    topic.Status = (int)TopicStatusEnumId.PROPOSAL_REVIEW_ASSIGNMENT;
                    topic.Updateddate = DateTime.Now;
                    topic.Status = (int)TopicStatusEnumId.PROPOSAL_REVIEW_ASSIGNMENT;
                    _context.Topics.Update(topic);

                    var oldCommittee = _context.Committees.FirstOrDefault(c => c.Topicid == topicId && c.Typeid == (int)CommitteeTypeEnumId.PROPOSAL_COMMITTEE);
                    if (oldCommittee != null) {
                        return new Response("Lỗi, đề tài này đã được phân công hội đồng phê duyệt thuyết minh");
                    }

                    var committee = new Committee{
                        Topicid = topicId,
                        Typeid = (int)CommitteeTypeEnumId.PROPOSAL_COMMITTEE,
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
                        CreateNotificationAboutNewCommitteeAssignment(member.accountid, (int)NotificationType.PROPOSAL_ASSIGNMENT, request.topicid, topic.Title);
                        var memberAcc = _context.Accounts.FirstOrDefault(c => c.Id == member.accountid);
                        if (memberAcc != null) {
                            emails.Add(memberAcc.Email);
                        }
                    }

                    string emailBody = _emailService.GetEmailTemplate("ProposalAssignment.html");
                    emailBody = emailBody
                        .Replace("{title}", topic.Title)
                        .Replace("{DepartmentName}", "Tổ Nghiên cứu Khoa học" + " " + topic.Department.Name);

                    _emailService.SendEmailAsync(emails, "[Thông báo][Phân công phê duyệt thuyết minh đề tài]", emailBody);

                    _context.SaveChanges();
                    transaction.Commit();

                    return new Response(0, "Phân công hội đồng và gửi thông báo đến giảng viên đã được thực hiện thành công");
                } catch (Exception e) {
                    transaction.Rollback();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
        }
        private Response EditCommitteeAssignment(CommitteeAddRequest request, int topicId) {
            using (var transaction = _context.Database.BeginTransaction()) {
                try {

                    var topic = _context.Topics
                        .Include(c => c.Department)
                        .Include(c => c.Proposals)
                        .FirstOrDefault(c => c.Id == topicId);

                    var committee = _context.Committees
                    .FirstOrDefault(c => c.Topicid == topicId && c.Typeid == (int)CommitteeTypeEnumId.PROPOSAL_COMMITTEE);
                    committee.Updateddate = DateTime.Now;
                    _context.Committees.Update(committee);

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
                        CreateNotificationAboutNewCommitteeAssignment(updated.Accountid, (int)NotificationType.UPDATE_PROPOSAL_ASSIGNMENT, request.topicid, topic.Title);
                    }

                    foreach (var added in membersToAdd) {
                        CreateNotificationAboutNewCommitteeAssignment(added.accountid, (int)NotificationType.PROPOSAL_ASSIGNMENT, request.topicid, topic.Title);
                    }

                    foreach (var removed in membersToRemove) {
                        CreateNotificationAboutNewCommitteeAssignment(removed.Accountid, (int)NotificationType.UPDATE_MEMBER_PROPOSAL_ASSIGNMENT, request.topicid, topic.Title);
                    }

                    var keepEmails = membersToKeep.Select(c => c.Account.Email).ToList();
                    var removeEmails = membersToRemove.Select(c => c.Account.Email).ToList();
                    var addIds = membersToAdd.Select(m => m.accountid).ToList();
                    var addAccounts = _context.Accounts.Where(c => addIds.Contains(c.Id)).ToList();
                    var addEmails = addAccounts.Select(c => c.Email).ToList();

                    var keepTemplate = _emailService.GetEmailTemplate("UpdateProposalAssignment.html");
                    var removeTemplate = _emailService.GetEmailTemplate("UpdateMemberProposalAssignment.html");
                    var addTemplate = _emailService.GetEmailTemplate("ProposalAssignment.html");

                    var footer = "Tổ Nghiên cứu Khoa học" + " " + topic.Department.Name;
                    _emailService.SendEmailAsync(keepEmails, "[Thông báo][Cập nhật phân công phê duyệt thuyết minh]", keepTemplate.Replace("{title}", topic.Title).Replace("{DepartmentName}", footer));
                    _emailService.SendEmailAsync(removeEmails, "[Thông báo][Cập nhật phân công phê duyệt thuyết minh]", removeTemplate.Replace("{title}", topic.Title).Replace("{DepartmentName}", footer));
                    _emailService.SendEmailAsync(addEmails, "[Thông báo][Phân công phê duyệt thuyết minh]", addTemplate.Replace("{title}", topic.Title).Replace("{DepartmentName}", footer));

                    _context.SaveChanges();
                    transaction.Commit();

                    return new Response(0, "Phân công hội đồng và gửi thông báo đến giảng viên đã được thực hiện thành công");
                } catch (Exception e) {
                    transaction.Rollback();
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

            int chuTichCount = request.members.Count(m => m.roleid == (int)CommitteeRoleEnumId.PROPOSAL_CHU_TICH_HD);
            if (chuTichCount != 1) {
                return new Response("Thành phần hội đồng cần bảo đảm có một và chỉ một Chủ tịch.");
            }

            int thuKyCount = request.members.Count(m => m.roleid == (int)CommitteeRoleEnumId.PROPOSAL_THU_KY_HD);
            if (thuKyCount > 2) {
                return new Response("Hội đồng không được quá 2 Thư ký");
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

            var committee = _context.Committees
                .Include(c => c.Committeemembers)
                .FirstOrDefault(c => c.Topicid == topicId);
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

        private void CreateNotificationAboutNewCommitteeAssignment(int receiverId, int notificationId, string hashTopicId, string topicTitle) {
            var notification = _context.Notifications
                .FirstOrDefault(c => c.Id == notificationId);
            var notificationAccc = new Notificationaccount{
                Receiverid = receiverId,
                Notificationid = notificationId,
                Notificationcontent = notification.Notificationtemplate
                    .Replace("{title}", topicTitle),
                Link = $"{RouterName.GET_TOPIC_DETAIL}?target={hashTopicId}"
            };
            _context.Notificationaccounts.Add(notificationAccc);
        }

        // lấy ra danh sách đề tài đã phân công hội đồng phê duyệt thuyết minh
        public ResponseData<List<TopicModelView>> GetTopicProposalAssignment(int senderId, ProposalAssignedIndexViewPage options) {

            var query = BuildQueryProposalAssignment(options, senderId);

            var startRow = options.Pagination.GetStartRow();
            var data = query
                .Skip(startRow)
                .Take(options.Pagination.PageLength)
                .Select(c => new TopicModelView(c, _hashids.Encode(c.Id), false))
                .ToList();

            return new ResponseData<List<TopicModelView>> {
                code = 0,
                message = "OK",
                data = data
            };
        }

        public double GetTopicProposalAssignmentCount(int senderId, ProposalAssignedIndexViewPage options) {
            
            var query = BuildQueryProposalAssignment(options, senderId);

            double count = query.Count();
            return count;
        }

        private IQueryable<Topic> BuildQueryProposalAssignment(ProposalAssignedIndexViewPage options, int senderId) {

            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);
            if (senderAcc == null)
                return Enumerable.Empty<Topic>().AsQueryable();

            var minStatus = (int)TopicStatusEnumId.PROPOSAL_REVIEW_ASSIGNMENT;
            var maxStatus = (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_UNIVERSITY;
            var query = _context.Topics
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
                var type = (int)CommitteeTypeEnumId.PROPOSAL_COMMITTEE;
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

        public Response ApproveProposal(int senderId, string topicId, int statusId, string feedback) {
            using (var transaction = _context.Database.BeginTransaction()) {
                try {

                    var topicIdDecoded = _hashids.Decode(topicId).First();
                    var topic = _context.Topics.FirstOrDefault(c => c.Id == topicIdDecoded);

                    if (topic == null)
                        return new Response("Không tìm thấy đề tài");

                    var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);
                    if (senderAcc == null)
                        return new Response("Lỗi");

                    Response res;
                    if (topic.Status == (int)TopicStatusEnumId.PROPOSAL_REVIEW_ASSIGNMENT) {
                        res = CommitteeProposalApproval(senderId, topicIdDecoded, statusId, feedback);
                    } else if (topic.Status == (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_FACULTY) {
                        res = ResearchOfficeProposalApproval(senderId, topicIdDecoded, statusId, feedback);
                    } else {
                        res = new Response("Trạng thái đề tài không hợp lệ để phê duyệt");
                    }

                    transaction.Commit();
                    return res;
                }
                catch(Exception e) {
                    transaction.Rollback();
                    return new Response("Lỗi hệ thống, vui lòng thử lại sau");
                }
            }
        }

        public Response CancelProposalApproval(int senderId, string topicId) {
            using (var transaction = _context.Database.BeginTransaction()) {
                try {
                    var topicIdDecoded = _hashids.Decode(topicId).FirstOrDefault();
                    var topic = _context.Topics
                    .Include(c => c.Secondteacher)
                    .Include(c => c.CreatedbyNavigation)
                    .Include(c => c.Student)
                    .Include(c => c.Committees)
                    .ThenInclude(d => d.Committeemembers)
                    .FirstOrDefault(c => c.Id == topicIdDecoded);

                    if (topic == null)
                        return new Response("Không tìm thấy đề tài");

                    var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);
                    if (senderAcc == null)
                        return new Response("Lỗi tài khoản");

                    var isResearchOffice = senderAcc.Roleid == (int)RoleEnumId.SCIENTIFIC_RESEARCH_OFFICE;
                    var isCommittee = topic.Committees.Any(c => c.Committeemembers.Any(cm => cm.Accountid == senderId));

                    if (!isResearchOffice && !isCommittee)
                        return new Response("Bạn không có quyền hủy phê duyệt thuyết minh của đề tài này");

                    var emailTemplate = _emailService.GetEmailTemplate("CancelProposalApproval.html");
                    // Gửi thông báo và xóa bản ghi đánh giá
                    if (isResearchOffice) {
                        var proposal = _context.Proposalevaluations.FirstOrDefault(p => p.Topicid == topic.Id && p.Type == (int)ProposalEvaluationTypeId.SR_EVALUATION);
                        if (proposal != null)
                            _context.Proposalevaluations.Remove(proposal);

                        topic.Status = (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_FACULTY;

                    } else if (isCommittee) {
                        var proposal = _context.Proposalevaluations.FirstOrDefault(p => p.Topicid == topic.Id && p.Type == (int)ProposalEvaluationTypeId.COMMITTEE_EVALUATION);
                        if (proposal != null)
                            _context.Proposalevaluations.Remove(proposal);

                        topic.Status = (int)TopicStatusEnumId.PROPOSAL_REVIEW_ASSIGNMENT;
                    }

                    CreateNotificationAboutNewCommitteeAssignment(topic.Createdby, (int)NotificationType.CANCEL_APPROVAL_PROPOSAL, topicId, topic.Title);
                    CreateNotificationAboutNewCommitteeAssignment(topic.Studentid ?? 0, (int)NotificationType.CANCEL_APPROVAL_PROPOSAL, topicId, topic.Title);
                    if (topic.Secondteacherid != null)
                        CreateNotificationAboutNewCommitteeAssignment(topic.Secondteacherid.Value, (int)NotificationType.CANCEL_APPROVAL_PROPOSAL, topicId, topic.Title);

                    _emailService.SendEmailAsync(
                        new[] { topic?.Student?.Email, topic?.CreatedbyNavigation.Email, topic?.Secondteacher?.Email }
                            .Where(e => !string.IsNullOrEmpty(e)).ToList(),
                        "[Thông báo] Hủy phê duyệt thuyết minh đề tài",
                        emailTemplate.Replace("{title}", topic.Title)
                    );

                    _context.Update(topic);
                    _context.SaveChanges();
                    transaction.Commit();

                    return new Response(0, "Hủy phê duyệt thuyết minh thành công");
                } catch (Exception e) {
                    transaction.Rollback();
                    return new Response("Đã xảy ra lỗi hệ thống khi hủy phê duyệt");
                }
            }
        }


        private Response CommitteeProposalApproval(int senderId, int topicId, int statusId, string feedback) {
            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);
            var topic = _context.Topics
                .Include(c => c.Secondteacher)
                .Include(c => c.CreatedbyNavigation)
                .Include(c => c.Student)
                .Include(c => c.Proposals)
                .Include(c => c.Committees)
                .ThenInclude(d => d.Committeemembers)
                .FirstOrDefault(c => c.Id == topicId);

            var userIsInCommittee = topic.Committees
                .Any(c => c.Committeemembers.Any(cm => cm.Accountid == senderId));

            if (!userIsInCommittee)
                return new Response("Bạn không có quyền phê duyệt thuyết minh của đề tài này");

            var proposalEvaluation = _context.Proposalevaluations.FirstOrDefault(c => c.Topicid == topic.Id && c.Type == (int)ProposalEvaluationTypeId.COMMITTEE_EVALUATION);
            if (proposalEvaluation != null) {
                _context.Proposalevaluations.Remove(proposalEvaluation);
            }

            var newProposalEvaluation = new Proposalevaluation{
                Topicid = topic.Id,
                Type = (int)ProposalEvaluationTypeId.COMMITTEE_EVALUATION,
                Statusid = statusId,
                Feedback = feedback,
                Approverid = senderId,
                Proposalid = topic.Proposals.First().Id
            };
            _context.Proposalevaluations.Add(newProposalEvaluation);

            topic.Status = (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_FACULTY;
            _context.Update(topic);

            var emails = new List<string>();

            // gửi thông báo cho sinh viên, giảng viên
            var hashTopicId = _hashids.Encode(topic.Id);
            var notiId = (int)NotificationType.APPROVE_PROPOSAL_BY_COMMITTEE;
            CreateNotificationAboutNewCommitteeAssignment(topic.Createdby, notiId, hashTopicId, topic.Title);
            CreateNotificationAboutNewCommitteeAssignment(topic.Studentid??0, notiId, hashTopicId, topic.Title);
            if (topic.Secondteacher != null) {

                CreateNotificationAboutNewCommitteeAssignment(topic.Secondteacherid ?? 0, notiId, hashTopicId, topic.Title);
                emails.Add(topic.Secondteacher.Email);
            }

            emails.Add(topic.CreatedbyNavigation.Email);
            emails.Add(topic.Student.Email);

            var template = _emailService.GetEmailTemplate("ProposalApprovalCommittee.html");
            _emailService.SendEmailAsync(emails, "[Thông báo][Phê duyệt thuyết minh đề tài]", template.Replace("{title}", topic.Title));

            _context.SaveChanges();
            return new Response(0, "Phê duyệt thuyết minh cấp Khoa thành công");
        }

        private Response ResearchOfficeProposalApproval(int senderId, int topicId, int statusId, string feedback) {
            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);
            var topic = _context.Topics
                .Include(c => c.Secondteacher)
                .Include(c => c.CreatedbyNavigation)
                .Include(c => c.Student)
                .Include(c => c.Proposals)
                .Include(c => c.Proposalevaluations)
                .Include(c => c.Committees)
                .ThenInclude(d => d.Committeemembers)
                .FirstOrDefault(c => c.Id == topicId);

            var userIsPNCKH = senderAcc.Roleid == (int)RoleEnumId.SCIENTIFIC_RESEARCH_OFFICE;

            if (!userIsPNCKH)
                return new Response("Bạn không có quyền phê duyệt thuyết minh của đề tài này");

            if (topic?.Proposalevaluations?.FirstOrDefault()?.Statusid == (int)ProposalStatusId.REJECTED) {
                return new Response("Thuyết minh đề tài đã bị từ chối ở cấp khoa");
            }

            var proposalEvaluation = _context.Proposalevaluations.FirstOrDefault(c => c.Topicid == topic.Id && c.Type == (int)ProposalEvaluationTypeId.SR_EVALUATION);
            if (proposalEvaluation != null) {
                _context.Proposalevaluations.Remove(proposalEvaluation);
            }

            var newProposalEvaluation = new Proposalevaluation{
                Topicid = topic.Id,
                Type = (int)ProposalEvaluationTypeId.SR_EVALUATION,
                Statusid = statusId,
                Feedback = feedback,
                Approverid = senderId,
                Proposalid = topic.Proposals.First().Id
            };
            _context.Proposalevaluations.Add(newProposalEvaluation);

            topic.Status = (int)TopicStatusEnumId.PROPOSAL_APPROVED_BY_UNIVERSITY;
            topic.Startdate = DateTime.Now;
            _context.Update(topic);

            var emails = new List<string>();

            // gửi thông báo cho sinh viên, giảng viên
            var hashTopicId = _hashids.Encode(topic.Id);
            var notiId = (int)NotificationType.APPROVE_PROPOSAL_BY_RESEARCH_OFFICE;
            CreateNotificationAboutNewCommitteeAssignment(topic.Createdby, notiId, hashTopicId, topic.Title);
            CreateNotificationAboutNewCommitteeAssignment(topic.Studentid ?? 0, notiId, hashTopicId, topic.Title);
            if (topic.Secondteacher != null) {

                CreateNotificationAboutNewCommitteeAssignment(topic.Secondteacherid ?? 0, notiId, hashTopicId, topic.Title);
                emails.Add(topic.Secondteacher.Email);
            }

            emails.Add(topic.CreatedbyNavigation.Email);
            emails.Add(topic.Student.Email);

            var template = _emailService.GetEmailTemplate("ProposalApproval.html");
            _emailService.SendEmailAsync(emails, "[Thông báo][Phê duyệt thuyết minh đề tài]", template.Replace("{title}", topic.Title));

            _context.SaveChanges();
            return new Response(0, "Phê duyệt thuyết minh thành công");
        }

       
    }
}
