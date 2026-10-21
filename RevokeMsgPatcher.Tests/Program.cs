using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using RevokeMsgPatcher;
using RevokeMsgPatcher.Matcher;
using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Modifier;
using RevokeMsgPatcher.Utils;

internal static class Program
{
    private static int passed;
    private static string temp;
    private static Bag bag;
    private static readonly JavaScriptSerializer Json = new JavaScriptSerializer();

    [STAThread]
    private static int Main(string[] args)
    {
        temp = Path.Combine(Path.GetTempPath(), "RevokeMsgPatcher-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            string embedded = new ResourceManager("RevokeMsgPatcher.Properties.Resources", typeof(App).Assembly).GetString("PatchJson");
            bag = Json.Deserialize<Bag>(embedded);
            Test("Generated rules equal embedded rules", () => Assert(embedded == new JsonData().BagJson()));
            UnitTests();
            if (args.Length > 0)
            {
                if (args.Length != 4 || (args[0] != "--local" && args[0] != "--probe"))
                    throw new ArgumentException("Usage: RevokeMsgPatcher.Tests.exe [--probe|--local QQ_ROOT WEIXIN_ROOT REPORT.json]");
                bool roundTrip = args[0] == "--local";
                var reports = new[] { Probe("QQNT", args[1], roundTrip), Probe("Weixin", args[2], roundTrip) };
                File.WriteAllText(Path.GetFullPath(args[3]), Json.Serialize(new {
                    CheckedAtUtc = DateTime.UtcNow.ToString("o"), TestsPassed = passed,
                    RuntimeRecallTested = false, InstallationsModified = false, Results = reports
                }));
            }
            Console.WriteLine("PASS: " + passed + " checks. Client installations were not modified.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally { Directory.Delete(temp, true); }
    }

    private static void Assert(bool value) { if (!value) throw new Exception("Assertion failed"); }
    private static void Test(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }
    private static ReplacePattern Pattern(string before, string after, string category = "test", int expected = 0)
    {
        return new ReplacePattern { Search = ByteUtil.HexStringToByteArray(before), Replace = ByteUtil.HexStringToByteArray(after), Category = category, ExpectedMatches = expected };
    }
    private static string Fixture(string bytes)
    {
        string path = Path.Combine(temp, Guid.NewGuid().ToString("N") + ".bin");
        File.WriteAllBytes(path, ByteUtil.HexStringToByteArray(bytes)); return path;
    }
    private static void Reject(Action action, string code)
    {
        try { action(); } catch (BusinessException ex) { Assert(ex.ErrorCode == code); return; }
        throw new Exception("Expected rejection: " + code);
    }
    private static AppModifier Modifier(string name)
    {
        var type = typeof(App).Assembly.GetType("RevokeMsgPatcher.Modifier." + (name == "QQNT" ? "QQNT" : "Weixin") + "Modifier");
        return (AppModifier)Activator.CreateInstance(type, new object[] { bag.Apps[name] });
    }

    private static void UnitTests()
    {
        Test("Wildcard match at EOF stays in bounds", () => {
            Assert(FuzzyMatcher.MatchAll(new byte[] { 1, 2, 1 }, new byte[] { 1, 63, 3 }).Length == 0);
            Assert(!FuzzyMatcher.IsEqual(new byte[] { 1 }, 0, new byte[] { 1, 63 }));
            Assert(FuzzyMatcher.MatchAll(new byte[] { 1, 2 }, new byte[] { 1, 63 }).SequenceEqual(new[] { 0 }));
        });
        Test("Duplicate hits cannot mask a missing rule", () => Reject(() => ModifyFinder.FindChanges(Fixture("AA BB AA BB"), new List<ReplacePattern> {
            Pattern("AA BB", "AA CC"), Pattern("DD EE", "DD FF") }), "match_inconformity"));
        Test("Explicit count rejects ambiguous matches", () => Reject(() => ModifyFinder.FindChanges(Fixture("AA BB AA BB"), new List<ReplacePattern> {
            Pattern("AA BB", "AA CC", expected: 1) }), "match_inconformity"));
        Test("Legacy repeated matches remain supported", () => Assert(ModifyFinder.FindChanges(Fixture("AA BB AA BB"), new List<ReplacePattern> {
            Pattern("AA BB", "AA CC") }).Count == 2));
        Test("A partially patched feature is not installed or available", () => {
            var state = ModifyFinder.InspectFunctions(Fixture("AA CC DD EE"), new List<ReplacePattern> {
                Pattern("AA BB", "AA CC"), Pattern("DD EE", "DD FF") });
            Assert(state.Item1.Count == 0 && state.Item2.Count == 0);
        });
        Test("All rules patched reports already installed", () => Reject(() => ModifyFinder.FindChanges(Fixture("AA CC DD FF"), new List<ReplacePattern> {
            Pattern("AA BB", "AA CC"), Pattern("DD EE", "DD FF") }), "match_already_replace"));
        Test("Overlapping changes are rejected", () => Reject(() => ModifyFinder.FindChanges(Fixture("AA BB CC"), new List<ReplacePattern> {
            Pattern("AA BB", "AA DD"), Pattern("BB CC", "EE CC") }), "overlapping_patterns"));
        Test("An empty selection cannot silently succeed", () => Reject(() => ModifyFinder.FindChanges(Fixture("AA BB"), new List<ReplacePattern>()), "empty_patterns"));
        Test("Invalid pattern lengths are rejected", () => Reject(() => ModifyFinder.FindChanges(Fixture("AA BB"), new List<ReplacePattern> {
            Pattern("AA BB", "AA") }), "invalid_pattern"));
        Test("Hashing is read only and accepts a read-only file", () => {
            string path = Fixture("AA BB"); File.SetAttributes(path, FileAttributes.ReadOnly);
            try { Assert(FileUtil.ComputeFileSHA1(path).Length == 40); Assert((File.GetAttributes(path) & FileAttributes.ReadOnly) != 0); }
            finally { File.SetAttributes(path, FileAttributes.Normal); }
        });
        Test("Backup patch restore preserves wildcard bytes", () => {
            string path = Fixture("AA 12 CC"); var patterns = new List<ReplacePattern> { Pattern("AA 3F CC", "BB 3F DD", expected: 1) };
            var editor = Editor(path); string before = editor.FileSHA1;
            editor.TargetChanges = ModifyFinder.FindChanges(path, patterns); editor.Backup(); editor.Patch();
            Assert(File.ReadAllBytes(path).SequenceEqual(new byte[] { 0xBB, 0x12, 0xDD }));
            Assert(ModifyFinder.FindReplacedFunction(path, patterns).Contains("test"));
            editor.Restore(); Assert(FileUtil.ComputeFileSHA1(path) == before);
        });
        Test("A changed file cannot be patched using stale verification", () => {
            string path = Fixture("AA BB"); var editor = Editor(path); string before = editor.FileSHA1;
            editor.TargetChanges = new List<Change> { new Change(0, new byte[] { 0xCC }) };
            File.WriteAllBytes(path, new byte[] { 1, 2 });
            Reject(() => editor.Patch(), "file_changed"); Assert(File.ReadAllBytes(path).SequenceEqual(new byte[] { 1, 2 }));
        });
        Test("A locked file survives failed replacement", () => {
            string path = Fixture("AA BB"); var editor = Editor(path); string before = editor.FileSHA1;
            editor.TargetChanges = new List<Change> { new Change(0, new byte[] { 0xCC }) };
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                bool failed = false; try { editor.Patch(); } catch (IOException) { failed = true; }
                Assert(failed); Assert(FileUtil.ComputeFileSHA1(path) == before);
            }
            Assert(!Directory.GetFiles(temp, "*.h.process.*").Any());
        });
        Test("Target cloning retains version metadata", () => {
            var target = new TargetInfo { Name = "a", RelativePath = "a", StartVersion = "1.0", EndVersion = "2.0", RelativePathForVersion = "b" };
            var clone = target.Clone(); Assert(clone.StartVersion == "1.0" && clone.EndVersion == "2.0" && clone.RelativePathForVersion == "b");
        });
        Test("QQ selects highest complete version regardless of timestamps", () => {
            string root = Path.Combine(temp, "QQ");
            foreach (string version in new[] { "9.9.9-99999", "9.9.33-51802" }) {
                string app = Path.Combine(root, "versions", version, "resources", "app"); Directory.CreateDirectory(app); File.WriteAllText(Path.Combine(app, "wrapper.node"), "fixture");
            }
            Directory.CreateDirectory(Path.Combine(root, "versions", "9.9.99-99999"));
            var target = bag.Apps["QQNT"].FileTargetInfos["wrapper.node"];
            string expected = Path.Combine(root, "versions", "9.9.33-51802", "resources", "app", "wrapper.node");
            // Upgrade leftovers must not override the selected versioned module.
            Directory.CreateDirectory(Path.Combine(root, "resources", "app"));
            File.WriteAllText(Path.Combine(root, "resources", "app", "wrapper.node"), "old fixture");
            File.WriteAllText(Path.Combine(root, "wrapper.node"), "old fixture");
            Assert(target.GetAbsolutePath(root) == expected);
            Assert(target.GetAbsolutePath(Path.Combine(root, "versions", "9.9.33-51802")) == expected);
            Assert(target.GetAbsolutePath(Path.GetDirectoryName(expected)) == expected);
            Assert(target.GetAbsolutePath(Path.Combine(root, "missing")) == "");
        });
        Test("Weixin accepts root, version folder, executable and absent path", () => {
            string root = Path.Combine(temp, "Weixin");
            foreach (string version in new[] { "4.1.9.99", "4.1.15.56" }) {
                string folder = Path.Combine(root, version); Directory.CreateDirectory(folder); File.WriteAllText(Path.Combine(folder, "Weixin.dll"), "fixture");
            }
            var modifier = Modifier("Weixin"); string expected = Path.Combine(root, "4.1.15.56");
            Assert(modifier.ResolveInstallPath(root) == expected); Assert(modifier.IsAllFilesExist(root));
            Assert(modifier.ResolveInstallPath(expected) == expected);
            Assert(modifier.ResolveInstallPath(Path.Combine(root, "missing")) == null);
            File.WriteAllText(Path.Combine(expected, "Weixin.exe"), "fixture");
            Assert(PathUtil.NormalizeInstallPath("\"" + Path.Combine(expected, "Weixin.exe") + "\" /silent") == expected);
        });
    }

    private static FileHexEditor Editor(string path)
    {
        return new FileHexEditor(Path.GetDirectoryName(path), new TargetInfo { Name = Path.GetFileName(path), RelativePath = Path.GetFileName(path) });
    }
    private static string Sha256(string path)
    {
        using (var stream = File.OpenRead(path)) using (var hash = SHA256.Create())
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }
    private static object Probe(string name, string root, bool roundTrip)
    {
        var modifier = Modifier(name);
        Assert(modifier.IsAllFilesExist(root)); Assert(modifier.InitEditors(root));
        string resolved = modifier.InstallPath;
        var target = bag.Apps[name].FileTargetInfos.Values.Single();
        string path = target.GetAbsolutePath(resolved); string beforeHash = Sha256(path);
        var attributes = File.GetAttributes(path); var writeTime = File.GetLastWriteTimeUtc(path);
        string version = FileUtil.GetFileVersion(path);
        var rule = bag.Apps[name].FileCommonModifyInfos[target.Name].First(r => modifier.IsInVersionRange(version, r.StartVersion, r.EndVersion));
        var matchDetails = new List<object>(); byte[] bytes = File.ReadAllBytes(path);
        foreach (var pattern in rule.ReplacePatterns)
        {
            int[] original = FuzzyMatcher.MatchAll(bytes, pattern.Search);
            int[] installed = FuzzyMatcher.MatchAll(bytes, pattern.Replace);
            Test(name + " unique " + pattern.Category + " pattern", () =>
                Assert((original.Length == 1 && installed.Length == 0) || (original.Length == 0 && installed.Length == 1)));
            matchDetails.Add(new { pattern.Category, pattern.ExpectedMatches, OriginalCount = original.Length, PatchedCount = installed.Length, Offsets = original.Select(v => "0x" + v.ToString("X")).ToArray() });
        }
        var functions = ModifyFinder.InspectFunctions(path, rule.ReplacePatterns);
        Test(name + " real modifier validates current patch state", () => {
            Assert(functions.Item1.Count + functions.Item2.Count == rule.GetCategories().Distinct().Count());
            if (functions.Item1.Count > 0) modifier.ValidateAndFindModifyInfo(functions.Item1.ToList());
            else Reject(() => modifier.ValidateAndFindModifyInfo(rule.GetCategories().Distinct().ToList()), "match_already_replace");
        });
        Test(name + " UI offers matching functions", () => {
            using (var label = new Label()) using (var panel = new Panel()) {
                modifier.SetVersionLabelAndCategoryCategories(label, panel);
                Assert(panel.Controls.OfType<CheckBox>().Count() == rule.GetCategories().Distinct().Count());
            }
        });
        string auto = modifier.FindInstallPath();
        Test(name + " automatic discovery resolves this installation", () => Assert(auto != null &&
            string.Equals(target.GetAbsolutePath(modifier.ResolveInstallPath(auto)), path, StringComparison.OrdinalIgnoreCase)));
        if (roundTrip)
        {
            // Installed clients remain untouched. Use their original backup as the test fixture.
            string sample = functions.Item2.Count == 0 ? path : path + ".h.bak";
            if (!File.Exists(sample) || FileUtil.GetFileVersion(sample) != version)
                throw new InvalidOperationException("An original backup with the same version is required: " + sample);
            var originalState = ModifyFinder.InspectFunctions(sample, rule.ReplacePatterns);
            Assert(originalState.Item2.Count == 0 && originalState.Item1.SetEquals(rule.GetCategories()));
            string sampleHash = Sha256(sample);
            string folder = Path.Combine(temp, name); Directory.CreateDirectory(folder);
            string copy = Path.Combine(folder, target.Name); File.Copy(sample, copy);
            var editor = Editor(copy); string originalHash = editor.FileSHA1;
            var patterns = rule.ReplacePatterns;
            Test(name + " copy patch/install-state/restore round trip", () => {
                editor.TargetChanges = ModifyFinder.FindChanges(copy, patterns); editor.Backup(); editor.Patch();
                Assert(Sha256(copy) != sampleHash); Assert(Sha256(editor.FileBakPath) == sampleHash);
                var state = ModifyFinder.InspectFunctions(copy, patterns);
                Assert(state.Item1.Count == 0 && state.Item2.SetEquals(patterns.Select(p => p.Category)));
                Reject(() => ModifyFinder.FindChanges(copy, patterns), "match_already_replace");
                editor.Restore(); Assert(Sha256(copy) == sampleHash); Assert(Sha256(sample) == sampleHash);
            });
        }
        Test(name + " installed binary is unchanged", () => Assert(Sha256(path) == beforeHash && File.GetAttributes(path) == attributes && File.GetLastWriteTimeUtc(path) == writeTime));
        return new { Name = name, Root = root, ResolvedPath = path, DisplayVersion = modifier.GetVersion(), ModuleVersion = version,
            Sha256 = beforeHash, Patterns = matchDetails, InstalledFunctions = functions.Item2.ToArray(),
            CopyRoundTripPassed = roundTrip, RuntimeRecallTested = false };
    }
}
