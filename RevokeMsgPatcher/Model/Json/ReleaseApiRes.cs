using System;
using System.Collections.Generic;

namespace RevokeMsgPatcher.Model.Json
{
    internal class ReleaseApiRes
    {

        public string Url { get; set; }
        public string AssetsUrl { get; set; }
        public string UploadUrl { get; set; }
        public string HtmlUrl { get; set; }
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string TagName { get; set; }
        public string TargetCommitish { get; set; }
        public string Name { get; set; }
        public bool Draft { get; set; }
        public bool Prerelease { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public List<Asset> Assets { get; set; }
        public string TarballUrl { get; set; }
        public string ZipballUrl { get; set; }
        public string Body { get; set; }
    }

    public class Asset
    {
        public string Url { get; set; }
        public int Id { get; set; }
        public string NodeId { get; set; }
        public string Name { get; set; }
        public object Label { get; set; }
        public string ContentType { get; set; }
        public string State { get; set; }
        public int Size { get; set; }
        public int DownloadCount { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string BrowserDownloadUrl { get; set; }
    }
}
