using Newtonsoft.Json;

namespace Discord_BotGPT.GptInteraction
{
    internal class GPTRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "gpt-4";

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
