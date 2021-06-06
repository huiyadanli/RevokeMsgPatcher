using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevokeMsgPatcher.Utils
{
    /// <summary>
    /// 用于软件的 Google Analytics 实现 By huiyadanli
    /// 相关文档：
    /// 指南 https://developers.google.com/analytics/devguides/collection/protocol/v1/devguide
    /// 参数 https://developers.google.com/analytics/devguides/collection/protocol/v1/parameters
    /// 测试 https://ga-dev-tools.appspot.com/hit-builder/
    /// </summary>
    public class GAHelper
    {
        private static GAHelper instance = null;
        private static readonly object obj = new object();

        public static GAHelper Instance
        {
            get
            {
                lock (obj)
                {
                    if (instance == null)
                    {
                        instance = new GAHelper();
                    }
                    return instance;
                }
            }
        }

        // 根据实际情况修改
        private static readonly HttpClient client = HttpUtil.Client;

        private const string GAUrl = "https://www.google-analytics.com/collect";

        // 根据实际使用分析账号设置
        private const string tid = "UA-80358493-2"; // GA Tracking ID / Property ID.

        private static readonly string cid = Device.Value(); // Anonymous Client ID. // Guid.NewGuid().ToString()

        // 屏幕分辨率(可选)
        private static readonly string sr = Screen.PrimaryScreen.Bounds.Width + "x" + Screen.PrimaryScreen.Bounds.Height;

        public string UserAgent { get; set; }

        public GAHelper()
        {
            UserAgent = string.Format("Hui Google Analytics Tracker/1.0 ({0}; {1}; {2})", Environment.OSVersion.Platform.ToString(), Environment.OSVersion.Version.ToString(), Environment.OSVersion.VersionString);
        }

        public async Task RequestPageViewAsync(string page, string title = null)
        {
            try
            {
                if (!page.StartsWith("/"))
                {
                    page = "/" + page;
                }
                // 请求参数
                var values = new Dictionary<string, string>
                {
                    { "v", "1" }, // 当前必填1
                    { "tid", tid },
                    { "cid", cid },
                    { "ua", UserAgent },
                    { "t", "pageview" },
                    { "sr", sr },
                    { "dp", page },
                    { "dt", title },
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(GAUrl, content);
            }
            catch (Exception ex)
            {

                Console.WriteLine("GAHelper:" + ex.Message);
            }
        }

        public void RequestPageView(string page, string title = null)
        {
            Task.Run(() => RequestPageViewAsync(page, title));
        }
    }
}
