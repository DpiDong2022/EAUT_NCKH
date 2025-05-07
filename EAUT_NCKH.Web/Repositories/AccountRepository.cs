using EAUT_NCKH.Web.Data;
using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Enums;
using EAUT_NCKH.Web.IRepositories;
using EAUT_NCKH.Web.Models;
using EAUT_NCKH.Web.Services;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Utils;
using System.Text.RegularExpressions;

namespace EAUT_NCKH.Web.Repositories
{
    public class AccountRepository: IAccountRepository {

        private readonly EntityDataContext _eautNckhContext;
        private readonly IConfiguration _configuration;
        private readonly LogService<AccountRepository> _logService;
        private const string regularExCheckPass = @"^(?=.*[0-9])(?=.*[!@#$%^&*])[a-zA-Z0-9!@#$%^&*]{6,16}$";
        public AccountRepository(EntityDataContext eautNckhContext, LogService<AccountRepository> logService, IConfiguration configuration) {
            _eautNckhContext = eautNckhContext;
            _logService = logService;
            _configuration = configuration;
        }

        public async Task<List<Account>>? GetAll(AccountRequestOptions option) {
            var dssdsd = _eautNckhContext.Accounts.ToList();
            var list = await _eautNckhContext.Accounts.Where(c => c.Email == option.AccountName || c.Phonenumber == option.AccountName).Include(c => c.Role).ToListAsync();
            return list;
        }

        public async Task<List<Account>>? GetDataTable(AccountIndexViewPage options, int userId) {

            options.Search = options.Search.ToLower();
            var senderAccount = _eautNckhContext.Accounts.FirstOrDefault(a => a.Id == userId);
            if (senderAccount?.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM) {
                options.DepartmentId = senderAccount.Departmentid ?? 0;
            }

            return await _eautNckhContext.Accounts
                .Include(i => i.Role)
                .Include(k => k.Department)
                .Where(a =>
                (options.RoleId == 0 || a.Roleid == options.RoleId)
                && (options.DepartmentId == 0 || a.Departmentid == options.DepartmentId)
                && (options.Search == ""
                || EF.Functions.FreeText(a.Fullname, options.Search)
                || EF.Functions.FreeText(a.Email, options.Search)
                || EF.Functions.FreeText(a.Phonenumber, options.Search))
            ).OrderByDescending(c => c.Createddate)
            .Skip(options.Pagination.GetStartRow())
            .Take(options.Pagination.PageLength)
            .ToListAsync();
        }
        public async Task<double> GetCountDataTable(AccountIndexViewPage options, int userId) {
            var senderAccount = _eautNckhContext.Accounts.FirstOrDefault(a => a.Id == userId);
            if (senderAccount?.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM) {
                options.DepartmentId = senderAccount.Departmentid ?? 0;
            }
            options.Search = options.Search.ToLower();
            return await _eautNckhContext.Accounts.LongCountAsync(a =>
                (options.RoleId == 0 || a.Roleid == options.RoleId)
                && (options.DepartmentId == 0 || a.Departmentid == options.DepartmentId)
                && (options.Search == ""
                || EF.Functions.FreeText(a.Fullname, options.Search)
                || EF.Functions.FreeText(a.Email, options.Search)
                || EF.Functions.FreeText(a.Phonenumber, options.Search))
                );
        }

        public async Task<Response> ChangePassword(int userId, string currentPass, string newPass, string confirmNewPass) {

            try {
                var account = _eautNckhContext.Accounts.First(a => a.Id == userId);
                if (account == null) {
                    return new Response {
                        code = 1,
                        message = "Tài khoản không tồn tại!"
                    };
                }

                var checkResponse = CheckPassword(account.Password, currentPass, newPass, confirmNewPass);
                if (checkResponse.code != 0) {
                    return checkResponse;
                }
                // Check constrain new password
                account.Password = newPass;
                _eautNckhContext.Accounts.Update(account);
                await _eautNckhContext.SaveChangesAsync();
                return new Response {
                    code = 0,
                    message = "Đổi mật khẩu thành công"
                };
            } catch (Exception e) {
                _logService.LogError(e);
                return new Response {
                    code = 1,
                    message = $"Lỗi hệ thống, vui lòng thử lại sau: {e?.InnerException?.Message}!"
                };
            }
        }

