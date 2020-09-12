using System.Net;

namespace Server.Exception
{
    public class DeveloperException : System.Exception
    {
        public Code.ResultCode ResultCode { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public string Detail { get; set; }

        public DeveloperException(Code.ResultCode resultCode, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError, string detail = null)
        {
            ResultCode = resultCode;
            HttpStatusCode = httpStatusCode;
            Detail = detail;
        }
    }
}
