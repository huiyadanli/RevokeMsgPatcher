
<p align="center">
	<a><img width="100px" src="https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/Images/logo.png"/></a>
</p>
<p align="center">
	<a href="https://www.microsoft.com/download/details.aspx?id=30653">
		<img src="https://img.shields.io/badge/platform-windows-lightgrey.svg?style=flat-square"/>
	</a>
	<a href="https://github.com/huiyadanli/RevokeMsgPatcher/releases">
		<img src="https://img.shields.io/github/downloads/huiyadanli/RevokeMsgPatcher/total.svg?style=flat-squares"/>
	</a>
	<a href="http://hits.dwyl.io/huiyadanli/RevokeMsgPatcher">
		<img src="http://hits.dwyl.io/huiyadanli/RevokeMsgPatcher.svg"/>
	</a>
	<a href="https://ci.appveyor.com/project/huiyadanli/RevokeMsgPatcher">
		<img src="https://img.shields.io/appveyor/ci/huiyadanli/RevokeMsgPatcher.svg?style=flat-square"/>
	</a>
</p>

# :eyes:微信/QQ/TIM防撤回补丁
适用于 Windows 下 PC 版微信/QQ/TIM的防撤回补丁。

<img width="180px" src="https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/Images/revoke.jpg"/>

下载地址：
[:zap:点我下载最新版本](https://github.com/huiyadanli/RevokeMsgPatcher/releases/download/0.5/RevokeMsgPatcher.v0.5.zip)


最新版本支持如下（微信从 2.7.1.74 版本开始支持多开）：

| 名称  | 支持最新版本                          |
|-----|---------------------------------|
| 微信  | 测试版 2\.7\.1\.82、稳定版 2\.6\.8\.68 |
| QQ  | 9\.1\.9\.26346、9\.1\.8\.26211                  |
| TIM | 2\.3\.2\.21173                  |

历史版本支持如下：

| 名称  | 支持历史版本                                                                                                                                          |
|-----|-------------------------------------------------------------------------------------------------------------------------------------------------|
| 微信  | 2\.6\.6\.28、2\.6\.7\.32、2\.6\.7\.40、2\.6\.7\.57、2\.6\.8\.37、2\.6\.8\.51、2\.6\.8\.52、2\.6\.8\.65、2\.7\.0\.65、2\.7\.0\.70、2\.7\.1\.43、2\.7\.1\.59、2\.7\.1\.65、2\.7\.1\.74 |
| QQ  | 9\.1\.7\.25980                                                                                                                                  |
| TIM |                                                                                                                                                 |


## :camera:截图
![Screenshot](https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/Images/screenshot.png)

## 🔨使用方法

1. 环境要求，你的系统需要满足以下条件：

    * Windows 7 或更高版本。
    * [.NET Framework 4.5](https://www.microsoft.com/zh-cn/download/details.aspx?id=30653) 或更高版本。

2. 使用本程序前，先关闭微信/QQ/TIM。

3. **以管理员身份运行本程序**，等待右下角获取最新的补丁信息。

4. 选择微信/QQ/TIM的安装路径。如果你用的安装版的微信/QQ/TIM，正常情况下本程序会自动从注册表中获取安装路径，绿色版需要手动选择路径。

5. 点击防撤回。界面可能会出现一段时间的无响应，耐心等待即可。

## ❓常见问题解答 FAQ

1. 防撤回时，360/杀毒软件 弹出警告怎么回事？
    * 由于修改了微信的 `WeChatWin.dll` 文件、QQ/TIM的 `IM.dll` 文件，杀毒软件可能会弹出警告，放行即可。

2. 点击防撤回后出现提示：“文件 xxxx 正由另一进程使用，因此该进程无法访问此文件。”
    * 请先关闭微信/QQ/TIM，再进行防撤回。

3. 为什么明明程序支持这个版本，点击防撤回的时候却提示版本不支持。
    * 程序自带的补丁信息并不是最新的，需要动态从网上获取最新补丁信息。请耐心等待右下角获取补丁信息的提示变成“获取成功”，点击这行字可以查看当前支持防撤回的版本信息，如果其中显示支持你当前需要防撤回的版本。可以[提Issue](https://github.com/huiyadanli/RevokeMsgPatcher/issues)给我

4. 能否添加防撤回提示？
    * 由于本程序只提供编辑二进制文件的功能，所以无法支持防撤回提示等更多扩展功能。

## :heart:Thanks

感谢以下repo提供的微信反撤回数据

[wechat_anti_revoke](https://github.com/36huo/wechat_anti_revoke)

## 📄License

使用本程序源码时请遵守 GPLv3 许可。

本程序的本质是一个十六进制编辑器，使用的修改数据集收集自网络。

![](https://raw.githubusercontent.com/huiyadanli/RevokeMsgPatcher/master/Images/give_a_star.png)