        public Response CheckPassword(string realPass, string currentPassInput, string newPass, string confirmNewPass) {
            if (realPass != currentPassInput) {
                return new Response {
                    code = 1,
                    message = "Mật khẩu hiện tại không trùng khớp!"
                };
            }

            if (currentPassInput == newPass) {
                return new Response {
                    code = 1,
                    message = "Mật khẩu mới và mật khẩu cũ không được trùng nhau!"
                };
            }

            if (newPass.Length < 8) {
                return new Response {
                    code = 1,
                    message = "Mật khẩu mới phải có ít nhất 8 kí tự!"
                };
            }

            // for debug, edit later
            if (Regex.IsMatch(newPass, regularExCheckPass)) {
                return new Response {
                    code = 1,
                    message = "Mật khẩu mới chưa đủ mạnh!"
                };
            }

            if (newPass != confirmNewPass) {
                return new Response {
                    code = 1,
                    message = "Mật khẩu mới và mật khẩu xác nhận không trùng khớp!"
                };
            }

            return new Response {
                code = 0
            };
        }

        public async Task<ResponseData<int>> AddOrEditToNCKH(GeneralInformationAccount viewModel) {
            Account account;
            viewModel.email = viewModel.email.ToLower();
            if (viewModel.id > 0) {
                account = await _eautNckhContext.Accounts.FirstOrDefaultAsync(a => a.Id == viewModel.id);
                if (account == null || account.Id == 0) {
                    return new ResponseData<int> {
                        code = 1,
                        message = "Tài khoản không tồn tại"
                    };
                }
                if (account.Email != viewModel.email && _eautNckhContext.Accounts.Any(c => c.Email == viewModel.email)) {
                    return new ResponseData<int> {
                        code = 1,
                        message = "Email đã được đăng ký"
                    };
                }
                account.Fullname = viewModel.fullName;
                account.Email = viewModel.email;
                _eautNckhContext.Accounts.Update(account);
                _eautNckhContext.SaveChanges();
                return new ResponseData<int> {
                    code = 0,
                    message = "Cập nhật tài khoản thành công"
                };
            } else {
                account = null;
                account = await _eautNckhContext.Accounts.FirstOrDefaultAsync(a => a.Phonenumber == viewModel.phoneNumber);
                if (account != null) {
                    return new ResponseData<int> {
                        code = 1,
                        message = "Số điện thoại đã được đăng ký"
                    };
                }
                account = await _eautNckhContext.Accounts.FirstOrDefaultAsync(a => a.Email == viewModel.email);
                if (account != null) {
                    return new ResponseData<int> {
                        code = 1,
                        message = "Email đã được đăng ký"
                    };
                }
                account = await _eautNckhContext.Accounts.FirstOrDefaultAsync(c => c.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM && c.Departmentid == viewModel.departmentId);
                if (account != null) {
                    return new ResponseData<int>() {
                        code = 1,
                        message = $"Khoa / Viện hiện tại đã có tài khoản của Tổ Nghiên cứu Khoa học, số điện thoại của tài khoản là: {account.Phonenumber}"
                    };
                }
                try {
                    using (var transaction = _eautNckhContext.Database.BeginTransaction()) {
                        var newAcc = new Account() {
                            Fullname = viewModel.fullName,
                            Email = viewModel.email,
                            Phonenumber = viewModel.phoneNumber ?? "",
                            Roleid = (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM,
                            Departmentid = viewModel.departmentId,
                        };
                        _eautNckhContext.Accounts.Add(newAcc);
                        _eautNckhContext.SaveChanges();
                        transaction.Commit();

                        return new ResponseData<int> {
                            code = 0,
                            message = "Thêm tài khoản thành công",
                            data = newAcc.Id
                        };
                    }
                } catch (Exception e) {
                    await _eautNckhContext.Database.RollbackTransactionAsync();
                    _logService.LogError(e);
                    return new ResponseData<int> {
                        code = 1,
                        message = "Lỗi hệ thống, vui lòng liên hệ với Phòng Nghiên cứu Khoa học"
                    };
                }

            }
        }

        public async Task<ResponseData<int>> AddOrEditTeacher(ResearchAdvisorViewModel viewModel, int createrId) {
            viewModel.email = viewModel?.email?.ToLower();
            // kiem tra giang vien da ton tai hay chua, so dien thoai, email,
            if (viewModel?.id > 0) {
                return await UpdateTeacher(viewModel);
            }
            return await AddTeacher(viewModel, createrId);
        }

        private async Task<ResponseData<int>> AddTeacher(ResearchAdvisorViewModel viewModel, int createrId) {
            Account creater = await _eautNckhContext.Accounts.FirstOrDefaultAsync(c => c.Id == createrId);
            if (creater == null) {
                return new ResponseData<int>() {
                    code = 1,
                    message = "Bạn không có quyền thêm tài khoản"
                };
            }
            if (creater.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM && creater.Departmentid != viewModel.departmentId) {
                return new ResponseData<int>() {
                    code = 1,
                    message= "Bạn không có quyền thêm tài khoản của khoa / viện khác"
                };
            }

            Account currentAccount;
            currentAccount = await _eautNckhContext.Accounts.FirstOrDefaultAsync(a => a.Phonenumber == viewModel.phoneNumber);
            if (currentAccount != null) {
                return new ResponseData<int> {
                    code = 1,
                    message = "Số điện thoại đã được đăng ký"
                };
            }
            currentAccount = await _eautNckhContext.Accounts.FirstOrDefaultAsync(a => a.Email == viewModel.email);
            if (currentAccount != null) {
                return new ResponseData<int> {
                    code = 1,
                    message = "Email đã được đăng ký"
                };
            }

            try {
                using (var transaction = _eautNckhContext.Database.BeginTransaction()) {
                    var account = new Account(){
                        Fullname = viewModel.fullName,
                        Email = viewModel.email ?? "",
                        Phonenumber = viewModel.phoneNumber ?? "",
                        Roleid = viewModel.roleId,
                        Departmentid = viewModel.departmentId,
                    };
                    _eautNckhContext.Accounts.Add(account);
                    _eautNckhContext.SaveChanges();

                    if (account.Id == 0) {
                        _eautNckhContext.Database.RollbackTransaction();
                        throw new InvalidOperationException();
                    }
                    var teacher = new Teacher() {
                        Accountid = account.Id,
                        Academictitleid = viewModel.departmentId ?? 0,
                        Majorid = viewModel.majorId??0,
                    };
                    _eautNckhContext.Teachers.Add(teacher);
                    _eautNckhContext.SaveChanges();
                    transaction.Commit();

                    return new ResponseData<int> {
                        code = 0,
                        message = "Thêm tài khoản thành công",
                        data = account.Id
                    };
                };
            } catch (Exception e) {
                _eautNckhContext.Database.RollbackTransaction();
                _logService.LogError(e);
                return new ResponseData<int> {
                    code = 1,
                    message = "Lỗi hệ thống, vui lòng liên hệ với Phòng Nghiên cứu Khoa học"
                };
            }
        }

        private async Task<ResponseData<int>> UpdateTeacher(ResearchAdvisorViewModel viewModel) {
            Account currentAccount;
            currentAccount = await _eautNckhContext.Accounts.FirstOrDefaultAsync(a => a.Id == viewModel.id);
            if (currentAccount == null) {
                return new ResponseData<int> {
                    code = 1,
                    message = "Tài khoản không tồn tại"
                };
            }

            if (currentAccount.Email != viewModel.email && _eautNckhContext.Accounts.Any(c => c.Email == viewModel.email)) {
                return new ResponseData<int> {
                    code = 1,
                    message = "Email đã được đăng ký"
                };
            }
            currentAccount.Fullname = viewModel.fullName;
            currentAccount.Email = viewModel.email??"";
            _eautNckhContext.Accounts.Update(currentAccount);

            var teacher = _eautNckhContext.Teachers.FirstOrDefault(t => t.Accountid == currentAccount.Id);
            if (teacher == null) {
                teacher = new Teacher() {
                    Accountid = currentAccount.Id,
                    Academictitleid = viewModel.academicTitleId ?? 0,
                    Majorid = _eautNckhContext.Majors.First(c => c.Departmentid == currentAccount.Departmentid).Id
                };
                _eautNckhContext.Teachers.Add(teacher);
            } else {
                teacher.Academictitleid = viewModel.academicTitleId ?? 0;
                _eautNckhContext.Teachers.Update(teacher);
            }
            _eautNckhContext.SaveChanges();
            return new ResponseData<int> {
                code = 0,
                message = "Cập nhật tài khoản thành công"
            };
        }

        public async Task<ResponseData<int>> AddOrEditStudent(StudentViewModel viewModel, int createrId) {
            Account creater = await _eautNckhContext.Accounts.FirstOrDefaultAsync(c => c.Id == createrId);
            if (creater == null) {
                return new ResponseData<int>() {
                    code = 1,
                    message = "Bạn không có quyền thêm tài khoản"
                };
            }
            if (creater.Roleid == (int)RoleEnumId.DEPARTMENT_SCIENTIFIC_RESEARCH_TEAM && creater.Departmentid != viewModel.departmentId) {
                return new ResponseData<int>() {
                    code = 1,
                    message = "Bạn không có quyền thêm tài khoản của khoa / viện khác"
                };
            }

            Account currentAccount;
            currentAccount = await _eautNckhContext.Accounts.FirstOrDefaultAsync(a => a.Phonenumber == viewModel.phoneNumber);
            if (currentAccount != null) {
                return new ResponseData<int> {
                    code = 1,
                    message = "Số điện thoại đã được đăng ký"
                };
            }
            currentAccount = await _eautNckhContext.Accounts.FirstOrDefaultAsync(a => a.Email == viewModel.email);
            if (currentAccount != null) {
                return new ResponseData<int>  {
                    code = 1,
                    message = "Email đã được đăng ký"
                };
            }

            try {
                using (var transaction = _eautNckhContext.Database.BeginTransaction()) {
                    var account = new Account(){
                        Fullname = viewModel.fullName,
                        Email = viewModel.email??"",
                        Phonenumber = viewModel.phoneNumber??"",
                        Roleid = viewModel.roleId,
                        Departmentid = viewModel.departmentId,
                    };
                    _eautNckhContext.Accounts.Add(account);
                    await _eautNckhContext.SaveChangesAsync();

                    if (account.Id == 0) {
                        _eautNckhContext.Database.RollbackTransaction();
                        throw new InvalidOperationException();
                    }

                    var student = await _eautNckhContext.Students.FirstOrDefaultAsync(c => c.Id == viewModel.studentCode);
                    if (student != null && student.Accountid<1) {
                        student.Classname = viewModel.className??"";
                        student.Majorid = viewModel.majorId ?? 0;
                        student.Trainingprogramid = viewModel.trainingProgram ?? 0;
                        student.Accountid = account.Id;
                        _eautNckhContext.Students.Update(student);
                    } 
                    else if (student != null && student.Accountid >0) {
                        _eautNckhContext.Database.RollbackTransaction();
                        var existedAccount = await _eautNckhContext.Accounts.FirstOrDefaultAsync(c => c.Id == student.Accountid);
                        return new ResponseData<int>() { 
                            code = 1,
                            message = $"Sinh viên đã có tài khoản, số điện thoại là {existedAccount?.Phonenumber}",
                        };
                    }
                    else {
                        var newStudent = new Student() {
                            Accountid = account.Id,
                            Id = viewModel.studentCode,
                            Classname = viewModel.className ?? "",
                            Majorid = viewModel.majorId??0,
                            Trainingprogramid = viewModel.trainingProgram ?? 0,
                        };
                        _eautNckhContext.Students.Add(newStudent);
                    }
                    await _eautNckhContext.SaveChangesAsync();
                    transaction.Commit();

                    return new ResponseData<int> {
                        code = 0,
                        message = "Thêm tài khoản thành công",
                        data = account.Id
                    };
                };
            } catch (Exception e) {
                _eautNckhContext.Database.RollbackTransaction();
                _logService.LogError(e);
                return new ResponseData<int> {
                    code = 1,
                    message = "Lỗi hệ thống, vui lòng liên hệ với Phòng Nghiên cứu Khoa học"
                };
            }
        }

        public async Task<ResponseData<object>> GetAccountInformation(int id) {
            var account = await _eautNckhContext.Accounts.FirstOrDefaultAsync(c => c.Id == id);
            if (account == null) {
                return new ResponseData<object>() {
                    code = 1,
                    message = "Tài khoản không tồn tại"
                };
            }
            GeneralInformationAccount viewModel;
            if (account.Roleid == (int)RoleEnumId.RESEARCH_ADVISOR) {
                var teacher = await _eautNckhContext.Teachers.FirstOrDefaultAsync(c => c.Accountid == id);
                viewModel = new ResearchAdvisorViewModel() {
                    id = account.Id,
                    fullName = account.Fullname,
                    email = account.Email,
                    phoneNumber = account.Phonenumber,
                    academicTitleId = teacher?.Academictitleid ?? 0,
                    majorId = teacher?.Majorid == 0 ? -1 : teacher?.Majorid,
                    departmentId = account.Departmentid,
                    roleId = account.Roleid
                };
            }
            else if (account.Roleid == (int)RoleEnumId.STUDENT) {
                var student = await _eautNckhContext.Students.FirstOrDefaultAsync(c => c.Accountid == id);
                viewModel = new StudentViewModel() {
                    id = account.Id,
                    fullName = account.Fullname,
                    email = account.Email,
                    phoneNumber = account.Phonenumber,
                    studentCode = student?.Id,
                    className = student?.Classname??"",
                    majorId = student?.Majorid == 0 ? -1 : student?.Majorid,
                    trainingProgram = student?.Trainingprogramid == 0 ? -1 : student?.Trainingprogramid,
                    departmentId = account.Departmentid,
                    roleId = account.Roleid
                };

            }
            else {
                viewModel = new GeneralInformationAccount() {
                    id = account.Id,
                    fullName = account.Fullname,
                    email = account.Email,
                    phoneNumber = account.Phonenumber,
                    departmentId = account.Departmentid == 0 ? -1 : account.Departmentid,
                    roleId = account.Roleid
                };
            }
            return new ResponseData<object>() {
                code = 0,
                data = viewModel
            };
        }

        public async Task<Account?> GetAccountByEmail(string email) {
            return await _eautNckhContext.Accounts.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<Response> CreateOTP(string email) {
            email = email.ToLower();
            var acccount = await _eautNckhContext.Accounts.FirstOrDefaultAsync(c => c.Email == email);
            if (acccount == null) {
                return new Response {
                    code = 1,
                    message = "Tài khoản không tồn tại"
                };
            }
            email = "phungdaidong1114@gmail.com";
            var otp = new Random().Next(100000, 999999).ToString();
            acccount.Otp = otp;
            acccount.Otpgeneratedat = DateTime.Now;
            _eautNckhContext.SaveChanges();

             var displayName = "EAUT-NCKH";
             var sendEmail =  "phungdaidong1114@gmail.com";
            // send email
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(displayName, sendEmail));
            message.To.Add(MailboxAddress.Parse(sendEmail));
            message.Subject = "Mã OTP khôi phục mật khẩu";

            var bodyBuilder = new BodyBuilder();

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "EAUT_LOGO.png");
            var image = bodyBuilder.LinkedResources.Add(logoPath);
            image.ContentId = MimeUtils.GenerateMessageId();

            bodyBuilder.HtmlBody = $@"
                <div style='text-align:center'>
                    <img src='cid:{image.ContentId}' alt='EAUT-NCKH Logo' style='max-height:80px' />
                </div>
                <p>Xin chào {email}</p>
                <p>Bạn hoặc ai đó đã yêu cầu khôi phục mật khẩu cho tài khoản của bạn.</p>
                <p>Mã OTP của bạn là:</p>
                <h2 style='color:#007bff'>{otp}</h2>
                <p>Mã này sẽ hết hạn sau 5 phút.</p>
                <p>Nếu bạn không yêu cầu điều này, vui lòng bỏ qua email này.</p>
                <br/>
                <p>Trân trọng,<br/>Đội ngũ hỗ trợ EAUT-NCKH</p>";

            message.Body = bodyBuilder.ToMessageBody();
            _ = Task.Run(async () => {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 465, true);
                await smtp.AuthenticateAsync("phungdaidong1114@gmail.com", "kboe nntl cmsa bsme");
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);
            });

