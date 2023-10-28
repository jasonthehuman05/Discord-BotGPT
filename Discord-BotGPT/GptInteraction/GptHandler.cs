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
        public string model = "gpt-4-0613";
        HttpClient httpClient;

        public GptHandler() 
        {
            //load token from file
            token = System.IO.File.ReadAllText(".openai");
            
            //create http client with token attached for later use
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<GPTResponse> SendMessage(Discord_BotGPT.Message[] input)
        {
            //Do some stuff to get the input
            GPTRequest data = new GPTRequest
            {
                Messages = input,
                Model= model
            };
            //Send to API
            string request = JsonConvert.SerializeObject(data);
            HttpContent c = new StringContent(request, Encoding.UTF8, "application/json");
            #region REMOVE COMMENTS TO USE OPENAI AGAIN
            HttpResponseMessage response = await httpClient.PostAsync(apiUrl, c);

            //receive response
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine(jsonResponse);
            #endregion
            GPTResponse responseObject = JsonConvert.DeserializeObject<GPTResponse>(jsonResponse);
            //string message =
            return responseObject;
        }
    }
}
