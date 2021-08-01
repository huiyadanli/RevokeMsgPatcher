using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Utils
{
    public class HttpUtil
    {
        public static HttpClient Client { get; } = new HttpClient();

        static HttpUtil()
        {
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        /// <summary>
        /// 补丁路径
        /// 已经弃用的路径
        /// https://swordmaker-hauls-51508.netlify.com/i/revokemsg/05.json
        /// https://huiyadanli.github.io/i/revokemsg/05.json
        /// </summary>
        private static readonly string[] urls = new string[]
        {
            "https://gitee.com/huiyadanli/RevokeMsgPatcher/raw/master/RevokeMsgPatcher.Assistant/Data/1.1/patch.json",
            "https://huiyadanli.coding.net/p/RevokeMsgPatcher/d/RevokeMsgPatcher/git/raw/master/RevokeMsgPatcher.Assistant/Data/1.1/patch.json",
            "https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/RevokeMsgPatcher.Assistant/Data/1.1/patch.json"
        };

        private static int i = 0;

        public static async Task<string> GetPatchJsonAsync()
        {
            try
            {
                return await Client.GetStringAsync(urls[i]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("第" + (i + 1) + "次请求异常:[" + ex.Message + "]\nURL:" + urls[i]);
                GAHelper.Instance.RequestPageView($"/main/json/request_ex/{i + 1}/{ex.Message}", "第" + (i + 1) + "次请求异常:[" + ex.Message + "]");
                i++;
                if (i >= urls.Length)
                {
                    i = 0;
                    return null;
                }
                else
                {
                    return await GetPatchJsonAsync();
                }
            }
        }
    }
}
