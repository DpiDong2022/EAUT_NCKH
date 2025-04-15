namespace EAUT_NCKH.Web.DTOs.Options {
    public class TopicDataTableOptions : DataTableOptions {
        public TopicDataTableOptions() {
        }

        public int DepartmentId { get; set; } = -1;
        public string Search { get; set; } = "";
        public int Status { get; set; } = -1;
        public string SubStatus { get; set; } = "-1";
        public int Year { get; set; } = DateTime.Now.Year;
    }
}
