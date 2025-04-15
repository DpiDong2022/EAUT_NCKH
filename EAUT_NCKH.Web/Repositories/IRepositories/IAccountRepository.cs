using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.IRepositories
{
    public interface IAccountRepository {
        Task<List<Account>>? GetAll(AccountRequestOptions options);
        //Response UpdateAccount();
        Task<List<Account>>? GetDataTable(AccountDataTableOptions options, int userId);
        Task<double> GetCountDataTable(AccountDataTableOptions options, int userId);
        public Task<Response> ChangePassword(int userId, string currentPass, string newPass, string confirmNewPass);
        Response CheckPassword(string realPass, string currentPassInput, string newPass, string confirmNewPass);
        Task<ResponseData> AddOrEditToNCKH(GeneralInformationAccount viewModel);
        Task<ResponseData> AddOrEditTeacher(ResearchAdvisorViewModel viewModel, int createrId);
        Task<ResponseData> AddOrEditStudent(StudentViewModel viewModel, int createrId);
        Task<ResponseData> GetAccountInformation(int id);
        Task<Account?> GetAccountByEmail(string email);
        Task<ResponseData> CreateOTP(string email);
        Task<ResponseData> VarifyOTP(string otp, string email);
    }
}
