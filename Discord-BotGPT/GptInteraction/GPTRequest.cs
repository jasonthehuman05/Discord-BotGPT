using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_BotGPT.GptInteraction
{
    internal class GPTRequest
    {
        [JsonProperty("model")]
        string Model { get; set; } = "gpt-4";

        [JsonProperty("messages")]
        public Discord_BotGPT.Message[] Messages { get; set; }
    }

    //public class Message
    //{
    //    [JsonProperty("role")]
    //    public string Role { get; set; }

    //    [JsonProperty("content")]
    //    public string Content { get; set; }
    //}
}