            return new Response {
                code = 0,
                message = "Mã OTP đã được gửi"
            };
        }

        public async Task<Response> VarifyOTP(string otp, string email) {
            var account = await _eautNckhContext.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (account == null) {
                return new Response { code = 1, message = "Tài khoản không tồn tại" };
            }

            if (account.Otp != otp) {
                return new Response { code = 1, message = "Mã OTP không đúng" };
            }

            var timeoutOTP = _configuration.GetValue<int>("OTP:Timeout");
            if ((DateTime.Now - account.Otpgeneratedat)?.TotalMinutes > timeoutOTP) {
                return new Response { code = 2, message = "Mã OTP đã hết hạn" };
            }

            return new Response { code = 0, message = "Xác thực thành công, mật khẩu mới đã được gửi tới email của bạn" };
        }

        public async Task<ResponseData<AccountDto>> GetAccountInformationAccount(int id) {
            var account = await _eautNckhContext.Accounts
                .Include(c => c.Department)
                .Include(c => c.Students)
                .ThenInclude(c => c.Major)
                .ThenInclude(c => c.Department)
                .Include(c => c.Teachers)
                .ThenInclude(c => c.Major)
                .ThenInclude(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == id);

            var accountDto = new AccountDto() {
                Id = account.Id,
                Fullname = account.Fullname,
                Email = account.Email,
                Sex = account.Sex,
                Phonenumber = account.Phonenumber,
                Avatar = account.Avatar,
                Roleid = account.Roleid,
                Role = account.Role,
                Departmentid = account.Departmentid,
                Department = account.Department,
                Students = account.Students,
                Teachers = account.Teachers
            };
            return new ResponseData<AccountDto> { code = 0, data= accountDto };
        }

        public async Task<ResponseData<List<AccountDto>>> SearchTeacher(string input, int departmentId) {
            input = input?.Trim();
            try {
                if (string.IsNullOrEmpty(input)) {
                    return new ResponseData<List<AccountDto>>(0, "OK", new List<AccountDto>());
                }
                var roleid = (int)RoleEnumId.RESEARCH_ADVISOR;
                var account = await _eautNckhContext.Accounts
                    .Include(c => c.Teachers)
                    .ThenInclude(t => t.Major)
                    .ThenInclude(m => m.Department)
                    .Where(c => c.Roleid == roleid
                        && (departmentId == -1 || c.Departmentid == departmentId )
                        && (
                            EF.Functions.FreeText(c.Fullname, input)
                            || c.Email == input.ToLower()
                            || EF.Functions.FreeText(c.Phonenumber, input)
                        )
                    )
                .Take(40)
                .Select(c => new AccountDto(c)).ToListAsync();

                return new ResponseData<List<AccountDto>>(0, "", account);
            } catch (Exception e) {
                return new ResponseData<List<AccountDto>>(0, "OK", new List<AccountDto>());
            }
        }
    }
}
