using System.Collections.Generic;
using System.Net.Http;
using ZulipNetCore.Interfaces;

namespace ZulipNetCore {

    public class StreamMessage : IParseResponse {

        private static ZulipClient _ZulipClient;
        private static HttpClient _HttpClient;
        public string JsonOutput;
        public bool StatusCode;
        public string ResponseMessage { get; protected set; }
        public string ResponseResult { get; protected set; }
        public string ResponseID { get; protected set; }
        private object ResponseArray;

        public StreamMessage(ZulipClient ZulipClient) {
            _ZulipClient = ZulipClient;
            _HttpClient = ZulipClient.Login();
        }

        public async void PostStreamMessage(string StreamName, string Topic, string MessageText) {
            //https://stackoverflow.com/questions/30858890/how-to-use-httpclient-to-post-with-authentication
            string targetURL = $"{_ZulipClient.Server.ServerApiURL}/{EndPointPath.Messages}";
            var request = new HttpRequestMessage(HttpMethod.Post, targetURL);
            var FormData = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("type", "stream"),
                new KeyValuePair<string, string>("to", StreamName),
                new KeyValuePair<string, string>("subject", Topic),
                new KeyValuePair<string, string>("content", MessageText)
            };

            request.Content = new FormUrlEncodedContent(FormData);
            using (HttpResponseMessage Response = await _HttpClient.SendAsync(request))
            using (HttpContent content = Response.Content) {
                JsonOutput = string.Copy(await Response.Content.ReadAsStringAsync());
                StatusCode = Response.IsSuccessStatusCode;
            }
            ParseResponse();
        }

        public void ParseResponse() {
            var Json = new JSONHelper();
            dynamic JObj = Json.ParseJSON(JsonOutput);
            ResponseMessage = JObj.msg;
            ResponseResult = JObj.result;
            ResponseID = JObj.id;
        }

    }
}
