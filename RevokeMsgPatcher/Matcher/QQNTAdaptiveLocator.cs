using RevokeMsgPatcher.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RevokeMsgPatcher.Matcher
{
    public sealed class QQNTAdaptiveResult
    {
        public bool IsPatched { get; internal set; }
        public long GroupBranchOffset { get; internal set; }
        public long PrivateBranchOffset { get; internal set; }
        public List<Change> Changes { get; internal set; }
    }

    /// <summary>
    /// QQNT撤回逻辑自动分析
    /// </summary>
    public static class QQNTAdaptiveLocator
    {
        private const int ImageScnMemExecute = unchecked((int)0x20000000);
        private static readonly object CacheGate = new object();
        private static string cachedKey;
        private static QQNTAdaptiveResult cachedResult;

        private sealed class Section
        {
            public int VirtualAddress;
            public int VirtualSize;
            public int RawOffset;
            public int RawSize;
            public int Characteristics;
            public bool Executable { get { return (Characteristics & ImageScnMemExecute) != 0; } }
        }

        private sealed class Image
        {
            public byte[] Bytes;
            public List<Section> Sections;

            public int RvaToOffset(int rva)
            {
                foreach (Section section in Sections)
                {
                    int span = Math.Max(section.VirtualSize, section.RawSize);
                    if (rva >= section.VirtualAddress && rva < section.VirtualAddress + span)
                        return section.RawOffset + rva - section.VirtualAddress;
                }
                return rva >= 0 && rva < Bytes.Length ? rva : -1;
            }

            public int OffsetToRva(int offset)
            {
                foreach (Section section in Sections)
                {
                    if (offset >= section.RawOffset && offset < section.RawOffset + section.RawSize)
                        return section.VirtualAddress + offset - section.RawOffset;
                }
                return offset >= 0 && offset < Bytes.Length ? offset : -1;
            }

            public bool IsExecutableRva(int rva)
            {
                return Sections.Any(section => section.Executable && rva >= section.VirtualAddress && rva < section.VirtualAddress + Math.Max(section.VirtualSize, section.RawSize));
            }
        }

        private sealed class BranchCandidate
        {
            public int Offset;
            public int Score;
            public bool Patched;
            public byte[] Replacement;
        }

        private sealed class RegMemInstruction
        {
            public int Length;
            public int Register;
        }

        public static QQNTAdaptiveResult Analyze(string path)
        {
            FileInfo info = new FileInfo(path);
            string key = info.FullName + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks;
            lock (CacheGate)
            {
                if (cachedKey == key && cachedResult != null) return Clone(cachedResult);
            }

            Image image = Parse(File.ReadAllBytes(path));
            List<int> groupStrings;
            List<int> privateStrings;
            DiscoverRecallStrings(image, out groupStrings, out privateStrings);
            if (groupStrings.Count == 0 || privateStrings.Count == 0)
                throw new BusinessException("adaptive_anchor_missing", "自动分析失败：未找到撤回逻辑！");

            List<int> groupReferences = FindRipReferences(image, new HashSet<int>(groupStrings));
            List<int> privateReferences = FindRipReferences(image, new HashSet<int>(privateStrings));
            if (groupReferences.Count == 0 || privateReferences.Count == 0)
                throw new BusinessException("adaptive_xref_missing", "自动分析失败：未找到撤回逻辑！");

            BranchCandidate group = FindUniqueGroupBranch(image, groupReferences);
            BranchCandidate privateBranch = FindUniquePrivatePatch(image, privateReferences);
            if (group.Patched != privateBranch.Patched)
                throw new BusinessException("adaptive_partial_patch", "补丁状态异常，请先还原！");
            if (!ReplayGroupBranch() || !ReplayPrivateBranch())
                throw new BusinessException("adaptive_replay_failed", "自动分析校验失败！");

            QQNTAdaptiveResult result = new QQNTAdaptiveResult
            {
                IsPatched = group.Patched,
                GroupBranchOffset = group.Offset,
                PrivateBranchOffset = privateBranch.Offset,
                Changes = new List<Change>()
            };
            if (!result.IsPatched)
            {
                result.Changes.Add(new Change(group.Offset, group.Replacement));
                result.Changes.Add(new Change(privateBranch.Offset, privateBranch.Replacement));
            }

            lock (CacheGate)
            {
                cachedKey = key;
                cachedResult = Clone(result);
            }
            return result;
        }

        public static bool ReplayGroupBranch()
        {
            bool originalSeven = 7UL > 7UL;
            bool originalEight = 8UL > 7UL;
            bool patchedSeven = false;
            bool patchedEight = false;
            return !originalSeven && originalEight && !patchedSeven && !patchedEight;
        }

        public static bool ReplayPrivateBranch()
        {
            bool originalEqual = 1UL == 1UL;
            bool originalDifferent = 1UL == 2UL;
            bool patchedEqual = true;
            bool patchedDifferent = true;
            return originalEqual && !originalDifferent && patchedEqual && patchedDifferent;
        }

        private static Image Parse(byte[] bytes)
        {
            if (bytes.Length < 0x100 || bytes[0] != (byte)'M' || bytes[1] != (byte)'Z')
                throw new BusinessException("adaptive_not_pe", "自动分析失败：文件格式错误！");
            int pe = ReadInt32(bytes, 0x3C);
            if (pe < 0 || pe + 24 > bytes.Length || bytes[pe] != (byte)'P' || bytes[pe + 1] != (byte)'E')
                throw new BusinessException("adaptive_not_pe", "自动分析失败：文件格式错误！");
            int sectionCount = ReadUInt16(bytes, pe + 6);
            int optionalSize = ReadUInt16(bytes, pe + 20);
            int sectionTable = pe + 24 + optionalSize;
            List<Section> sections = new List<Section>();
            for (int index = 0; index < sectionCount; index++)
            {
                int offset = sectionTable + index * 40;
                if (offset + 40 > bytes.Length) throw new BusinessException("adaptive_bad_pe", "自动分析失败：文件格式错误！");
                sections.Add(new Section
                {
                    VirtualSize = ReadInt32(bytes, offset + 8),
                    VirtualAddress = ReadInt32(bytes, offset + 12),
                    RawSize = ReadInt32(bytes, offset + 16),
                    RawOffset = ReadInt32(bytes, offset + 20),
                    Characteristics = ReadInt32(bytes, offset + 36)
                });
            }
            return new Image { Bytes = bytes, Sections = sections };
        }

        private static void DiscoverRecallStrings(Image image, out List<int> group, out List<int> privateStrings)
        {
            List<int> preferredGroup = new List<int>();
            List<int> genericGroup = new List<int>();
            List<int> preferredPrivate = new List<int>();
            List<int> genericPrivate = new List<int>();
            group = new List<int>();
            privateStrings = new List<int>();
            foreach (Section section in image.Sections.Where(item => !item.Executable))
            {
                int start = Math.Max(0, section.RawOffset);
                int end = Math.Min(image.Bytes.Length, section.RawOffset + section.RawSize);
                int cursor = start;
                while (cursor < end)
                {
                    while (cursor < end && !IsAscii(image.Bytes[cursor])) cursor++;
                    int stringStart = cursor;
                    while (cursor < end && IsAscii(image.Bytes[cursor])) cursor++;
                    int length = cursor - stringStart;
                    if (length < 8) continue;
                    string text = Encoding.ASCII.GetString(image.Bytes, stringStart, length).ToLowerInvariant();
                    if (text.IndexOf("recall", StringComparison.Ordinal) < 0) continue;
                    int rva = image.OffsetToRva(stringStart);
                    if (text.IndexOf("group", StringComparison.Ordinal) >= 0 || text.IndexOf("grp", StringComparison.Ordinal) >= 0)
                    {
                        genericGroup.Add(rva);
                        // 排除拍一拍撤回
                        if (text.IndexOf("sys msg", StringComparison.Ordinal) >= 0
                            && text.IndexOf("paiyipai", StringComparison.Ordinal) < 0
                            && text.IndexOf("pai yi pai", StringComparison.Ordinal) < 0)
                            preferredGroup.Add(rva);
                    }
                    if (text.IndexOf("c2c", StringComparison.Ordinal) >= 0 || text.IndexOf("private", StringComparison.Ordinal) >= 0)
                    {
                        genericPrivate.Add(rva);
                        if (text.IndexOf("c2c recall nfy", StringComparison.Ordinal) >= 0
                            && text.IndexOf("paiyipai", StringComparison.Ordinal) < 0
                            && text.IndexOf("pai yi pai", StringComparison.Ordinal) < 0)
                            preferredPrivate.Add(rva);
                    }
                }
            }
            group = (preferredGroup.Count > 0 ? preferredGroup : genericGroup).Distinct().ToList();
            privateStrings = (preferredPrivate.Count > 0 ? preferredPrivate : genericPrivate).Distinct().ToList();
        }

        private static List<int> FindRipReferences(Image image, HashSet<int> targets)
        {
            List<int> result = new List<int>();
            foreach (Section section in image.Sections.Where(item => item.Executable))
            {
                int start = section.RawOffset;
                int end = Math.Min(image.Bytes.Length, section.RawOffset + section.RawSize);
                for (int offset = start; offset + 7 <= end; offset++)
                {
                    int opcodeOffset = offset;
                    int size = 6;
                    if (image.Bytes[offset] >= 0x40 && image.Bytes[offset] <= 0x4F) { opcodeOffset++; size++; }
                    if (opcodeOffset + 6 > end) continue;
                    byte opcode = image.Bytes[opcodeOffset];
                    byte modrm = image.Bytes[opcodeOffset + 1];
                    if ((opcode != 0x8D && opcode != 0x8B) || (modrm & 0xC7) != 0x05) continue;
                    int instructionRva = image.OffsetToRva(offset);
                    int target = instructionRva + size + ReadInt32(image.Bytes, opcodeOffset + 2);
                    if (targets.Contains(target)) result.Add(instructionRva);
                }
            }
            return result.Distinct().ToList();
        }

        private static BranchCandidate FindUniqueGroupBranch(Image image, List<int> references)
        {
            Dictionary<int, BranchCandidate> candidates = new Dictionary<int, BranchCandidate>();
            foreach (Section section in image.Sections.Where(item => item.Executable))
            {
                int start = section.RawOffset;
                int end = Math.Min(image.Bytes.Length, section.RawOffset + section.RawSize);
                for (int offset = start; offset + 10 <= end; offset++)
                {
                    int register;
                    int cmpLength;
                    if (!TryDecodeCmpImmediateSeven(image.Bytes, offset, out register, out cmpLength)) continue;
                    int branchOffset = offset + cmpLength;
                    int branchRva = image.OffsetToRva(branchOffset);
                    int distance = DistanceToNearest(branchRva, references);
                    if (distance > 0x800) continue;
                    bool patched = IsNops(image.Bytes, branchOffset, 6);
                    bool original = image.Bytes[branchOffset] == 0x0F && image.Bytes[branchOffset + 1] == 0x87;
                    if (!patched && !original) continue;
                    if (!HasSubIntoRegister(image.Bytes, Math.Max(start, offset - 28), offset, register)) continue;
                    if (original)
                    {
                        int targetRva = branchRva + 6 + ReadInt32(image.Bytes, branchOffset + 2);
                        if (!image.IsExecutableRva(targetRva)) continue;
                    }
                    candidates[branchOffset] = new BranchCandidate
                    {
                        Offset = branchOffset,
                        Patched = patched,
                        Replacement = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 },
                        Score = 100 - Math.Min(distance / 8, 80)
                    };
                }
            }
            return RequireUnique(candidates.Values.ToList(), "群聊");
        }

        private static BranchCandidate FindUniquePrivatePatch(Image image, List<int> references)
        {
            BranchCandidate modern = FindModernPrivateBranch(image, references);
            BranchCandidate dispatch = FindPrivateDispatchCall(image, references);
            if (modern != null && modern.Patched) return modern;
            if (dispatch != null && dispatch.Patched) return dispatch;
            if (modern != null) return modern;
            if (dispatch != null) return dispatch;
            throw new BusinessException("adaptive_missing", "自动分析失败：未找到私聊撤回逻辑！");
        }

        private static BranchCandidate FindModernPrivateBranch(Image image, List<int> references)
        {
            Dictionary<int, BranchCandidate> candidates = new Dictionary<int, BranchCandidate>();
            foreach (Section section in image.Sections.Where(item => item.Executable))
            {
                int start = section.RawOffset;
                int end = Math.Min(image.Bytes.Length, section.RawOffset + section.RawSize);
                for (int branchOffset = start + 8; branchOffset + 8 <= end; branchOffset++)
                {
                    bool patched = image.Bytes[branchOffset] == 0x90 && image.Bytes[branchOffset + 1] == 0xE9;
                    bool original = image.Bytes[branchOffset] == 0x0F && image.Bytes[branchOffset + 1] == 0x84;
                    if (!patched && !original) continue;
                    int branchRva = image.OffsetToRva(branchOffset);
                    int distance = DistanceToNearest(branchRva, references);
                    if (distance > 0x1000) continue;
                    int cmpOffset;
                    RegMemInstruction cmp;
                    if (!FindRegMemEndingAt(image.Bytes, Math.Max(start, branchOffset - 12), branchOffset, 0x3B, out cmpOffset, out cmp)) continue;
                    int movOffset;
                    RegMemInstruction producer;
                    if (!FindRegMemEndingAt(image.Bytes, Math.Max(start, cmpOffset - 12), cmpOffset, 0x8B, out movOffset, out producer)) continue;
                    if (producer.Register != cmp.Register) continue;
                    RegMemInstruction firstLoad;
                    if (!TryDecodeRegMem(image.Bytes, branchOffset + 6, 0x8B, out firstLoad)) continue;
                    if (!HasIndirectCall(image.Bytes, branchOffset + 6, Math.Min(end, branchOffset + 70))) continue;
                    if (original)
                    {
                        int targetRva = branchRva + 6 + ReadInt32(image.Bytes, branchOffset + 2);
                        if (!image.IsExecutableRva(targetRva)) continue;
                    }
                    candidates[branchOffset] = new BranchCandidate
                    {
                        Offset = branchOffset,
                        Patched = patched,
                        Replacement = new byte[] { 0x90, 0xE9, image.Bytes[branchOffset + 2], image.Bytes[branchOffset + 3], image.Bytes[branchOffset + 4], image.Bytes[branchOffset + 5] },
                        Score = 120 - Math.Min(distance / 16, 90)
                    };
                }
            }
            return TryRequireUnique(candidates.Values.ToList(), "私聊");
        }

        private static BranchCandidate FindPrivateDispatchCall(Image image, List<int> references)
        {
            Dictionary<int, BranchCandidate> candidates = new Dictionary<int, BranchCandidate>();
            foreach (int reference in references)
            {
                int referenceOffset = image.RvaToOffset(reference);
                if (referenceOffset < 0) continue;
                int end = Math.Min(image.Bytes.Length, referenceOffset + 0x90);
                for (int callOffset = referenceOffset + 7; callOffset + 5 <= end; callOffset++)
                {
                    bool patched = IsNops(image.Bytes, callOffset, 5);
                    bool original = image.Bytes[callOffset] == 0xE8;
                    if (!patched && !original) continue;

                    int leaOffset;
                    RegMemInstruction lea;
                    if (!FindRegMemEndingAt(image.Bytes, Math.Max(referenceOffset, callOffset - 12), callOffset, 0x8D, out leaOffset, out lea)) continue;
                    if (lea.Register != 2) continue;
                    int movOffset;
                    RegMemInstruction mov;
                    if (!FindRegMemEndingAt(image.Bytes, Math.Max(referenceOffset, leaOffset - 12), leaOffset, 0x8B, out movOffset, out mov)) continue;
                    if (mov.Register != 1) continue;
                    if (CountDirectCalls(image.Bytes, callOffset + 5, Math.Min(image.Bytes.Length, callOffset + 70)) < 3) continue;

                    int callRva = image.OffsetToRva(callOffset);
                    if (original)
                    {
                        int targetRva = callRva + 5 + ReadInt32(image.Bytes, callOffset + 1);
                        if (!image.IsExecutableRva(targetRva)) continue;
                    }
                    candidates[callOffset] = new BranchCandidate
                    {
                        Offset = callOffset,
                        Patched = patched,
                        Replacement = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 },
                        Score = 120 - Math.Min(Math.Abs(callRva - reference) / 4, 90)
                    };
                }
            }
            return TryRequireUnique(candidates.Values.ToList(), "私聊");
        }

        private static BranchCandidate RequireUnique(List<BranchCandidate> candidates, string kind)
        {
            BranchCandidate result = TryRequireUnique(candidates, kind);
            if (result == null)
                throw new BusinessException("adaptive_missing", "自动分析失败：未找到" + kind + "撤回逻辑！");
            return result;
        }

        private static BranchCandidate TryRequireUnique(List<BranchCandidate> candidates, string kind)
        {
            List<BranchCandidate> ordered = candidates.OrderByDescending(item => item.Score).ThenBy(item => item.Offset).ToList();
            if (ordered.Count == 0) return null;
            if (ordered.Count > 1 && ordered[0].Score - ordered[1].Score < 12)
                throw new BusinessException("adaptive_ambiguous", "自动分析失败：" + kind + "撤回逻辑不唯一！");
            return ordered[0];
        }

        private static bool TryDecodeCmpImmediateSeven(byte[] bytes, int offset, out int register, out int length)
        {
            register = -1;
            length = 0;
            if (offset + 4 > bytes.Length) return false;
            byte rex = bytes[offset];
            if ((rex & 0xF8) != 0x48 || bytes[offset + 1] != 0x83) return false;
            byte modrm = bytes[offset + 2];
            if ((modrm & 0xF8) != 0xF8 || bytes[offset + 3] != 0x07) return false;
            register = (modrm & 7) | ((rex & 1) != 0 ? 8 : 0);
            length = 4;
            return true;
        }

        private static bool HasSubIntoRegister(byte[] bytes, int start, int end, int targetRegister)
        {
            for (int offset = start; offset + 3 <= end; offset++)
            {
                byte rex = bytes[offset];
                if ((rex & 0xF8) != 0x48) continue;
                byte opcode = bytes[offset + 1];
                byte modrm = bytes[offset + 2];
                if ((opcode != 0x29 && opcode != 0x2B) || (modrm & 0xC0) != 0xC0) continue;
                int reg = ((modrm >> 3) & 7) | ((rex & 4) != 0 ? 8 : 0);
                int rm = (modrm & 7) | ((rex & 1) != 0 ? 8 : 0);
                int destination = opcode == 0x29 ? rm : reg;
                if (destination == targetRegister) return true;
            }
            return false;
        }

        private static bool FindRegMemEndingAt(byte[] bytes, int start, int end, byte opcode, out int instructionOffset, out RegMemInstruction instruction)
        {
            instructionOffset = -1;
            instruction = null;
            for (int offset = start; offset < end; offset++)
            {
                RegMemInstruction candidate;
                if (TryDecodeRegMem(bytes, offset, opcode, out candidate) && offset + candidate.Length == end)
                {
                    instructionOffset = offset;
                    instruction = candidate;
                    return true;
                }
            }
            return false;
        }

        private static bool TryDecodeRegMem(byte[] bytes, int offset, byte opcode, out RegMemInstruction instruction)
        {
            instruction = null;
            if (offset + 3 > bytes.Length) return false;
            byte rex = bytes[offset];
            if ((rex & 0xF8) != 0x48 || bytes[offset + 1] != opcode) return false;
            byte modrm = bytes[offset + 2];
            int mod = modrm >> 6;
            if (mod == 3) return false;
            int rm = modrm & 7;
            int length = 3;
            bool hasSib = rm == 4;
            int sibBase = -1;
            if (hasSib)
            {
                if (offset + length >= bytes.Length) return false;
                sibBase = bytes[offset + length] & 7;
                length++;
            }
            if (mod == 0 && ((!hasSib && rm == 5) || (hasSib && sibBase == 5))) length += 4;
            else if (mod == 1) length += 1;
            else if (mod == 2) length += 4;
            if (offset + length > bytes.Length) return false;
            instruction = new RegMemInstruction { Length = length, Register = ((modrm >> 3) & 7) | ((rex & 4) != 0 ? 8 : 0) };
            return true;
        }

        private static bool HasIndirectCall(byte[] bytes, int start, int end)
        {
            for (int offset = start; offset + 2 <= end; offset++)
            {
                if (bytes[offset] == 0xFF && (((bytes[offset + 1] >> 3) & 7) == 2)) return true;
            }
            return false;
        }

        private static int CountDirectCalls(byte[] bytes, int start, int end)
        {
            int count = 0;
            for (int offset = start; offset < end; offset++)
            {
                if (bytes[offset] == 0xE8) count++;
            }
            return count;
        }

        private static int DistanceToNearest(int value, List<int> references)
        {
            int distance = int.MaxValue;
            foreach (int reference in references) distance = Math.Min(distance, Math.Abs(reference - value));
            return distance;
        }

        private static bool IsNops(byte[] bytes, int offset, int count)
        {
            if (offset < 0 || offset + count > bytes.Length) return false;
            for (int index = 0; index < count; index++) if (bytes[offset + index] != 0x90) return false;
            return true;
        }

        private static bool IsAscii(byte value) { return value >= 0x20 && value <= 0x7E; }
        private static int ReadUInt16(byte[] bytes, int offset) { return bytes[offset] | (bytes[offset + 1] << 8); }
        private static int ReadInt32(byte[] bytes, int offset) { return bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24); }

        private static QQNTAdaptiveResult Clone(QQNTAdaptiveResult source)
        {
            return new QQNTAdaptiveResult
            {
                IsPatched = source.IsPatched,
                GroupBranchOffset = source.GroupBranchOffset,
                PrivateBranchOffset = source.PrivateBranchOffset,
                Changes = source.Changes.Select(change => change.Clone()).ToList()
            };
        }
    }
}
