namespace Server.Exception
{
    public class ErrorDetails : Protocols.Response.Header
    {
        public int StatusCode { get; set; }

        public string Detail { get; set; }

    }
}
