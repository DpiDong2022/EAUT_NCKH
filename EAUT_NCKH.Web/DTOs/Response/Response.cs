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

        public virtual int code { get; set; } = 1;
        public virtual string message { get; set; }
    }
    public class ResponseData<T> : Response {
        public ResponseData() : base() {
        }

        public ResponseData(string message, T data) : base(message) {
            this.data = data;
        }

        public ResponseData(string message) : base(message) {
        }
        public ResponseData(int code, string message) : base(code, message) {
        }

        public ResponseData(int code, string message, T data) : base(code, message) {
            this.data = data;
        }

        public T? data { get; set; }
    }

    public class ResponseOk : Response
    {
        public override string message { get; set; } = "OK";
        public override int code { get; set; } = 0;
    }
}
