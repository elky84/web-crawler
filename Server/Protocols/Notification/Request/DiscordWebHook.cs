using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Notification.Request
{
    public class DiscordWebHook
    {

#pragma warning disable IDE1006 // Naming Styles
        public class EmbedImage
        {
            public Dictionary<string, string> image { get; set; } = new Dictionary<string, string>();
        }

        public string content { get; set; }

        public string username { get; set; }

        public string avatar_url { get; set; }

        public List<object> embeds = new List<object>();

#pragma warning restore IDE1006 // Naming Styles

        [JsonIgnore]
        public string HookUrl { get; set; }

        public DiscordWebHook AddImage(List<string> imageUrls)
        {
            if (imageUrls == null)
            {
                return this;
            }

            foreach (var imageUrl in imageUrls)
            {
                var embedImage = new EmbedImage();
                embedImage.image.Add("url", imageUrl);
                embeds.Add(embedImage);
            }

            return this;
        }

        public DiscordWebHook Clone()
        {
            return new DiscordWebHook
            {
                avatar_url = avatar_url,
                content = content,
                embeds = embeds,
                HookUrl = HookUrl,
                username = username
            };
        }
    }
}
