using RevokeMsgPatcher.Model;
using RevokeMsgPatcher.Utils;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace RevokeMsgPatcher
{
    /// <summary>
    /// 补丁信息
    /// </summary>
    public class JsonData
    {

        public Bag Bag()
        {
            return new Bag
            {
                Apps = AppConfig(),
                LatestVersion = "2.2",
                PatchVersion = 20261109,
                Notice = "",
                NoticeUrl = "",
            };
        }

        public string BagJson()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(Bag());
        }

        public Dictionary<string, App> AppConfig()
        {
            return new Dictionary<string, App>
            {
                { "Wechat" , Wechat() },
                { "Weixin" , Weixin() },
                { "QQNT" , QQNT() }
            };
        }

        public string AppConfigJson()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(AppConfig());
        }

        public App Wechat()
        {
            return new App
            {
                Name = "WeChat",
                FileTargetInfos = new Dictionary<string, TargetInfo>
                {
                    {
                        "WeChatWin.dll",
                        new TargetInfo
                        {
                            Name = "WeChatWin.dll",
                            RelativePath = "WeChatWin.dll",
                            StartVersion = "3.9.12.55",
                            EndVersion = "3.9.12.56"
                        }
                    }
                },
                FileCommonModifyInfos = new Dictionary<string, List<CommonModifyInfo>>
                {
                    {
                        "WeChatWin.dll",
                        new List<CommonModifyInfo>
                        {
                            // 原历史注释保留：带撤回提示；防撤回特征又和 3.4 时期一样了。
                            // 历史版本数据已清空，上述历史说明不作为当前特征的功能依据。
                            // 对比原版与 WXPatcher-main 样本，仅确认微信 3.9.12.56 x86。
                            // 版本范围左开右闭；3F 为通配符，保留导入函数的绝对地址。
                            // “多开”与“多开+防撤回”样本完全相同，均包含以下两项修改。
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.9.12.55",
                                EndVersion="3.9.12.56",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        // 文件偏移 0x017BF0B6：JE 改为 JMP，跳过设置 ESI=4 的分支。
                                        Search = ByteUtil.HexStringToByteArray("83 C4 10 84 C0 74 0A BE 04 00 00 00 E9"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 10 84 C0 EB 0A BE 04 00 00 00 E9"),
                                        Category = "防撤回",
                                        Tips = "微信 3.9.12.56 x86"
                                    },
                                    new ReplacePattern
                                    {
                                        // 文件偏移 0x021E8117：按样本将 6 字节 JE 改为 5 字节 JMP 加一个 00。
                                        // 位移从 0x85 调整为 0x86，跳转目标仍为 RVA 0x021E8DA2；末尾 00 不会执行。
                                        Search = ByteUtil.HexStringToByteArray("89 45 9C C6 45 A3 00 85 C0 0F 84 85 00 00 00 FF 15 3F 3F 3F 3F 0F 10 05"),
                                        Replace = ByteUtil.HexStringToByteArray("89 45 9C C6 45 A3 00 85 C0 E9 86 00 00 00 00 FF 15 3F 3F 3F 3F 0F 10 05"),
                                        Category = "多开",
                                        Tips = "微信 3.9.12.56 x86。"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }


        public App QQNT()
        {
            return new App
            {
                Name = "QQNT",
                FileTargetInfos = new Dictionary<string, TargetInfo>
                {
                    {
                        "wrapper.node",
                        new TargetInfo
                        {
                            Name = "wrapper.node",
                            RelativePath = @"versions\{version}\resources\app\wrapper.node",
                            RelativePathForVersion = "QQ.exe"
                        }
                    }
                },
                FileCommonModifyInfos = new Dictionary<string, List<CommonModifyInfo>>
                {
                    {
                        "wrapper.node",
                        new List<CommonModifyInfo>
                        {
                            // QQNT 9.9.36-53644 x64，用户已实测群聊、私聊防撤回正常。
                            // 匹配器使用 wrapper.node 的 PE 文件版本 9.8.0.24541；版本范围左开右闭。
                            // 3F 为通配符；保留栈偏移、相对地址，群聊固定走错误清理分支，私聊固定跳到空集合清理分支。
                            new CommonModifyInfo
                            {
                                Name="wrapper.node",
                                StartVersion="9.8.0.24540",
                                EndVersion="",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("48 8B 95 3F 3F 3F 3F 4C 8B 85 3F 3F 3F 3F 4C 89 C0 48 29 D0 48 83 F8 07 0F 87 3F 3F 3F 3F 48 8D 35 3F 3F 3F 3F 6A 5C 5A 48 89 F1"),
                                        Replace = ByteUtil.HexStringToByteArray("48 8B 95 3F 3F 3F 3F 4C 8B 85 3F 3F 3F 3F 4C 89 C0 48 29 D0 48 83 F8 07 90 90 90 90 90 90 48 8D 35 3F 3F 3F 3F 6A 5C 5A 48 89 F1"),
                                        Category = "防撤回",
                                        Tips = "9.9.36-53644 、9.9.37-54395 验证通过"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("48 8B 85 3F 3F 3F 3F 48 3B 85 3F 3F 3F 3F 0F 84 3F 3F 3F 3F 48 8B 85 3F 3F 3F 3F 48 8B 08 48 8B 41 08 48 83 C1 08 6A 06 5A FF 90 28 01 00 00 84 C0 0F 84 3F 3F 3F 3F"),
                                        Replace = ByteUtil.HexStringToByteArray("48 8B 85 3F 3F 3F 3F 48 3B 85 3F 3F 3F 3F 90 E9 3F 3F 3F 3F 48 8B 85 3F 3F 3F 3F 48 8B 08 48 8B 41 08 48 83 C1 08 6A 06 5A FF 90 28 01 00 00 84 C0 0F 84 3F 3F 3F 3F"),
                                        Category = "防撤回",
                                        Tips = "9.9.36-53644 、9.9.37-54395 验证通过"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public App Weixin()
        {
            return new App
            {
                Name = "Weixin",
                FileTargetInfos = new Dictionary<string, TargetInfo>
                {
                    {
                        "Weixin.dll",
                        new TargetInfo
                        {
                            Name = "Weixin.dll",
                            RelativePath = "Weixin.dll",
                            StartVersion = "1.0.0.0"
                        }
                    }
                },
                FileCommonModifyInfos = new Dictionary<string, List<CommonModifyInfo>>
                {
                    {
                        "Weixin.dll",
                        new List<CommonModifyInfo>
                        {
                            new CommonModifyInfo
                            {
                                Name="Weixin.dll",
                                StartVersion="4.1.12.0",
                                EndVersion="",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("48 89 86 3F 3F 3F 3F 4C 89 AD 08 02 00 00 4C 8D 05"),
                                        Replace = ByteUtil.HexStringToByteArray("48 29 86 3F 3F 3F 3F 4C 89 AD 08 02 00 00 4C 8D 05"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("89 C7 B9 02 00 00 00 E8 3F 3F 3F 3F 81 FF B7 00 00 00 0F 85"),
                                        Replace = ByteUtil.HexStringToByteArray("89 C7 B9 02 00 00 00 E8 3F 3F 3F 3F 81 FF B7 00 00 00 90 E9"),
                                        Category = "多开"
                                    }
                                }
                            },
                            //new CommonModifyInfo
                            //{
                            //    Name="Weixin.dll",
                            //    StartVersion="4.1.9.0",
                            //    EndVersion="",
                            //    ReplacePatterns = new List<ReplacePattern>
                            //    {
                            //        new ReplacePattern
                            //        {
                            //            Search = ByteUtil.HexStringToByteArray("48 8D 55 3F 45 31 C0 E8 3F 3F 3F 3F 90 48 8B 9D 3F 3F 3F 3F 48 85 DB 74 3F F0 FF 4B 08"),
                            //            Replace = ByteUtil.HexStringToByteArray("48 8D 55 3F 48 83 85 68 03 00 00 01 90 48 8B 9D 3F 3F 3F 3F 48 85 DB 74 3F F0 FF 4B 08"),
                            //            Category = "防撤回"
                            //        },
                            //        new ReplacePattern
                            //        {
                            //            Search = ByteUtil.HexStringToByteArray("48 83 EC 3F 4C 3F 3F 48 3F 3F C6 44 24 3F 01 C6 44 24 3F 00 4D 3F 3F 49 3F 3F E8"),
                            //            Replace = ByteUtil.HexStringToByteArray("48 83 EC 3F 4C 3F 3F 48 3F 3F C6 44 24 3F 01 C6 44 24 3F 01 4D 3F 3F 49 3F 3F E8"),
                            //            Category = "防撤回"
                            //        },
                            //        new ReplacePattern
                            //        {
                            //            Search = ByteUtil.HexStringToByteArray("81 FF B7 00 00 00 0F 85"),
                            //            Replace = ByteUtil.HexStringToByteArray("81 FF B7 00 00 00 90 E9"),
                            //            Category = "多开"
                            //        }
                            //    }
                            //},
                            new CommonModifyInfo
                            {
                                Name="Weixin.dll",
                                StartVersion="4.1.9.0",
                                EndVersion="4.1.12.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("90 48 89 86 C8 01 00 00 4C 89 AD 08 02 00 00 4C 8D 05"),
                                        Replace = ByteUtil.HexStringToByteArray("90 48 29 86 C8 01 00 00 4C 89 AD 08 02 00 00 4C 8D 05"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("81 FF B7 00 00 00 0F 85"),
                                        Replace = ByteUtil.HexStringToByteArray("81 FF B7 00 00 00 90 E9"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="Weixin.dll",
                                StartVersion="4.1.7.1",
                                EndVersion="4.1.9.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("90 48 89 86 A8 01 00 00"),
                                        Replace = ByteUtil.HexStringToByteArray("90 48 29 86 A8 01 00 00"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("81 FF B7 00 00 00 0F 85"),
                                        Replace = ByteUtil.HexStringToByteArray("81 FF B7 00 00 00 90 E9"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="Weixin.dll",
                                StartVersion="4.1.6.15",
                                EndVersion="4.1.7.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("CC 56 48 83 EC 30 4C 89 C0 48 89 D6 C6 44 24 20 00 4D 89 C8 49 89 C1 E8"),
                                        Replace = ByteUtil.HexStringToByteArray("CC 56 48 83 EC 30 4C 89 C0 48 89 D6 C6 44 24 20 01 4D 89 C8 49 89 C1 E8"),
                                        Category = "防撤回"
                                    },
                                    // new ReplacePattern
                                    // {
                                    //     Search = ByteUtil.HexStringToByteArray("65 69 78 69 6E 55 70 64 61 74 65 00 57"),
                                    //     Replace = ByteUtil.HexStringToByteArray("65 69 78 69 6E 55 70 64 61 74 2E 00 57"),
                                    //     Category = "防撤回"
                                    // },                                    
                                    // new ReplacePattern
                                    // {
                                    //     Search = ByteUtil.HexStringToByteArray("6E 55 70 64 61 74 65 2E 65 78 65 00 00 00 00 00 00 00 00 00 00 57 65 69 78 69 6E 55 70 64 61 74 65 2E 65"),
                                    //     Replace = ByteUtil.HexStringToByteArray("6E 55 70 64 61 74 2E 2E 65 78 65 00 00 00 00 00 00 00 00 00 00 57 65 69 78 69 6E 55 70 64 61 74 2E 2E 65"),
                                    //     Category = "防撤回"
                                    // },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("48 8D 55 C0 45 31 C0 E8 29 F8 47 FE 90 48"),
                                        Replace = ByteUtil.HexStringToByteArray("48 8D 55 C0 48 83 85 D0 02 00 00 01 90 48"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("84 C0 75 1B E8 76 E5 F7 FF 84 C0 75 12 E8"),
                                        Replace = ByteUtil.HexStringToByteArray("84 C0 EB 3F E8 3F 3F 3F FF 84 C0 75 3F E8"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="Weixin.dll",
                                StartVersion="4.1.0.17",
                                EndVersion="4.1.6.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("DA FE FF FF 4C 89 F1 E8 3F 3F 3F 3F 84 C0 0F 84"),
                                        Replace = ByteUtil.HexStringToByteArray("DA FE FF FF 4C 89 F1 E8 3F 3F 3F 3F 84 C0 90 E9"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("31 C9 31 D2 49 89 C0 FF 15 3F 3F 3F 3F 48 85 C0 0F 84"),
                                        Replace = ByteUtil.HexStringToByteArray("31 C9 31 D2 49 89 C0 FF 15 3F 3F 3F 3F 48 85 C0 90 E9"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="Weixin.dll",
                                StartVersion="4.1.0.0",
                                EndVersion="4.1.0.17",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("48 8D 8D 20 04 00 00 E8 CD 3F 3F 3F 84 C0 0F 84 3F 3F 3F 3F 48 8D 8D"),
                                        Replace = ByteUtil.HexStringToByteArray("48 8D 8D 20 04 00 00 E8 CD 3F 3F 3F 84 C0 90 E9 3F 3F 3F 3F 48 8D 8D"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("5F C3 CC CC CC CC CC CC CC CC CC CC CC CC CC CC 55 41 56 56 57 53"),
                                        Replace = ByteUtil.HexStringToByteArray("5F C3 CC CC CC CC CC CC CC CC CC CC CC CC CC CC C3 41 56 56 57 53"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="Weixin.dll",
                                StartVersion="4.0.3.0",
                                EndVersion="4.1.0.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("75 21 48 B8 72 65 76 6F 6B 65 6D 73 48 89 05 3F 3F 3F 3F 66 C7 05 3F 3F 3F 3F 67 00 C6 05 3F 3F 3F 3F 01 48 8D"),
                                        Replace = ByteUtil.HexStringToByteArray("EB 21 48 B8 72 65 76 6F 6B 65 6D 73 48 89 05 3F 3F 3F 3F 66 C7 05 3F 3F 3F 3F 67 00 C6 05 3F 3F 3F 3F 01 48 8D"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("55 56 57 53 48 81 EC 3F 3F 3F 3F 48 8D AC 24 3F 3F 3F 3F 48 C7 85 3F 3F 3F 3F FE FF FF FF 48 C7 85 3F 3F 3F 3F 00 00 00 00 B9 60 00 00 00"),
                                        Replace = ByteUtil.HexStringToByteArray("C3 56 57 53 48 81 EC 3F 3F 3F 3F 48 8D AC 24 3F 3F 3F 3F 48 C7 85 3F 3F 3F 3F FE FF FF FF 48 C7 85 3F 3F 3F 3F 00 00 00 00 B9 60 00 00 00"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="Weixin.dll",
                                StartVersion="4.0.0.0",
                                EndVersion="4.0.3.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("57 53 48 83 EC 20 48 89 CE 80 3D 3F 3F 3F 3F 00 75 21 48 B8"),
                                        Replace = ByteUtil.HexStringToByteArray("57 53 48 83 EC 20 48 89 CE 80 3D 3F 3F 3F 3F 00 EB 21 48 B8"),
                                        Category = "防撤回"
                                    },
                                    // new ReplacePattern
                                    // {
                                    //     Search = ByteUtil.HexStringToByteArray("BA 01 00 00 00 E8 3F 3F 3F 00 85 C0 0F 84 3F 03 00"),
                                    //     Replace = ByteUtil.HexStringToByteArray("BA 01 00 00 00 E8 3F 3F 3F 00 85 C0 E9 50 03 00 00"),
                                    //     Category = "多开"
                                    // }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
