using RevokeMsgPatcher;
using RevokeMsgPatcher.Matcher;
using RevokeMsgPatcher.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

internal static class Program
{
    private static int assertions;

    private static int Main(string[] args)
    {
        try
        {
            TestBoundaries();
            TestRandomMatches();
            string root = args.Length > 0 ? args[0] : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\.."));
            TestConfiguredPatterns(root);
            TestModifyFinder();
            TestQqntAdaptiveLocator();
            TestAllocation();
            Console.WriteLine("PASS: {0} assertions (.NET Framework 4.5.2 target)", assertions);
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static void TestBoundaries()
    {
        Check(new byte[] { 1, 1, 1, 1, 1 }, new byte[] { 1, 1, 1 });
        Check(new byte[] { 1, 2, 1, 2, 1, 2, 1 }, new byte[] { 1, 63, 1, 2, 1 });
        Check(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 1, 63, 3, 4, 5 });
        Check(new byte[] { 1, 2, 3 }, new byte[] { 1, 63, 63 });
        Check(new byte[] { 1, 2, 3, 4, 5 }, new byte[] { 1, 2, 3, 4, 5 });
        Check(new byte[] { 0, 1 }, new byte[] { 1, 63, 2, 3 });
        Check(new byte[0], new byte[] { 1 });
        Check(new byte[] { 1 }, new byte[] { 1, 2 });
        Check(new byte[] { 0, 1, 1 }, new byte[] { 1 });
        Check(new byte[] { 1, 63, 2, 1, 9, 2 }, new byte[] { 1, 63, 2 });
        AssertSequence(new[] { 0 }, FuzzyMatcher.MatchNotReplaced(
            new byte[] { 1, 8, 2, 0, 1, 9, 2 }, new byte[] { 1, 63, 2 }, new byte[] { 1, 9, 2 }), "filter patched candidates");
        AssertSequence(new[] { 0 }, FuzzyMatcher.MatchNotReplaced(
            new byte[] { 1, 2 }, new byte[] { 1, 2 }, new byte[] { 1, 2, 3 }), "longer replacement boundary");
        // 定位串出现在开头时不能产生负偏移；末尾的不完整候选不能越界。
        Check(new byte[] { 2, 3, 4, 1, 0, 2, 3, 4, 1 }, new byte[] { 1, 63, 2, 3, 4 });
        Check(new byte[] { 1, 0, 2, 3, 4 }, new byte[] { 1, 63, 2, 3, 4, 63 });
        Assert(!FuzzyMatcher.IsEqual(new byte[] { 1 }, -1, new byte[] { 1 }), "negative offset");
        Assert(!FuzzyMatcher.IsEqual(new byte[] { 1 }, int.MaxValue, new byte[] { 1 }), "overflow offset");
        Assert(!FuzzyMatcher.IsEqual(new byte[] { 1 }, 0, new byte[] { 1, 63 }), "truncated wildcard tail");
        AssertThrows<ArgumentException>(() => FuzzyMatcher.MatchAll(new byte[] { 1 }, new byte[] { 63, 1 }));
        AssertThrows<ArgumentException>(() => FuzzyMatcher.MatchAll(new byte[] { 1 }, new byte[0]));
        AssertThrows<ArgumentException>(() => BoyerMooreMatcher.MatchAll(new byte[] { 1 }, new byte[0]));
        AssertThrows<ArgumentNullException>(() => FuzzyMatcher.MatchAll(null, new byte[] { 1 }));
        AssertThrows<ArgumentNullException>(() => FuzzyMatcher.MatchAll(new byte[0], null));
        AssertThrows<ArgumentNullException>(() => BoyerMooreMatcher.MatchAll(null, new byte[] { 1 }));
    }

    private static void TestRandomMatches()
    {
        Random random = new Random(20261109);
        for (int trial = 0; trial < 20000; trial++)
        {
            byte[] content = new byte[random.Next(0, 257)];
            byte[] pattern = new byte[random.Next(1, 65)];
            if (trial % 2 == 0)
            {
                random.NextBytes(content);
                random.NextBytes(pattern);
            }
            else
            {
                // 小字母表用于覆盖大量重叠匹配和重复后缀。
                for (int i = 0; i < content.Length; i++) content[i] = (byte)random.Next(0, 4);
                for (int i = 0; i < pattern.Length; i++) pattern[i] = (byte)random.Next(0, 4);
            }
            for (int i = 1; i < pattern.Length; i++)
            {
                if (random.Next(0, 4) == 0) pattern[i] = FuzzyMatcher.wildcard;
            }
            if (pattern[0] == FuzzyMatcher.wildcard) pattern[0] = 1;
            if (content.Length >= pattern.Length && trial % 3 == 0)
            {
                int start = random.Next(content.Length - pattern.Length + 1);
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (pattern[i] != FuzzyMatcher.wildcard) content[start + i] = pattern[i];
                }
            }
            Check(content, pattern);
            byte[] replacement = (byte[])pattern.Clone();
            replacement[0] ^= 128;
            int[] expected = NaiveMatches(content, pattern, true).Where(i => !NaiveEqual(content, i, replacement, true)).ToArray();
            AssertSequence(expected, FuzzyMatcher.MatchNotReplaced(content, pattern, replacement), "not replaced");
            AssertSequence(new int[0], FuzzyMatcher.MatchNotReplaced(content, pattern, pattern), "already replaced");
        }
    }

