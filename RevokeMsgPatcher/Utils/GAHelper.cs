using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace RevokeMsgPatcher.Utils
{
    /// <summary>
    /// 用于软件的 Google Analytics 实现 By huiyadanli
    /// 20230409 更新 GA4 的实现
    /// 相关文档：
    /// #GA指南(过时) https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide
    /// #GA参数(过时) https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
    /// GA4教程 https://firebase.google.com/codelabs/firebase_mp
    /// 测试 https://ga-dev-tools.google/ga4/event-builder/
    /// </summary>
    public class GAHelper
    {
        private static GAHelper instance = null;
        private static readonly object obj = new object();

        public static GAHelper Instance
        {
            get
            {
                //lock (obj)
                //{
                if (instance == null)
                {
                    instance = new GAHelper();
                }
                return instance;
                //}
            }
        }

        // 根据实际情况修改
        private static readonly HttpClient client = HttpUtil.Client;

        private const string GAUrl = "https://www.google-analytics.com/mp/collect?api_secret=urKlcc29TSy3OIkHr8yFSQ&measurement_id=G-BE6FRPZS1W";

        private static readonly string cid = Device.Value(); // Anonymous Client ID. // Guid.NewGuid().ToString()


        public string UserAgent { get; set; }

        public GAHelper()
        {
            UserAgent = string.Format("Hui Google Analytics Tracker/1.0 ({0}; {1}; {2})", Environment.OSVersion.Platform.ToString(), Environment.OSVersion.Version.ToString(), Environment.OSVersion.VersionString);
        }

        public async Task RequestPageViewAsync(string page, string title = null)
        {
            try
            {
                if (page.StartsWith("/"))
                {
                    page = page.Remove(0, 1);
                }
                page = page.Replace("/", "_").Replace(".", "_");
                // 请求参数
                var values = new Dictionary<string, object>
                {
                    { "client_id",UserAgent},
                    { "user_id", cid },
                    { "non_personalized_ads", "false" },
                    { "events", new List<Dictionary<string, string>>()
                        {
                            new Dictionary<string, string>()
                            {
                                 { "name",page},
                            }
                        }
                    },
                };
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(values);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(GAUrl, content);
                Console.WriteLine(response.ToString());
            }
            catch (Exception ex)
            {

                Console.WriteLine("GAHelper:" + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void RequestPageView(string page, string title = null)
        {
            Task.Run(() => RequestPageViewAsync(page, title));
        }
    }
}
