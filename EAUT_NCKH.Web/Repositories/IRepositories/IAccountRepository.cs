using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace EAUT_NCKH.Web.IRepositories
{
    public interface IAccountRepository {
        Task<List<Account>>? GetAll(AccountRequestOptions options);
        //Response UpdateAccount();
        Task<List<Account>>? GetDataTable(AccountIndexViewPage options, int userId);
        Task<double> GetCountDataTable(AccountIndexViewPage options, int userId);
        public Task<Response> ChangePassword(int userId, string currentPass, string newPass, string confirmNewPass);
        Response CheckPassword(string realPass, string currentPassInput, string newPass, string confirmNewPass);
        Task<ResponseData<int>> AddOrEditToNCKH(GeneralInformationAccount viewModel);
        Task<ResponseData<int>> AddOrEditTeacher(ResearchAdvisorViewModel viewModel, int createrId);
        Task<ResponseData<int>> AddOrEditStudent(StudentViewModel viewModel, int createrId);
        Task<ResponseData<object>> GetAccountInformation(int id);
        Task<ResponseData<AccountDto>> GetAccountInformationAccount(int id);
        Task<Account?> GetAccountByEmail(string email);
        Task<Response> CreateOTP(string email);
        Task<Response> VarifyOTP(string otp, string email);
        Task<ResponseData<List<AccountDto>>> SearchTeacher(string input, int departmentId);
    }
}
