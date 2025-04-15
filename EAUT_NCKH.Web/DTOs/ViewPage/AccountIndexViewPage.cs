

using EAUT_NCKH.Web.Models;

namespace EAUT_NCKH.Web.DTOs {
    public class ViewPage {
        public int Start { get; set; }
        public int End { get; set; }
        public int Range { get; set; } = 3;
        public virtual void CalculatePagination() {

        }
    }

    public class AccountIndexViewPage : ViewPage {
        public AccountDataTableOptions Pagination { get; set; } = new AccountDataTableOptions();
        public List<Account> Accounts { get; set; } = new List<Account>();

        public override void CalculatePagination() {
            base.CalculatePagination();
            if (Pagination != null) {
                Start = Math.Max(1, Pagination.PageNumber - Range);
                End = Math.Min(Pagination.GetTotalPage(), Pagination.PageNumber + Range);
                if (Accounts.Count==0) {
                    Pagination.PageNumber = 0;
                }
            }
        }
    }
}
