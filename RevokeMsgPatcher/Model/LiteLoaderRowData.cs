using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RevokeMsgPatcher.Model.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = System.Windows.Forms.Application;

namespace RevokeMsgPatcher.Model
{
    internal class LiteLoaderRowData
    {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string Name { get; set; }

        public string NameLink { get; set; }

        public string Author { get; set; }

        public string AuthorLink { get; set; }

        public string Status { get; set; }

        public event Action<string> StatusUpdated;

        public DataGridViewRow Row { get; set; }

        /// <summary>
        /// 本地存储的目录
        /// </summary>
        public string LocalPath { get; set; }

        /// <summary>
        /// 主干名称
        /// </summary>
        public string MainBranchName { get; set; }

        private void UpdateStatus(string newStatus)
        {
            Status = newStatus;
            StatusUpdated?.Invoke(newStatus);
        }


        /// <summary>
        /// 由于这个 api.github.com 没有加速的方式，所以不用了
        /// </summary>
        [Obsolete("由于这个 api.github.com 没有加速的方式，所以不用了")]
        public string ReleasesApi
        {
            get
            {
                var repo = NameLink.Replace(@"https://github.com", @"https://api.github.com/repos");
                return repo + @"/releases/latest";
            }
        }

        public string DownloadUrl { get; set; }

        public string VersionJsonUrl
        {
            get
            {
                var repo = NameLink.Replace(@"https://github.com", @"https://raw.githubusercontent.com");
                if (NameLink == "https://github.com/LiteLoaderQQNT/LiteLoaderQQNT")
                {
                    return repo + $@"/refs/heads/{MainBranchName}/package.json";
                }
                else
                {
                    return repo + $@"/refs/heads/{MainBranchName}/manifest.json";
                }
            }
        }

        public string GetLocalVersion()
        {
            if (NameLink.Contains("QQNTFileVerifyPatch"))
            {
                return null;
            }

            if (!Directory.Exists(LocalPath))
            {
                Directory.CreateDirectory(LocalPath);
            }

            string path = null;
            path = Path.Combine(LocalPath, NameLink == "https://github.com/LiteLoaderQQNT/LiteLoaderQQNT" ? "package.json" : "manifest.json");

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var package = JsonConvert.DeserializeObject<VersionJson>(json, SerializerSettings);
                return package.Version;
            }

            return null;
        }

        public void GetLocalVersionAndUpdateStatus()
        {
            var localVersion = GetLocalVersion();
            if (localVersion != null)
            {
                UpdateStatus($"当前版本{localVersion}");
            }
            else
            {
                if (NameLink.Contains("QQNTFileVerifyPatch"))
                {
                    UpdateStatus("无需更新");
                }
                else
                {
                    UpdateStatus("未检查");
                }
            }
        }

        public async Task<string> GetRemoteVersion(string proxyUrl)
        {
            using (var client = new HttpClient())
            {
                var url = FormatUrl(proxyUrl, VersionJsonUrl);
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var package = JsonConvert.DeserializeObject<VersionJson>(json, SerializerSettings);
                    return package.Version;
                }

                return null;
            }
        }

        private string FormatUrl(string proxyUrl, string target)
        {
            if (string.IsNullOrEmpty(proxyUrl))
            {
                return target;
            }
            else
            {
                if (proxyUrl.Contains("{0}"))
                {
                    return string.Format(proxyUrl, target);
                }
                else
                {
                    if (!proxyUrl.EndsWith("/"))
                    {
                        proxyUrl += "/";
                    }

                    return proxyUrl + target;
                }
            }
        }

        public async Task CheckAndUpdate(string proxyUrl = null)
        {
            try
            {
                if (NameLink.Contains("QQNTFileVerifyPatch"))
                {
                    return;
                }

                string localVersion = GetLocalVersion();
                string remoteVersion = await GetRemoteVersion(proxyUrl);

                if (localVersion == null || new Version(remoteVersion) > new Version(localVersion))
                {
                    UpdateStatus($"存在新版本{remoteVersion}，正在下载...");
                    Debug.WriteLine("发现新版本，正在下载...");
                    var url = DownloadUrl.Replace("#{version}", remoteVersion);
                    url = FormatUrl(proxyUrl, url);
                    string downloadedFilePath = await DownloadLatestPackage(url, Path.Combine(Application.StartupPath, "Public/Download"));
                    Debug.WriteLine("下载到：" + downloadedFilePath);
                    UpdateStatus($"下载成功，解压中...");

                    // 解压
                    string zipFileName = Path.GetFileNameWithoutExtension(downloadedFilePath);
                    string extractPath = Path.Combine(Application.StartupPath, "Public/Extracted", zipFileName);
                    if (Directory.Exists(extractPath))
                    {
                        Directory.Delete(extractPath, true);
                    }

                    Directory.CreateDirectory(extractPath);
                    ZipFile.ExtractToDirectory(downloadedFilePath, extractPath);

                    Debug.WriteLine("解压至：" + extractPath);
                    UpdateStatus($"解压成功，替换中...");

                    // 找到根目录
                    string pluginPath = FindDirectoryWithJson(extractPath);
                    Debug.WriteLine("解压后的插件/本体目录：" + pluginPath);

                    // 拷贝
                    DirectoryCopy(pluginPath, LocalPath);
                    Debug.WriteLine("拷贝至：" + LocalPath);


                    // 清理
                    if (File.Exists(downloadedFilePath))
                    {
                        File.Delete(downloadedFilePath);
                    }

                    if (Directory.Exists(extractPath))
                    {
                        Directory.Delete(extractPath, true);
                    }

                    Debug.WriteLine("清理完成。");
                    UpdateStatus($"{remoteVersion}更新完成");
                }
                else
                {
                    UpdateStatus($"已是最新版本{localVersion}");
                    Debug.WriteLine("当前已是最新版本。");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                UpdateStatus(Status + " 后发生异常：" + e.Message);
            }
        }

        private string FindDirectoryWithJson(string extractPath, int maxDepth = 2)
        {
            return FindDirectoryWithJsonRecursive(extractPath, maxDepth, 0);
        }

        private string FindDirectoryWithJsonRecursive(string currentPath, int maxDepth, int currentDepth)
        {
            if (currentDepth > maxDepth)
            {
                return null;
            }

            string[] jsonFiles = { "package.json", "manifest.json" };
            foreach (var jsonFile in jsonFiles)
            {
                if (File.Exists(Path.Combine(currentPath, jsonFile)))
                {
                    return currentPath;
                }
            }

            if (currentDepth < maxDepth)
            {
                foreach (var directory in Directory.GetDirectories(currentPath))
                {
                    var result = FindDirectoryWithJsonRecursive(directory, maxDepth, currentDepth + 1);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        private async Task<string> DownloadLatestPackage(string url, string localDirectory)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    var fileName = Path.GetFileName(url);
                    var localPath = Path.Combine(localDirectory, fileName);
                    Directory.CreateDirectory(localDirectory); // 确保目录存在
                    File.WriteAllBytes(localPath, data);
                    return localPath;
                }
                else
                {
                    throw new Exception("下载失败");
                }
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("源目录不存在: " + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}