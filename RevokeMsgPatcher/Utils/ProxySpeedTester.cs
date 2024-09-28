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
        private static readonly string TargetUrl = "https://raw.githubusercontent.com/LiteLoaderQQNT/LiteLoaderQQNT/refs/heads/main/package.json";

        private static readonly HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };

        public static readonly List<string> ProxyUrls = new List<string>()
        {
            "https://mirror.ghproxy.com",
            "https://hub.gitmirror.com",
            "https://ghproxy.cc",
            "https://www.ghproxy.cc",
            "https://ghproxy.cn",
            "https://ghproxy.net",
        };

        public static async Task<string> GetFastestProxyAsync()
        {
            return await GetFastestProxyAsync(ProxyUrls);
        }

        public static async Task<string> GetFastestProxyAsync(List<string> proxyAddresses)
        {
            var tasks = new List<Task<string>>();
            var cts = new CancellationTokenSource();

            foreach (var proxy in proxyAddresses)
            {
                tasks.Add(TestProxyAsync(proxy, cts.Token));
            }

            var firstCompletedTask = await Task.WhenAny(tasks);
            cts.Cancel(); // 取消所有其他请求

            try
            {
                return await firstCompletedTask; // 返回第一个完成的代理地址
            }
            catch (OperationCanceledException)
            {
                return null; // 如果第一个任务被取消，返回 null
            }
        }

        private static async Task<string> TestProxyAsync(string proxyAddress, CancellationToken cancellationToken)
        {
            try
            {
                // 模拟代理测试请求
                var response = await _httpClient.GetAsync(proxyAddress, cancellationToken);
                response.EnsureSuccessStatusCode();
                return proxyAddress;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}