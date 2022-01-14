using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCrawler.Util
{
    public static class StringUtil
    {
        // >은 link bracket을 깨뜨려서, :는 Slack Desktop notification에서 하이라이팅되서
        public static string ToWebHookText(this string text) => text.Replace('<', '(')
                                                                  .Replace('>', ')')
                                                                  .Replace(':', ';');
    }
}
