using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Sec;

namespace EAUT_NCKH.Web.Repositories
{
    public class TopicRepository : ITopicRepository
    {
        private readonly EntityDataContext _context;

        public TopicRepository(EntityDataContext context) {
            _context = context;
        }

        public async Task<double> GetCountDataTable(TopicDataTableOptions options, int userId) {
            var senderAccount = _context.Accounts.FirstOrDefault(a => a.Id == userId);
            if (senderAccount?.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM
                || senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                options.DepartmentId = senderAccount.Departmentid ?? 0;
            }

            var topics = _context.Topics
                .Where(c =>
                    (options.DepartmentId == -1 || c.Departmentid == options.DepartmentId)
                    && (options.Year == -1 || c.Year == options.Year)
                    && (options.Status == -1 || c.Status == options.Status)
                    && (options.SubStatus == "-1" || c.Substatus == options.SubStatus)
                    && (string.IsNullOrEmpty(options.Search) || EF.Functions.FreeText(c.Title, options.Search)));

            if (senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                topics = topics.Where(c => c.Createdby == senderAccount.Id
                || (c.Secondteacher != null && c.Secondteacher.Id == senderAccount.Id));
            }
            var count = await topics.CountAsync();
            return count;
        }

        public async Task<List<Topic>>? GetDataTable(TopicDataTableOptions options, int userId) {
            var senderAccount = _context.Accounts.FirstOrDefault(a => a.Id == userId);
            if (senderAccount?.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM
                || senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                options.DepartmentId = senderAccount.Departmentid ?? 0;
            }

            var topics = _context.Topics
                .Include(t => t.Department)
                .Include(t => t.CreatedbyNavigation)
                .Include(t => t.SubstatusNavigation)
                .Include(t => t.StatusNavigation)
                .Include(t => t.Defenseassignments)
                .Where(c =>
                    (options.DepartmentId == -1 || c.Departmentid == options.DepartmentId)
                    && (options.Year == -1 || c.Year == options.Year)
                    && (options.Status == -1 || c.Status == options.Status)
                    && (options.SubStatus == "-1" || c.Substatus == options.SubStatus)
                    && (string.IsNullOrEmpty(options.Search) || EF.Functions.FreeText(c.Title, options.Search)))
                .OrderByDescending(t => t.Createddate)
                .AsQueryable();
            if (senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                topics = topics.Where(c => c.Createdby == senderAccount.Id 
                || (c.Secondteacher!=null && c.Secondteacher.Id == senderAccount.Id));
            }

            return await topics.Skip(options.GetStartRow())
                .Take(options.PageLength).ToListAsync();
        }

        public async Task<ResponseData> GetSecondTeacherForTopicRegister(int senderId, string accountName) {
            try {
                accountName = accountName.ToLower();
                var senderAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == senderId);
                if (senderAccount == null) {
                    return new ResponseData("Bạn không có quyền chỉnh sửa");
                }

                var secondTacher = _context.Accounts
                .Include(c => c.Students)
                .Where(a => a.Email == accountName || a.Phonenumber == accountName).FirstOrDefault();
                if (secondTacher == null) {
                    return new ResponseData("Không tìm thấy giảng viên");
                }

                if (secondTacher.Id == senderId) {
                    return new ResponseData("Không thể chọn chính bạn làm giảng viên thứ hai");
                }

                if (secondTacher.Roleid != (int)RoleEnumId.RESEARCH_ADVISOR) {
                    return new ResponseData("Người dùng không hợp lệ");
                }

                return new ResponseData(0, "Thành công", secondTacher);
            }
            catch (Exception e) {
                return new ResponseData("Lỗi máy chủ, vui lòng thử lại sau");
            }
        }

        public async Task<ResponseData> GetStudentTopic(int senderId, int studentCode) {
            var sender = await _context.Accounts.Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == senderId);
            var student = await _context.Students.Where(c => c.Id == studentCode)
                .Include(c => c.Topicstudents)
                .ThenInclude(ts => ts.Topic)
                .FirstOrDefaultAsync();
            if (student == null) {
                return new ResponseData(2, "Sinh viên không tồn tại");
            }
            if (sender?.Departmentid != student.Departmentid) {
                return new ResponseData($"Sinh viên không thuộc khoa {sender?.Department?.Name}");
            }
            var currYear = DateTime.Now.Year;
            if (student.Topicstudents?.Any(ts => ts.Topic.Year == currYear) == true) {
                return new ResponseData("Sinh viên không được tham gia hơn 1 đề tài Nghiên cứu Khoa học trong 1 năm");
            }

            var studentView = new Student(){
                Id = student.Id,
                Departmentid = student.Departmentid,
                Majorid = student.Majorid,
                Trainingprogramid = student.Trainingprogramid,
                Classname = student.Classname,
                Fullname = student.Fullname,
                Phonenumber = student.Phonenumber,
                Email = student.Email
            };
            return new ResponseData(0, "OK", studentView);
        }

        // Implement methods for topic management here
    }
}
