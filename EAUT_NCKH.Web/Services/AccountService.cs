using EAUT_NCKH.Web.DTOs;
using EAUT_NCKH.Web.IRepositories;
using EAUT_NCKH.Web.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EAUT_NCKH.Web.Services
{
    public class AccountService {

        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository) {
            _accountRepository = accountRepository;
        }

        public async Task<List<Account>>? GetAll(AccountRequestOptions? options) {
            return await _accountRepository.GetAll(options);
        }

        public async Task<List<Account>>? GetDataTable(AccountDataTableOptions options, int senderId) {
            return await _accountRepository.GetDataTable(options, senderId);
        }
        public async Task<double> GetCountDataTable(AccountDataTableOptions options, int senderId) {
            return await _accountRepository.GetCountDataTable(options, senderId);
        }
        public async Task<Response> ChangePasswordAsync(int userId, string currentPass, string newPass, string confirmNewPass) {
            return await _accountRepository.ChangePassword(userId, currentPass, newPass, confirmNewPass);
        }

        public async Task<ResponseData> AddOrEditToNCKH(GeneralInformationAccount viewModel) {
            return await _accountRepository.AddOrEditToNCKH(viewModel);
        }

        public async Task<ResponseData> AddOrEditTeacher(ResearchAdvisorViewModel viewModel, int createrId) {
            return await _accountRepository.AddOrEditTeacher(viewModel, createrId);
        }

        public async Task<ResponseData> AddOrEditStudent(StudentViewModel viewModel, int createrId) {
            return await _accountRepository.AddOrEditStudent(viewModel, createrId);
        }

        public async Task<ResponseData> GetAccountInformation(int id) {
            return await _accountRepository.GetAccountInformation(id);
        }
        public async Task<Account?> GetAccountByEmailAsync(string email) {
            return await _accountRepository.GetAccountByEmail(email);
        }

        public async Task<ResponseData> GetOTP(string email) {
            return await _accountRepository.CreateOTP(email);
        }

        public async Task<ResponseData> VarifyOTP(string otp, string email) {
            return await _accountRepository.VarifyOTP(otp, email);
        }
    }
}
