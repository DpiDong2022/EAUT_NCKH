using EAUT_NCKH.Web.DTOs.Options;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace EAUT_NCKH.Web.DTOs
{
    public class AccountRequestOptions
    {
        public AccountRequestOptions()
        {
        }

        public AccountRequestOptions(string accountName)
        {
            AccountName = accountName.ToLower();
        }


        public string AccountName { get; set; }
    }
}
