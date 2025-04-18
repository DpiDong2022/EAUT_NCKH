namespace EAUT_NCKH.Web.DTOs {
    public class Response {
        public Response() {
        }

        public Response(string message) {
            this.message = message;
        }

        public Response(int code, string message) {
            this.code = code;
            this.message = message;
        }

        public int code { get; set; } = 1;
        public string message { get; set; }
    }
    public class ResponseData : Response {
        public ResponseData() : base() {
        }

        public ResponseData(string message, object data = null) : base(message) {
            this.data = data;
        }

        public ResponseData(int code, string message, object data = null) : base(code, message) {
            this.data = data;
        }

        public object data { get; set; } = null!;
    }
}
