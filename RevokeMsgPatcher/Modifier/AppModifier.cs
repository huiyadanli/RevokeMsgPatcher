using RevokeMsgPatcher.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevokeMsgPatcher.Modifier
{
    public abstract class AppModifier
    {
        private App config;

        private List<FileHexEditor> editors;

        // 1. 获取安装目录
        // 2. 验证安装目录 通过 BinaryFiles
        // 3. 判断所有 BinaryFiles 是否符合防撤回要求
        // 4. 备份所有 BinaryFiles  // *.h.bak
        // 5. 对每个 BinaryFile 中的 Changes 循环修改（修改前要测试它的读写性质,是否被程序占用等）

        // 获取版本号
        // 通过SHA1判断是否可以进行16进制编辑 先判断版本号 再判断SHA1    根据不同结果返回不同的提示

        // ?多文件的备份回退 // 暂定有一个备份文件就点亮按钮

        public abstract string FindInstallPath();

        //public abstract bool ValidateInstallPath();

        public abstract bool GetVersion();

        public bool IsAllBinaryFilesExist(string installPath)
        {
            int success = 0;
            foreach(string relativePath in config.ModifyFilePaths)
            {
                string filePath = Path.Combine(installPath, relativePath);
                if(File.Exists(filePath))
                {
                    success++;
                }
            }
            if(success == config.ModifyFilePaths.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
