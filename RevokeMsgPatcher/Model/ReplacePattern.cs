using System;

namespace RevokeMsgPatcher.Model
{
    public class ReplacePattern
    {
        public byte[] Search { get; set; }

        public byte[] Replace { get; set; }

        public string Category { get; set; }

        /// <summary>
        /// 悬浮气泡提示
        /// </summary>
        public string Tips { get; set; }

        /// <summary>
        /// 同类冲突标签
        /// </summary>
        public string SimilarCategories { get; set; }

        /// <summary>
        /// 选择同类冲突标签时的提示
        /// </summary>
        public string ChooseSimilarCategoriesMsg { get; set; }

        public ReplacePattern Clone()
        {
            ReplacePattern o = new ReplacePattern();
            o.Search = Search;
            o.Replace = Replace;
            return o;
        }
    }
}