    private static void Check(byte[] content, byte[] pattern)
    {
        int[] expected = NaiveMatches(content, pattern, true);
        AssertSequence(expected, FuzzyMatcher.MatchAll(content, pattern), "wildcard matches");
        int first;
        bool found = FuzzyMatcher.TryMatch(content, pattern, out first);
        Assert(found == (expected.Length > 0) && first == (expected.Length > 0 ? expected[0] : -1), "wildcard first match");
        expected = NaiveMatches(content, pattern, false);
        AssertSequence(expected, BoyerMooreMatcher.MatchAll(content, pattern), "exact matches");
        found = BoyerMooreMatcher.TryMatch(content, pattern, out first);
        Assert(found == (expected.Length > 0) && first == (expected.Length > 0 ? expected[0] : -1), "exact first match");
    }

    private static int[] NaiveMatches(byte[] content, byte[] pattern, bool wildcard)
    {
        List<int> matches = new List<int>();
        for (int start = 0; start <= content.Length - pattern.Length; start++)
        {
            if (NaiveEqual(content, start, pattern, wildcard)) matches.Add(start);
        }
        return matches.ToArray();
    }

    private static bool NaiveEqual(byte[] content, int start, byte[] pattern, bool wildcard)
    {
        if (start < 0 || start > content.Length - pattern.Length) return false;
        for (int i = 0; i < pattern.Length; i++)
        {
            if ((!wildcard || pattern[i] != 63) && content[start + i] != pattern[i]) return false;
        }
        return true;
    }

    private static void TestConfiguredPatterns(string root)
    {
        string path = Path.Combine(root, @"RevokeMsgPatcher.Assistant\Data\2.1\patch.json");
        JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        int count = CheckConfiguredNode(serializer.DeserializeObject(File.ReadAllText(path)), new Random(42));
        Assert(count > 0, "configured patterns loaded");
        Console.WriteLine("Checked {0} configured search/replacement patterns", count);
    }

    private static int CheckConfiguredNode(object node, Random random)
    {
        int count = 0;
        Dictionary<string, object> map = node as Dictionary<string, object>;
        if (map != null)
        {
            foreach (KeyValuePair<string, object> entry in map)
            {
                if (entry.Key == "Search" || entry.Key == "Replace")
                {
                    byte[] pattern = ((IEnumerable)entry.Value).Cast<object>().Select(Convert.ToByte).ToArray();
                    foreach (int start in new[] { 0, 127, 256 })
                    {
                        byte[] content = new byte[256 + pattern.Length];
                        random.NextBytes(content);
                        for (int i = 0; i < pattern.Length; i++)
                        {
                            if (pattern[i] != 63) content[start + i] = pattern[i];
                        }
                        Check(content, pattern);
                    }
                    count++;
                }
                else count += CheckConfiguredNode(entry.Value, random);
            }
        }
        else
        {
            object[] items = node as object[];
            if (items != null)
            {
                foreach (object item in items) count += CheckConfiguredNode(item, random);
            }
        }
        return count;
    }

    private static void TestModifyFinder()
    {
        List<ReplacePattern> patterns = new List<ReplacePattern>
        {
            new ReplacePattern { Search = new byte[] { 1, 63, 2 }, Replace = new byte[] { 3, 63, 2 }, Category = "功能一" },
            new ReplacePattern { Search = new byte[] { 4, 63, 5 }, Replace = new byte[] { 6, 63, 5 }, Category = "功能二" }
        };
        string path = Path.GetTempFileName();
        TextWriter originalOutput = Console.Out;
        try
        {
            Console.SetOut(TextWriter.Null);
            File.WriteAllBytes(path, new byte[] { 1, 9, 2, 0, 4, 9, 5 });
            List<Change> changes = ModifyFinder.FindChanges(path, patterns);
            Assert(changes.Count == 2 && changes[0].Position == 0 && changes[1].Position == 4, "patch offsets");
            AssertSequence(patterns[0].Replace.Select(b => (int)b).ToArray(), changes[0].Content.Select(b => (int)b).ToArray(), "replacement bytes");
            Assert(ModifyFinder.FindReplacedFunction(path, patterns).Count == 0, "unpatched functions");
            File.WriteAllBytes(path, new byte[] { 3, 9, 2, 0, 6, 9, 5 });
            Assert(ModifyFinder.FindReplacedFunction(path, patterns).SetEquals(new[] { "功能一", "功能二" }), "patched functions");
            AssertBusinessError(() => ModifyFinder.FindChanges(path, patterns), "match_already_replace");
            File.WriteAllBytes(path, new byte[] { 3, 9, 2, 0, 4, 9, 5 });
            Assert(ModifyFinder.FindReplacedFunction(path, patterns).SetEquals(new[] { "功能一" }), "partially patched functions");
            AssertBusinessError(() => ModifyFinder.FindChanges(path, patterns), "match_inconformity");
            File.WriteAllBytes(path, new byte[] { 0 });
            AssertBusinessError(() => ModifyFinder.FindChanges(path, patterns), "match_inconformity");
            Assert(ModifyFinder.FindChanges(path, new List<ReplacePattern>()).Count == 0, "empty pattern list");
        }
        finally
        {
            Console.SetOut(originalOutput);
            File.Delete(path);
        }
    }

