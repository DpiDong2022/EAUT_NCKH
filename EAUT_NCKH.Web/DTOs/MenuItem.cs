namespace EAUT_NCKH.Web.DTOs {
    public class MenuItem {
        public string Title { get; set; }
        public string Url { get; set; } = "#";
        public List<MenuItem> SubItems { get; set; } = new List<MenuItem>();
    }
}
