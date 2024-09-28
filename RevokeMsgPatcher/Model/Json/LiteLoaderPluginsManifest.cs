namespace RevokeMsgPatcher.Model.Json
{

    /// <summary>
    /// 只有部分信息，主要是拿版本号
    /// https://github.com/xh321/LiteLoaderQQNT-Anti-Recall/blob/master/manifest.json
    /// </summary>
    internal class LiteLoaderPluginsManifest
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Icon { get; set; }
    }
}
