namespace EAUT_NCKH.Web.DTOs.Options {
    public class DataTableOptions {

        public double TotalRow { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageLength { get; set; } = 10;
        public int Start { get; set; }
        public int End { get; set; }
        public int Range { get; set; } = 3;
        public int GetTotalPage() {

            return (int)Math.Ceiling(TotalRow / PageLength);
        }

        public int GetStartRow() {
            if (PageNumber == 0) {
                return (PageNumber) * PageLength;
            }
            return (PageNumber - 1) * PageLength;
        }
    }
}
