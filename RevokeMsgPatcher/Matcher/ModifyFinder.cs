using RevokeMsgPatcher.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RevokeMsgPatcher.Matcher
{
    public class ModifyFinder
    {
        private sealed class PatternMatch
        {
            public ReplacePattern Pattern;
            public int[] Original;
            public int[] Replaced;
            public bool IsOriginal { get { return Replaced.Length == 0 && HasExpectedCount(Original.Length, Pattern); } }
            public bool IsReplaced { get { return Original.Length == 0 && HasExpectedCount(Replaced.Length, Pattern); } }
        }

        private static bool HasExpectedCount(int count, ReplacePattern pattern)
        {
            return pattern.ExpectedMatches > 0 ? count == pattern.ExpectedMatches : count > 0;
        }

        private static List<PatternMatch> Inspect(string path, List<ReplacePattern> patterns)
        {
            if (patterns == null || patterns.Count == 0)
                throw new BusinessException("empty_patterns", "所选功能没有特征码，不能安装补丁。");

            byte[] data = File.ReadAllBytes(path);
            var result = new List<PatternMatch>();
            foreach (ReplacePattern pattern in patterns)
            {
                if (pattern.Search == null || pattern.Replace == null || pattern.Search.Length == 0 ||
                    pattern.Search.Length != pattern.Replace.Length || pattern.ExpectedMatches < 0 ||
                    pattern.Search.SequenceEqual(pattern.Replace))
                    throw new BusinessException("invalid_pattern", "特征码为空、长度不一致或未包含有效修改。");

                result.Add(new PatternMatch {
                    Pattern = pattern,
                    Original = FuzzyMatcher.MatchAll(data, pattern.Search),
                    Replaced = FuzzyMatcher.MatchAll(data, pattern.Replace)
                });
            }
            return result;
        }

        public static List<Change> FindChanges(string path, List<ReplacePattern> replacePatterns)
        {
            var matches = Inspect(path, replacePatterns);
            if (matches.All(m => m.IsReplaced))
                throw new BusinessException("match_already_replace", "当前应用已经安装了所选功能的补丁。");

            // Validate each rule independently: extra hits cannot compensate for a missing rule.
            var invalid = matches.FirstOrDefault(m => !m.IsOriginal);
            if (invalid != null)
                throw new BusinessException("match_inconformity",
                    string.Format("特征比对失败：{0}，原始命中 {1}，已修改命中 {2}，期望 {3}。请还原已有补丁，或使用适配此版本的规则。",
                        invalid.Pattern.Category, invalid.Original.Length, invalid.Replaced.Length,
                        invalid.Pattern.ExpectedMatches > 0 ? invalid.Pattern.ExpectedMatches.ToString() : "至少 1"));

            var changes = matches.SelectMany(m => m.Original.Select(index => new Change(index, m.Pattern.Replace)))
                .OrderBy(c => c.Position).ToList();
            for (int i = 1; i < changes.Count; i++)
            {
                if (changes[i].Position < changes[i - 1].Position + changes[i - 1].Content.Length)
                    throw new BusinessException("overlapping_patterns", "特征码修改范围重叠，已停止安装。");
            }
            return changes;
        }

        // Item1: fully applicable categories; Item2: fully installed categories.
        public static Tuple<SortedSet<string>, SortedSet<string>> InspectFunctions(string path, List<ReplacePattern> patterns)
        {
            var available = new SortedSet<string>();
            var installed = new SortedSet<string>();
            foreach (var group in Inspect(path, patterns).GroupBy(m => m.Pattern.Category))
            {
                if (group.Key == null) continue;
                if (group.All(m => m.IsOriginal)) available.Add(group.Key);
                if (group.All(m => m.IsReplaced)) installed.Add(group.Key);
            }
            return Tuple.Create(available, installed);
        }

        public static SortedSet<string> FindReplacedFunction(string path, List<ReplacePattern> replacePatterns)
        {
            return InspectFunctions(path, replacePatterns).Item2;
        }
    }
}
