using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Utils
{
    public class ProxySpeedTester
    {
        public static readonly string TargetUrl = "https://raw.githubusercontent.com/LiteLoaderQQNT/LiteLoaderQQNT/refs/heads/main/package.json";

        private static readonly HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };

        public static readonly List<string> ProxyUrls = new List<string>()
        {
            "{0}",
            "https://mirror.ghproxy.com/{0}",
            "https://hub.gitmirror.com/{0}",
            "https://ghproxy.cc/{0}",
            "https://www.ghproxy.cc/{0}",
            "https://ghproxy.cn/{0}",
            "https://ghproxy.net/{0}"
        };

        /// <summary>
        /// 获得最快的代理地址
        /// </summary>
        /// <param name="target"></param>
        /// <returns>最快的代理地址,结果</returns>
        public static async Task<Tuple<string, string>> GetFastestProxyAsync(string target)
        {
            return await GetFastestProxyAsync(ProxyUrls, target);
        }

        public static async Task<Tuple<string, string>> GetFastestProxyAsync(List<string> proxyAddresses, string target)
        {
            var tasks = new List<Task<Tuple<string, string, bool>>>(); // 修改为包含成功标志的元组
            var cts = new CancellationTokenSource();

            foreach (var proxy in proxyAddresses)
            {
                // 如果目标地址为空且代理地址为默认地址，则跳过
                if (string.IsNullOrEmpty(target) && proxy == "{0}")
                {
                    continue;
                }

                tasks.Add(TestProxyAsync(proxy, target, cts.Token));
            }

            while (tasks.Count > 0)
            {
                var firstCompletedTask = await Task.WhenAny(tasks);
                tasks.Remove(firstCompletedTask);

                var result = await firstCompletedTask;
                if (result.Item3) // 检查是否成功
                {
                    cts.Cancel(); // 取消所有其他请求
                    return new Tuple<string, string>(result.Item1, result.Item2); // 返回第一个成功的代理地址
                }
            }

            return new Tuple<string, string>(string.Empty, string.Empty); // 如果没有成功的结果，返回空
        }

        private static async Task<Tuple<string, string, bool>> TestProxyAsync(string proxyAddress, string target, CancellationToken cancellationToken)
        {
            try
            {
                // 模拟代理测试请求
                var response = await _httpClient.GetAsync(string.Format(proxyAddress, target), cancellationToken);
                response.EnsureSuccessStatusCode();
                return new Tuple<string, string, bool>(proxyAddress.Replace("{0}", ""), await response.Content.ReadAsStringAsync(), true);
            }
            catch (Exception e)
            {
                return new Tuple<string, string, bool>(proxyAddress.Replace("{0}", ""), e.Message, false);
            }
        }
    }
}