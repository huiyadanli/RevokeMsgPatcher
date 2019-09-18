using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace RevokeMsgPatcher.Utils
{
    public class FileUtil
    {
        /// <summary>
        /// 获取文件版本
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileVersion(string path)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(path);
            return fileVersionInfo.FileVersion;
        }

        /// <summary>
        /// 计算文件SHA1
        /// </summary>
        /// <param name="s">文件路径</param>
        /// <returns></returns>
        public static string ComputeFileSHA1(string s)
        {
            FileStream file = new FileStream(s, FileMode.Open);
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] retval = sha1.ComputeHash(file);
            file.Close();

            StringBuilder sc = new StringBuilder();
            for (int i = 0; i < retval.Length; i++)
            {
                sc.Append(retval[i].ToString("x2"));
            }
            return sc.ToString();
        }

        /// <summary>
        /// 修改文件指定位置的字节
        /// </summary>
        /// <param name="path">WeChatWin.dll 的路径</param>
        /// <param name="position">偏移位置</param>
        /// <param name="after">修改后的值</param>
        /// <returns></returns>
        public static bool EditHex(string path, long position, byte after)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                stream.Position = position;
                stream.WriteByte(after);
            }
            return true;

        }
    }
}
