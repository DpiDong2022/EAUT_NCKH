using DocumentFormat.OpenXml.Wordprocessing;
using EAUT_NCKH.Web.Controllers;
using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.DTOs.Options;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Repositories.IRepositories;
using HashidsNet;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Sec;
using System.Formats.Asn1;
using System.Reflection.Metadata.Ecma335;

namespace EAUT_NCKH.Web.Repositories
{
    public class TopicRepository : ITopicRepository
    {
        private readonly EntityDataContext _context;
        private readonly IConfiguration _configuration;
        private readonly Hashids _hashids;

        public TopicRepository(EntityDataContext context, IConfiguration configuration) {
            _context = context;
            _configuration = configuration;
            _hashids = new Hashids(_configuration["EncryptKey:key"], _configuration.GetValue<int>("EncryptKey:min"));
        }

        public async Task<Response> Delete(int id, int senderId) {
            var senderAccount = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == senderId);
            var topic = await _context.Topics.Include(c => c.Topicstudents).FirstOrDefaultAsync(t => t.Id == id);
            if (topic == null) {
                return new Response(0, "Ok");
            }
            if (senderAccount == null || senderAccount.Id != topic.Createdby ) {
                return new Response("Bạn không có quyền xóa đề tài này");
            }

            if (topic.Status != (int)TopicStatusEnumId.PENDING_REGISTRATION) {
                return new Response("Đề tài đang trong quá trình xét duyệt/Triển khai, không thể xóa thông tin đề tài");
            }

            if (DateTime.Now.Year != topic.Year) {
                return new Response("Đề tài đã kết thúc, không thể xóa thông tin");
            }

            _context.Topicstudents.RemoveRange(topic.Topicstudents);
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
            return new Response(0, "OK");
        }

        public async Task<double> GetCountDataTable(TopicIndexViewPage options, int userId) {
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
                    && (string.IsNullOrEmpty(options.Search) || EF.Functions.FreeText(c.Title, options.Search)));

