匹配器回归测试直接编译生产代码，目标为 .NET Framework 4.5.2，无第三方测试依赖。

在仓库根目录的 Visual Studio Developer PowerShell 中执行：

```powershell
msbuild tests\Matcher.Tests\Matcher.Tests.csproj /p:Configuration=Release /nologo /v:minimal
.\tests\Matcher.Tests\bin\Release\Matcher.Tests.exe
```

测试以独立的逐字节匹配实现作为参照，覆盖 20,000 组固定随机种子数据、重叠匹配、通配符、文件首尾、模式长于文件、首个匹配、排除已替换位置及补丁状态判断。还会读取 `RevokeMsgPatcher.Assistant/Data/2.1/patch.json`，验证其中查找和替换特征的偏移。

内存回归检查要求 8 MiB 缓冲区的无命中扫描分配低于 1 MiB，防止重新引入按文件大小分配结果或候选数组的问题。

沿用原有语义：模糊匹配中 `0x3F` 表示单字节通配符，查找串不得以通配符开头；精确匹配器仍将 `0x3F` 作为普通字节。空查找串报参数错误，查找串长于文件时返回空结果。
