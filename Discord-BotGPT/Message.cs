using Newtonsoft.Json;

namespace Discord_BotGPT
{
    internal class Message
    {
        [JsonProperty("content")]
        public string Content { get; set; } = "gpt-4-0613";

        [JsonProperty("role")]
        public string Role { get; set; }
    }
}