    private static void AssertBusinessError(Action action, string code)
    {
        try { action(); }
        catch (BusinessException error) { Assert(error.ErrorCode == code, "business error " + code); return; }
        throw new Exception("Expected business error " + code);
    }

    private static void TestAllocation()
    {
        AppDomain.MonitoringIsEnabled = true;
        byte[] content = new byte[8 * 1024 * 1024];
        byte[] pattern = new byte[] { 0, 63, 1, 2, 3, 4 };
        FuzzyMatcher.MatchAll(new byte[64], pattern);
        long before = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
        int[] matches = FuzzyMatcher.MatchAll(content, pattern);
        long allocated = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize - before;
        Assert(matches.Length == 0, "large buffer without matches");
        // 大文件的无命中扫描不应再按文件大小分配结果数组或候选数组。
        Assert(allocated < 1024 * 1024, "allocation regression: " + allocated + " bytes");
    }

    private static void TestQqntAdaptiveLocator()
    {
        Assert(QQNTAdaptiveLocator.ReplayGroupBranch(), "adaptive group replay");
        Assert(QQNTAdaptiveLocator.ReplayPrivateBranch(), "adaptive private replay");
        string sample = Environment.GetEnvironmentVariable("QQNT_WRAPPER_TEST");
        if (!string.IsNullOrEmpty(sample) && File.Exists(sample))
        {
            string expectedError = Environment.GetEnvironmentVariable("QQNT_WRAPPER_EXPECT_ERROR");
            QQNTAdaptiveResult result = null;
            try
            {
                result = QQNTAdaptiveLocator.Analyze(sample);
            }
            catch (BusinessException error)
            {
                if (!string.IsNullOrEmpty(expectedError))
                {
                    Assert(error.ErrorCode == expectedError, "adaptive expected error");
                    return;
                }
                throw;
            }
            Assert(string.IsNullOrEmpty(expectedError), "adaptive expected an error");
            Assert(result.GroupBranchOffset > 0, "adaptive group location");
            Assert(result.PrivateBranchOffset > 0, "adaptive private location");
            string expectedGroupOffset = Environment.GetEnvironmentVariable("QQNT_WRAPPER_EXPECT_GROUP_OFFSET");
            string expectedPrivateOffset = Environment.GetEnvironmentVariable("QQNT_WRAPPER_EXPECT_PRIVATE_OFFSET");
            if (!string.IsNullOrEmpty(expectedGroupOffset))
                Assert(result.GroupBranchOffset == Convert.ToInt64(expectedGroupOffset, 16), "adaptive group offset");
            if (!string.IsNullOrEmpty(expectedPrivateOffset))
                Assert(result.PrivateBranchOffset == Convert.ToInt64(expectedPrivateOffset, 16), "adaptive private offset");
            Assert(result.IsPatched || result.Changes.Count == 2, "adaptive patch plan");
            string expectedPatched = Environment.GetEnvironmentVariable("QQNT_WRAPPER_EXPECT_PATCHED");
            if (!string.IsNullOrEmpty(expectedPatched))
            {
                bool expected = expectedPatched == "1";
                Assert(result.IsPatched == expected, "adaptive patched state");
                Assert(!expected || result.Changes.Count == 0, "patched image has no pending changes");
            }
        }
    }

    private static void AssertThrows<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { assertions++; return; }
        throw new Exception("Expected " + typeof(T).Name);
    }

    private static void AssertSequence(int[] expected, int[] actual, string message)
    {
        Assert(expected.SequenceEqual(actual), message + ": expected [" + string.Join(",", expected) + "] actual [" + string.Join(",", actual) + "]");
    }

    private static void Assert(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception(message);
    }
}