            if (senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                topics = topics.Where(c => c.Createdby == senderAccount.Id
                || (c.Secondteacher != null && c.Secondteacher.Id == senderAccount.Id));
            }
            var count = await topics.CountAsync();
            return count;
        }

        public async Task<List<TopicModelView>> GetDataTable(TopicIndexViewPage options, int userId) {
            var senderAccount = await _context.Accounts
                .Include(c => c.Students)
                .FirstOrDefaultAsync(a => a.Id == userId);
            if (senderAccount?.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM
                || senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                options.DepartmentId = senderAccount.Departmentid ?? 0;
            }

            // Start building the query for topics
            var query = _context.Topics
            .Include(t => t.Department)
            .Include(t => t.CreatedbyNavigation)
            .Include(t => t.StatusNavigation)
            .Include(t => t.Defenseassignments)
            .Where(c =>
                (options.DepartmentId == -1 || c.Departmentid == options.DepartmentId)
                && (options.Year == -1 || c.Year == options.Year)
                && (options.Status == -1 || c.Status == options.Status)
                && (string.IsNullOrEmpty(options.Search) || EF.Functions.FreeText(c.Title, options.Search))
            );

            // Filter topics based on the role of the user
            if (senderAccount?.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                query = query.Where(c => c.Createdby == senderAccount.Id || (c.Secondteacher != null && c.Secondteacher.Id == senderAccount.Id));
            }
            if (senderAccount?.Roleid == (int)RoleEnumId.STUDENT) {
                query = query.Where(c => c.Topicstudents.Any(s => s.Studentcode == senderAccount.Students.First().Id ));
            }

            // Apply ordering before applying pagination (Skip/Take)
            query = query.OrderByDescending(t => t.Createddate);
            var dss = options.Pagination.GetStartRow();
            // Execute the query and map to TopicModelView
            var topics = await query.Select(topic => new TopicModelView
            {
                Id = topic.Id,
                Title = topic.Title,
                Departmentid = topic.Departmentid,
                Studentid = topic.Studentid,
                Createdby = topic.Createdby,
                Secondteacherid = topic.Secondteacherid,
                Year = topic.Year,
                Startdate = topic.Startdate,
                Enddate = topic.Enddate,
                Createddate = topic.Createddate,
                Updateddate = topic.Updateddate,
                Status = topic.Status,
                Note = topic.Note,

                // Navigation properties
                CreatedbyNavigation = topic.CreatedbyNavigation,
                Secondteacher = topic.Secondteacher,
                Student = topic.Student,
                Department = topic.Department,
                StatusNavigation = topic.StatusNavigation,
                Defenseassignments = topic.Defenseassignments,
                Finalresults = topic.Finalresults,
                Proposals = topic.Proposals,
                Topicstudents = topic.Topicstudents,

                // Encrypted ID
                EncyptedID = _hashids.Encode(topic.Id),
                Editabled = senderAccount.Id == topic.Createdby && topic.Status == (int)TopicStatusEnumId.PENDING_REGISTRATION
            })
            // Apply Skip and Take for pagination
            .Skip(options.Pagination.GetStartRow())
            .Take(options.Pagination.PageLength)
            .ToListAsync();

            return topics;
        }

        public async Task<ResponseData<Account>> GetSecondTeacherForTopicRegister(int senderId, string accountName) {
            try {
                accountName = accountName.ToLower();
                var senderAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == senderId);
                if (senderAccount == null) {
                    return new ResponseData<Account>("Bạn không có quyền chỉnh sửa");
                }

                var secondTacher = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == accountName || a.Phonenumber == accountName);
                if (secondTacher == null) {
                    return new ResponseData<Account>("Không tìm thấy giảng viên");
                }

                if (secondTacher.Id == senderId) {
                    return new ResponseData<Account>("Không thể chọn chính bạn làm giảng viên thứ hai");
                }

                if (secondTacher.Roleid != (int)RoleEnumId.RESEARCH_ADVISOR) {
                    return new ResponseData<Account>("Người dùng không hợp lệ");
                }

                if (senderAccount.Departmentid != secondTacher.Departmentid) {
                    return new ResponseData<Account>($"Giảng viên này không thuộc {_context.Departments.First(c => c.Id == senderAccount.Departmentid).Name}");
                }

                int currntYear = DateTime.Now.Year;
                int countCreatedBy = _context.Topics.Count(c => c.Year == currntYear && c.Createdby == secondTacher.Id);
                int countSecondTeacher = _context.Topics.Count(c => c.Year == currntYear && c.Secondteacherid == secondTacher.Id);
                if (countCreatedBy + countSecondTeacher >=2 ) {
                    return new ResponseData<Account>($"Giảng viên {secondTacher.Fullname} đã đạt giới hạn số đề tài được đăng ký trong một năm");
                }

                secondTacher.Department = null;

                return await Task.FromResult(new ResponseData<Account>(0, "Thành công", secondTacher));
            }
            catch (Exception e) {
                return new ResponseData<Account>("Lỗi máy chủ, vui lòng thử lại sau");
            }
        }

        public async Task<ResponseData<Student>> GetStudentTopic(int senderId, string studentCode) {
            var sender = await _context.Accounts.Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == senderId);
            var student = await _context.Students.Where(c => c.Id == studentCode)
                .Include(c => c.Topicstudents)
                .ThenInclude(ts => ts.Topic)
                .FirstOrDefaultAsync();
            if (student == null) {
                return new ResponseData<Student>(2, "Sinh viên không tồn tại");
            }
            if (sender?.Departmentid != student.Departmentid) {
                return new ResponseData<Student>($"Sinh viên không thuộc khoa {sender?.Department?.Name}");
            }
            var currYear = DateTime.Now.Year;
            if (student.Topicstudents?.Any(ts => ts.Topic.Year == currYear) == true) {
                return new ResponseData<Student>("Sinh viên không được tham gia hơn 1 đề tài Nghiên cứu Khoa học trong 1 năm");
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
            return new ResponseData<Student>(0, "OK", studentView);
        }

        public async Task<Response> RegisterOrTopic(int senderId, NewTopicDto newTopicDto) {
            if (newTopicDto.Id != "0") {
                return await EditTopic(senderId, newTopicDto);
            }
            return await RegisterTopic(senderId, newTopicDto);
        }

        // Implement methods for topic management here
        public async Task<ResponseData<TopicRegisterDetailDto>> GetTopicRegisteredDetail(int id, int senderId) {
            var topic = await _context.Topics
                .Include(c => c.Secondteacher)
                .Include(c => c.Topicstudents)
                .ThenInclude(ts => ts.StudentcodeNavigation)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (topic == null) {
                return new ResponseData<TopicRegisterDetailDto>("Thông tin không tồn tại");
            }
            
            var topicDto = new TopicRegisterDetailDto{
                EncyptedID = _hashids.Encode(topic.Id),
                Title = topic.Title,
                Note = topic.Note,
                SecondteacherAccount = topic.Secondteacher,
                StudentList = topic.Topicstudents.ToList()
            };


            
            return new ResponseData<TopicRegisterDetailDto>(0, "OK", topicDto);
        }

        public async Task<ResponseData<TopicModelView>> GetTopicAllDetails(string id, int senderId) {

            var senderAcc = _context.Accounts.FirstOrDefault(c => c.Id == senderId);
            if (senderAcc == null) {
                return new ResponseData<TopicModelView>("không có quyền truy cập");
            }
            var decodedID = _hashids.Decode(id).First();


            var topic = await _context.Topics
                .Include(c => c.Department)
                .Include(c => c.Proposalevaluations)
                .ThenInclude(c => c.Status)
                .Include(c => c.CreatedbyNavigation)
                .ThenInclude(c => c.Teachers)
                .ThenInclude(c => c.Academictitle)
                .Include(c => c.Secondteacher)
                .ThenInclude(c => c.Teachers)
                .ThenInclude(c => c.Academictitle)
                .Include(c => c.StatusNavigation)
                .Include(c => c.Topicstudents)
                .ThenInclude(c => c.StudentcodeNavigation)
                .Include(c => c.Proposals)
                .Include(c => c.Committees)
                .ThenInclude(c => c.Building)
                .Include(c => c.Committees)
                .ThenInclude(c => c.Room)
                .Include(c => c.Committees)
                .ThenInclude(c => c.Committeemembers)
                .ThenInclude(c => c.Account)
                .ThenInclude(c => c.Department)
                .Include(c => c.Committees)
                .ThenInclude(c => c.Committeemembers)
                .ThenInclude(c => c.Role)
                .Include(c => c.Finalresults)
                .Include(c => c.Finalresultevaluations)
                .Include(c => c.Defenseassignments)
                .FirstOrDefaultAsync(c => c.Id == decodedID);

            var topicModelView = new TopicModelView(topic, id);

            return new ResponseData<TopicModelView>(0, "", topicModelView);
        }

        private async Task<Response> RegisterTopic(int senderId, NewTopicDto newTopicDto) {
            using (var transaction = await _context.Database.BeginTransactionAsync()) {
                try {
                    // Kiểm tra giáo viên đăng ký và giáo viên thứu hai (số lượng đề tài đã tham gia trong 1 năm)
                    var mainTeacher = await _context.Accounts
             .Include(c => c.TopicCreatedbyNavigations)
             .Include(c => c.TopicSecondteachers)
             .FirstOrDefaultAsync(a => a.Id == senderId);
                    if (mainTeacher == null || mainTeacher.Roleid != (int)RoleEnumId.RESEARCH_ADVISOR) {
                        return new Response("Bạn không có quyền đăng ký đề tài");
                    }

                    var currentYear = DateTime.Now.Year;
                    if ((mainTeacher.TopicSecondteachers.Count(c => c.Year == currentYear)
                    + (mainTeacher.TopicCreatedbyNavigations.Count(c => c.Year == currentYear))) >= 2) {
                        return new Response($"Giảng viên {mainTeacher.Fullname} đã đạt giới hạn số đề tài được đăng ký trong một năm");
                    }

                    Account secondTeacher;
                    bool hasSecondTeacher = false;
                    if (newTopicDto.SecondTeacherId != 0) {
                        hasSecondTeacher = true;
                        secondTeacher = await _context.Accounts
                        .Include(c => c.TopicCreatedbyNavigations)
                        .Include(c => c.TopicSecondteachers)
                        .FirstOrDefaultAsync(a => a.Id == newTopicDto.SecondTeacherId);
                        if (secondTeacher == null || secondTeacher.Roleid != (int)RoleEnumId.RESEARCH_ADVISOR) {
                            return new Response("Bạn không có quyền đăng ký đề tài");
                        }

                        if ((secondTeacher.TopicSecondteachers.Count(c => c.Year == currentYear)
                        + (secondTeacher.TopicCreatedbyNavigations.Count(c => c.Year == currentYear))) >= 2) {
                            return new Response($"Giảng viên {secondTeacher.Fullname} đã đạt giới hạn số đề tài được đăng ký trong một năm");
                        }
                    }

                    var createdTopic = new Topic() {
                        Title = newTopicDto.Title,
                        Note = newTopicDto.Note,
                        //Studentid = newTopicDto.Students.First(c => c.RoleId == 1).Id,
                        Year = currentYear,
                        Createdby = mainTeacher.Id,
                        Secondteacherid = hasSecondTeacher ? newTopicDto.SecondTeacherId : null,
                        Departmentid = mainTeacher.Departmentid??0,
                        Status = (int)TopicStatusEnumId.PENDING_REGISTRATION
                    };
                    _context.Topics.Add(createdTopic);
                    await _context.SaveChangesAsync();
                    // Kiểm tra sinh viên
                    foreach (var studentDto in newTopicDto.Students) {
                        studentDto.Email = studentDto.Email.ToLower();
                        var studentDB = await _context.Students
                 .Include(c => c.Topicstudents)
                 .ThenInclude(ts => ts.Topic)
                 .FirstOrDefaultAsync(c => c.Id == studentDto.StudentCode);

                        if (studentDB == null) {

                            if (_context.Students.Any(c => c.Email == studentDto.Email)) {
                                await transaction.RollbackAsync();
                                return new Response($"Email {studentDto.Email} đã được sử dụng bởi sinh viên khác");
                            }
                            if (_context.Students.Any(c => c.Phonenumber == studentDto.PhoneNumber)) {
                                await transaction.RollbackAsync();
                                return new Response($"Số điện thoại {studentDto.PhoneNumber} đã được sử dụng bởi sinh viên khác");
                            }
                            var topicStudentLink = new Topicstudent() {
                                Topicid = createdTopic.Id,
                                Studentcode = studentDto.StudentCode,
                                Role = studentDto.RoleId == 1
                            };

                            _context.Topicstudents.Add(topicStudentLink);
                            _context.Students.Add(new Student() {
                                Id = studentDto.StudentCode,
                                Fullname = studentDto.FullName,
                                Phonenumber = studentDto.PhoneNumber,
                                Email = studentDto.Email,
                                Classname = studentDto.ClassName,
                                Departmentid = mainTeacher.Departmentid,
                                Majorid = studentDto.MajorId,
                                Trainingprogramid = studentDto.TrainingProgramId
                            });
                        } else {
                            if (studentDB.Topicstudents.Count() == 1) {
                                await transaction.RollbackAsync();
                                return new Response($"Sinh viên {studentDB.Fullname} đã tham gia đề tài khác trong năm {currentYear}");
                            }
                            if (_context.Students.Any(c => c.Email != studentDB.Email && c.Email == studentDto.Email)) {
                                await transaction.RollbackAsync();
                                return new Response($"Email {studentDto.Email} đã được sử dụng bởi sinh viên khác");
                            }
                            if (_context.Students.Any(c => c.Phonenumber != studentDB.Phonenumber && c.Phonenumber == studentDto.PhoneNumber)) {
                                await transaction.RollbackAsync();
                                return new Response($"Số điện thoại {studentDto.PhoneNumber} đã được sử dụng bởi sinh viên khác");
                            }

                            studentDB.Updateddate = DateTime.Now;
                            studentDB.Email = studentDto.Email;
                            studentDB.Phonenumber = studentDto.PhoneNumber;
                            _context.Students.Update(studentDB);

                            var topicStudentLink = new Topicstudent() {
                                Topicid = createdTopic.Id,
                                Studentcode = studentDto.StudentCode,
                                Role = studentDto.RoleId == 1
                            };

                            _context.Topicstudents.Add(topicStudentLink);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new Response(0, "Đăng ký thành công");
                } catch (Exception e) {
                    await transaction.RollbackAsync();
                    return new Response("Lỗi máy chủ, vui lòng thử lại sau");
                }
            }
        }

        private async Task<Response> EditTopic(int senderId, NewTopicDto newTopicDto) {
            newTopicDto.Id = _hashids.Decode(newTopicDto.Id).First().ToString();

            if (!int.TryParse(newTopicDto.Id, out var topicId)) {
                return new Response("Đề tài không hợp lệ");
            }

            var oldTopic = await _context.Topics
                .Include(c => c.Topicstudents)
                .FirstOrDefaultAsync(c => c.Id == int.Parse(newTopicDto.Id));
            if (oldTopic == null) {
                return new Response("Thông tin đề tài không tồn tại");
            }
            var mainTeacher = await _context.Accounts
             .FirstOrDefaultAsync(a => a.Id == senderId);
            if (mainTeacher == null || mainTeacher.Roleid != (int)RoleEnumId.RESEARCH_ADVISOR) {
                return new Response("Bạn không có quyền chỉnh sửa thông tin đăng ký đề tài");
            }

            oldTopic.Title = newTopicDto.Title;
            oldTopic.Note = newTopicDto.Note;
            oldTopic.Updateddate = DateTime.Now;

            using (var transaction = await _context.Database.BeginTransactionAsync()) {
                try {

                    var currentYear = DateTime.Now.Year;

                    Account secondTeacher;
                    bool hasSecondTeacher = false;
                    if (newTopicDto.SecondTeacherId != oldTopic.Secondteacherid && newTopicDto.SecondTeacherId != 0) {
                        secondTeacher = await _context.Accounts
                        .Include(c => c.TopicCreatedbyNavigations)
                        .Include(c => c.TopicSecondteachers)
                        .FirstOrDefaultAsync(a => a.Id == newTopicDto.SecondTeacherId);
                        if (secondTeacher == null || secondTeacher.Roleid != (int)RoleEnumId.RESEARCH_ADVISOR) {
                            return new Response($"Người dùng thứ 2 không tồn tại hoặc không có quyền trong trong vai trò giảng viên");
                        }

                        if ((secondTeacher.TopicSecondteachers.Count(c => c.Year == currentYear)
                        + (secondTeacher.TopicCreatedbyNavigations.Count(c => c.Year == currentYear))) >= 2) {
                            return new Response($"Giảng viên {secondTeacher.Fullname} đã đạt giới hạn số đề tài được đăng ký trong một năm");
                        }
                        hasSecondTeacher = true;
                    }
                    oldTopic.Secondteacherid = hasSecondTeacher ? newTopicDto.SecondTeacherId : oldTopic.Secondteacherid;

                    _context.Topics.Update(oldTopic);

                    // Kiểm tra sinh viên
                    foreach (var studentDto in newTopicDto.Students) {
                        studentDto.Email = studentDto.Email.ToLower();
                        var studentTopic = await _context.Topicstudents
                            .FirstOrDefaultAsync(c => c.Topicid == oldTopic.Id && c.Studentcode == studentDto.StudentCode);
                        
                        if (studentTopic != null) {
                            // update sinh vien db , ok
                            var studentDB = await _context.Students
                                .FirstOrDefaultAsync(c => c.Id == studentDto.StudentCode);
                            if (studentDB.Email != studentDto.Email) {
                                if (await _context.Students.AnyAsync(c => c.Email == studentDto.Email && c.Id != studentDB.Id)) {
                                    await transaction.RollbackAsync();
                                    return new Response($"Email {studentDto.Email} đã được sử dụng bởi sinh viên khác");
                                }
                                studentDB.Email = studentDto.Email;
                            }
                            if (studentDB.Phonenumber != studentDto.PhoneNumber) {
                                if (await _context.Students.AnyAsync(c => c.Phonenumber == studentDto.PhoneNumber && c.Id != studentDB.Id)) {
                                    await transaction.RollbackAsync();
                                    return new Response($"Số điện thoại {studentDto.PhoneNumber} đã được sử dụng bởi sinh viên khác");
                                }
                                studentDB.Phonenumber = studentDto.PhoneNumber;
                            }
                            studentTopic.Role = studentDto.RoleId == 1;
                            _context.Topicstudents.Update(studentTopic);
                            _context.Students.Update(studentDB);
                        }
                        // 1.1. nếu sv chưa có trong db, thì thêm mới
                        // 1.2. nếu sv đã có trong db, thì update lại thông tin sinh viên
                        //   2. cuối cùng tạo liên kết student - topic mới
                        else {
                            var studentDB = await _context.Students
                            .FirstOrDefaultAsync(c => c.Id == studentDto.StudentCode);
                            if (studentDB != null) {
                                // update sinh vien db 
                                if (studentDB.Email != studentDto.Email) {
                                    if (await _context.Students.AnyAsync(c => c.Email == studentDto.Email && c.Id != studentDB.Id)) {
                                        await transaction.RollbackAsync();
                                        return new Response($"Email {studentDto.Email} đã được sử dụng bởi sinh viên khác");
                                    }
                                    studentDB.Email = studentDto.Email;
                                }
                                if (studentDB.Phonenumber != studentDto.PhoneNumber) {
                                    if (await _context.Students.AnyAsync(c => c.Phonenumber == studentDto.PhoneNumber && c.Id != studentDB.Id)) {
                                        await transaction.RollbackAsync();
                                        return new Response($"Số điện thoại {studentDto.PhoneNumber} đã được sử dụng bởi sinh viên khác");
                                    }
                                    studentDB.Phonenumber = studentDto.PhoneNumber;
                                }
                            } else {
                                if (await _context.Students.AnyAsync(c => c.Email == studentDto.Email)) {
                                    await transaction.RollbackAsync();
                                    return new Response($"Email {studentDto.Email} đã được sử dụng bởi sinh viên khác");
                                }
                                if (await _context.Students.AnyAsync(c => c.Phonenumber == studentDto.PhoneNumber)) {
                                    await transaction.RollbackAsync();
                                    return new Response($"Số điện thoại {studentDto.PhoneNumber} đã được sử dụng bởi sinh viên khác");
                                }

                                _context.Students.Add(new Student() {
                                    Id = studentDto.StudentCode,
                                    Fullname = studentDto.FullName,
                                    Phonenumber = studentDto.PhoneNumber,
                                    Email = studentDto.Email,
                                    Classname = studentDto.ClassName,
                                    Departmentid = mainTeacher.Departmentid,
                                    Majorid = studentDto.MajorId,
                                    Trainingprogramid = studentDto.TrainingProgramId
                                });
                            }

                            // 2. tạo liên kết student - topic mới
                            var topicStudentLink = new Topicstudent() {
                                Topicid = oldTopic.Id,
                                Studentcode = studentDto.StudentCode,
                                Role = studentDto.RoleId == 1
                            };
                            _context.Topicstudents.Add(topicStudentLink);
                        }
                    }

                    // Xóa các sinh viên không còn tham gia đề tài
                    foreach (var topicStudent in oldTopic.Topicstudents) {
                        if (!newTopicDto.Students.Any(c => c.StudentCode == topicStudent.Studentcode)) {
                            _context.Topicstudents.Remove(topicStudent);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new Response(0, "Chỉnh sửa thành công");
                } catch (Exception e) {
                    await transaction.RollbackAsync();
                    return new Response("Lỗi máy chủ, vui lòng thử lại sau");
                }
            }
        }
    }
}
