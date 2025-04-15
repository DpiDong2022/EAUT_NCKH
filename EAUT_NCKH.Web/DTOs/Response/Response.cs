namespace EAUT_NCKH.Web.DTOs {
    public class Response {
        public int code { get; set; }
        public string message { get; set; }
    }
    public class ResponseData {
        public int code { get; set; }
        public string message { get; set; }
        public object data { get; set; } = null!;
    }
}
