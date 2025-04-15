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

    public class AccountDataTableOptions : DataTableOptions {
        public AccountDataTableOptions()
        {
        }

        public int RoleId { get; set; } = 0;
        public int DepartmentId { get; set; } = 0;
        public string Search { get; set; } = "";
    }

    public class DataTableOptions {

        public double TotalRow { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageLength { get; set; } = 10;

        public int GetTotalPage() {

            return (int)Math.Ceiling(TotalRow / PageLength);
        }

        public int GetStartRow() {
            return (PageNumber - 1) * PageLength;
        }
    }
}
