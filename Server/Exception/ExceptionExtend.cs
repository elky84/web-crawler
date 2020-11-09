using Serilog;
using System.Net;

namespace Server.Exception
{
    public static class ExceptionExtend
    {
        public static void ExceptionLog(this System.Exception e)
        {
            if (e.InnerException?.GetType() == typeof(DeveloperException))
            {
                var developerException = (DeveloperException)e.InnerException;
                Log.Logger.Error($"[Exception] [Message:{developerException.Message}], [ResultCode:{developerException.ResultCode}]");
                Log.Logger.Error($"[Exception] [Source:{e.Source}], [InnerSource:{developerException.Source}]");
                Log.Logger.Error($"[StrackTrace] [{e.StackTrace}]");
                Log.Logger.Error($"[InnerStrackTrace] [{developerException.StackTrace}]");
            }
            else if (e.InnerException != null)
            {
                Log.Logger.Error($"[Exception] [Message:{e.Message}] [InnerMessage:{e.InnerException.Message}]");
                Log.Logger.Error($"[Exception] [Source:{e.Source}] [InnerSource:{e.InnerException.Source}]");
                Log.Logger.Error($"[StrackTrace] [{e.StackTrace}]");
                Log.Logger.Error($"[InnerStrackTrace] [{e.InnerException.StackTrace}]");
            }
            else
            {
                Log.Logger.Error($"[Exception] [Message:{e.Message}] [Source:{e.Source}]");
                Log.Logger.Error($"[StrackTrace] [{e.StackTrace}]");
            }
        }
    }
}
