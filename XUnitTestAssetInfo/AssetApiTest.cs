using System;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;

using AssetInfo.BLL;
using AssetInfo.Entity;
using System.Net.Http;
using System.Text;
using System.Collections.Generic;

namespace XUnitTestAssetInfo
{
    public class AssetApiTest
    {
        readonly ITestOutputHelper log;
        public AssetApiTest(ITestOutputHelper output)
        {
            log = output;
        }

        HttpClient client = new HttpClient();
        readonly string url = "http://localhost:25800/asset/";
        private void clientRequestTest(string apiMethod, Dictionary<string, string> formData)
        {
            string tk = AssetInfo.BLL.Token.NewToken();
            client.DefaultRequestHeaders.Add("Auth", tk);
            HttpContent content = new FormUrlEncodedContent(formData);
            using (HttpResponseMessage responseMessage = client.PostAsync($"{url}{apiMethod}", content).Result)
            {
                Byte[] resultBytes = responseMessage.Content.ReadAsByteArrayAsync().Result;
                string res = Encoding.UTF8.GetString(resultBytes);
                log.WriteLine(res);
            }
        }
        [Fact]
        public void List()
        {
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("fields", "id,title");
            clientRequestTest("list", formData);
        }
        [Fact]
        public void History()
        {
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("id", "5ce71c0d131f46cf9c36d1d68e48b763");
            clientRequestTest("History", formData);
        }
        [Fact]
        public void Item()
        {
            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("id", "625f7c64c37c4f5ba2888f0b4417c41d");
            clientRequestTest("item", formData);
        }
    }
}
