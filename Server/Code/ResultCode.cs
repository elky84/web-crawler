namespace Server.Code
{
    public class ResultCode : EzAspDotNet.Protocols.Code.ResultCode
    {
        public ResultCode(int id, string name) : base(id, name)
        {
        }


        public readonly static ResultCode UsingSourceId = new(10000, "UsingSourceId");
        public readonly static ResultCode UsingRssId = new(10001, "UsingRssId");
        public readonly static ResultCode UsingNotificationId = new(10002, "UsingNotificationId");
        public readonly static ResultCode NotFoundSource = new(10003, "NotFoundSource");
    }
}
