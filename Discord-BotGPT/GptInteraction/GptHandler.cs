using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discord_BotGPT.GptInteraction
{
    internal class GptHandler
    {
        string token;
        string apiUrl = "https://api.openai.com/v1/chat/completions";
        HttpClient httpClient;

        public GptHandler() 
        {
            //load token from file
            token = System.IO.File.ReadAllText(".openai");
            
            //create http client with token attached for later use
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorisation", $"Bearer {token}");
        }

        public async Task<string> SendMessage(Message[] input)
        {
            //Do some stuff to get the input
            GPTRequest data = new GPTRequest
            {
                Messages = input
            };
            //Send to API
            string request = JsonConvert.SerializeObject(data);
            HttpContent c = new StringContent(request);
            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, c);

            //receive response
            string jsonResponse = await response.Content.ReadAsStringAsync();

            return null;
        }
    }
}
