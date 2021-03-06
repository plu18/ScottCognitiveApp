﻿using System;
using System.Linq;
using System.Text;


using ScottCognitiveApp.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ScottCognitiveApp.Bot
{
    public class BotConnector
    {
        private HttpClient _httpClient;
        private Conversation _lastConversation;
        private string _directLineKey = "fv4-T-vbyEk.cwA.bhI.5-ku3T0Fir7hOCyZbfuuPHehACWpdB945ceLHb73peI";

        public async Task<Conversation> Setup()
        {
            //instantiate an HTTPClient, and set properties to our DirectLine bot
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://directline.botframework.com/api/conversations/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("BotConnector", _directLineKey);
            var response = await _httpClient.GetAsync("/api/tokens/");

            if (response.IsSuccessStatusCode)
            {
                var conversation = new Conversation();
                HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(conversation), Encoding.UTF8,
                    "application/json");
                response = await _httpClient.PostAsync("/api/conversations/", contentPost);
                if (response.IsSuccessStatusCode)
                {
                    var conversationInfo = await response.Content.ReadAsStringAsync();
                    _lastConversation = JsonConvert.DeserializeObject<Conversation>(conversationInfo);
                }
            }
            return _lastConversation;
        }



        public async Task<ChatMessage> SendMessage(ChatMessage message)
        {

            var contentPost = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            var conversationUrl = "https://directline.botframework.com/api/conversations/" + _lastConversation.ConversationId + "/messages/";

            var response = await _httpClient.PostAsync(conversationUrl, contentPost);
            var messageInfo = await response.Content.ReadAsStringAsync();

            var messagesReceived = await _httpClient.GetAsync(conversationUrl);
            var messagesReceivedData = await messagesReceived.Content.ReadAsStringAsync();
            var messagesRoot = JsonConvert.DeserializeObject<BotMessageRoot>(messagesReceivedData);
            var messages = messagesRoot.Messages;


            return messages.Last();
        }
    }
}