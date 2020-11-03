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
                Log.Logger.Error($"[Exception] LoopingService [Message:{developerException.Message}], [ResultCode:{developerException.ResultCode}]");
                Log.Logger.Error($"[StrackTrace] LoopingService [{developerException.StackTrace}]");
            }
            else if (e.InnerException != null)
            {
                Log.Logger.Error($"[Exception] LoopingService [Message:{e.Message}] [InnerMessage:{e.InnerException.Message}]");
                Log.Logger.Error($"[StrackTrace] LoopingService [{e.InnerException.StackTrace}]");
                Log.Logger.Error($"[InnerStrackTrace] LoopingService [{e.StackTrace}]");
            }
            else
            {
                Log.Logger.Error($"[Exception] LoopingService [Message:{e.Message}]");
                Log.Logger.Error($"[StrackTrace] LoopingService [{e.StackTrace}]");
            }
        }
    }
}
