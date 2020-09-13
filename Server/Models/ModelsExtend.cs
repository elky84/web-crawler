using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Models
{
    public static class ModelsExtend
    {
        public static Protocols.Common.Source ToProtocol(this WebCrawler.Models.Source source)
        {
            return new Protocols.Common.Source
            {
                Id = source.Id,
                BoardId = source.BoardId,
                Type = source.Type,
                Name = source.Name
            };
        }

        public static WebCrawler.Models.Source ToModel(this Protocols.Common.Source source)
        {
            return new WebCrawler.Models.Source
            {
                Id = source.Id,
                BoardId = source.BoardId,
                Type = source.Type,
                Name = source.Name
            };
        }
    }
}
