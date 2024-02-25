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
                LatestVersion = "1.7",
                PatchVersion = 20240225,
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
                { "QQ" , QQ() },
                { "TIM" , TIM() },
                { "QQLite" , QQLite() }
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
                            StartVersion = "1.0.0.0"
                        }
                    },
                    {
                        "WeChat.exe",
                        new TargetInfo
                        {
                            Name = "WeChat.exe",
                            RelativePath = "../WeChat.exe",
                            StartVersion = "3.7.0.0",
                            EndVersion = "3.7.0.26"
                        }
                    }
                },
                FileCommonModifyInfos = new Dictionary<string, List<CommonModifyInfo>>
                {
                    {
                        "WeChat.exe",
                        new List<CommonModifyInfo>
                        {
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.7.0.8",
                                EndVersion="3.7.0.26",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 08 84 C0 75 3F 68"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 08 84 C0 EB 3F 68"),
                                        Category = "去除校验"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.7.0.0",
                                EndVersion="3.7.0.8",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("85 C0 75 59"),
                                        Replace = ByteUtil.HexStringToByteArray("85 C0 EB 59"),
                                        Category = "去除校验"
                                    }
                                }
                            }
                        }
                    },
                    {
                        "WeChatWin.dll",
                        new List<CommonModifyInfo>
                        {
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.9.9.0",
                                EndVersion="",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("0F 1F 44 00 00 49 8B 50 08 48 85 D2 74 3F 48 C7 C1"),
                                        Replace = ByteUtil.HexStringToByteArray("0F 1F 44 00 00 49 8B 50 08 48 85 D2 75 3F 48 C7 C1"),
                                        Category = "防撤回(老)"
                                    },
                                    // 带撤回提示
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("4D 85 C0 0F 84 3F 3F 3F 3F EB BF 41 8B"),
                                        Replace = ByteUtil.HexStringToByteArray("4D 85 C0 0F 84 3F 3F 3F 3F 90 90 41 8B"),
                                        Category = "防撤回带提示(新)"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("01 3D B7 00 00 00 0F 85 3F 3F 3F 3F 48 8B CF"),
                                        Replace = ByteUtil.HexStringToByteArray("01 3D B7 00 00 00 90 E9 3F 3F 3F 3F 48 8B CF"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.9.6.0",
                                EndVersion="3.9.9.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("0F 1F 44 00 00 49 8B 50 08 48 85 D2 74 3F 48 C7 C1"),
                                        Replace = ByteUtil.HexStringToByteArray("0F 1F 44 00 00 49 8B 50 08 48 85 D2 75 3F 48 C7 C1"),
                                        Category = "防撤回(老)"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("01 3D B7 00 00 00 0F 85 3F 3F 3F 3F 48 8B CF"),
                                        Replace = ByteUtil.HexStringToByteArray("01 3D B7 00 00 00 90 E9 3F 3F 3F 3F 48 8B CF"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.9.5.0",
                                EndVersion="3.9.6.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("0F 1F 44 00 00 49 8B 50 08 48 85 D2 74 3F 48 C7 C1"),
                                        Replace = ByteUtil.HexStringToByteArray("0F 1F 44 00 00 49 8B 50 08 48 85 D2 75 3F 48 C7 C1"),
                                        Category = "防撤回(老)"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("18 01 3D B7 00 00 00 0F 85 3F 3F 3F 3F 48 8B CF"),
                                        Replace = ByteUtil.HexStringToByteArray("18 01 3D B7 00 00 00 90 E9 3F 3F 3F 3F 48 8B CF"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.9.2.0",
                                EndVersion="3.9.5.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 10 84 C0 74 0A BE 02 00 00 00 E9"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 10 84 C0 90 90 BE 02 00 00 00 E9"),
                                        Category = "防撤回(老)"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 04 84 DB 0F 84 3F 3F 3F 3F 8B 3D"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 04 84 DB 90 E9 3F 3F 3F 3F 8B 3D"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.9.0.0",
                                EndVersion="3.9.2.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("85 C0 74 32 B9 3F 3F 3F 3F 8A"),
                                        Replace = ByteUtil.HexStringToByteArray("85 C0 EB 32 B9 3F 3F 3F 3F 8A"),
                                        Category = "防撤回(老)"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("C3 33 C0 A3 3F 3F 3F 3F C3 CC CC CC CC CC CC CC CC CC CC CC CC 55 8B EC"),
                                        Replace = ByteUtil.HexStringToByteArray("C3 33 C0 A3 3F 3F 3F 3F C3 CC CC CC CC CC CC CC CC CC CC CC CC C3 8B EC"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.8.1.0",
                                EndVersion="3.9.0.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("85 C0 74 32 B9 3F 3F 3F 3F 8A"),
                                        Replace = ByteUtil.HexStringToByteArray("85 C0 EB 32 B9 3F 3F 3F 3F 8A"),
                                        Category = "防撤回(老)"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 04 84 DB 0F 84 26 01 00 00 8B 3D"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 04 84 DB 90 E9 26 01 00 00 8B 3D"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.8.0.0",
                                EndVersion="3.8.1.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("85 C0 74 32 B9 3F 3F 3F 3F 8A"),
                                        Replace = ByteUtil.HexStringToByteArray("85 C0 EB 32 B9 3F 3F 3F 3F 8A"),
                                        Category = "防撤回(老)"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 04 80 BD DF FB FF FF 00 74 58 8B 3D"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 04 80 BD DF FB FF FF 00 EB 58 8B 3D"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.7.6.0",
                                EndVersion="3.8.0.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("85 C0 74 32 B9 3F 3F 3F 3F 8A"),
                                        Replace = ByteUtil.HexStringToByteArray("85 C0 EB 32 B9 3F 3F 3F 3F 8A"),
                                        Category = "防撤回(老)"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 04 80 BD FF FB FF FF 00 74 58 8B 3D"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 04 80 BD FF FB FF FF 00 EB 58 8B 3D"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.7.0.0",
                                EndVersion="3.7.6.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("00 85 C0 74 32 B9 3F 3F 3F 3F 8A"),
                                        Replace = ByteUtil.HexStringToByteArray("00 85 C0 EB 32 B9 3F 3F 3F 3F 8A"),
                                        Category = "防撤回(老)"
                                    },
                                    // 带撤回提示
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("80 7D F2 00 6A 01 74"),
                                        Replace = ByteUtil.HexStringToByteArray("80 7D F2 00 6A 01 EB"),
                                        Category = "防撤回带提示(新)"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 04 80 BD 3F FC FF FF 00 74 58 8B 3D"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 04 80 BD 3F FC FF FF 00 EB 58 8B 3D"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.6.5.0",
                                EndVersion="3.7.0.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("00 85 C0 74 32 B9 3F 3F 3F 11 8A"),
                                        Replace = ByteUtil.HexStringToByteArray("00 85 C0 EB 32 B9 3F 3F 3F 11 8A"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 04 80 BD 3F FC FF FF 00 74 58 8B 3D"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 04 80 BD 3F FC FF FF 00 EB 58 8B 3D"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.6.0.5",
                                EndVersion="3.6.5.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    // 防撤回特征又和 3.4 时期一样了
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("EF 00 85 C0 74 32 B9 3F 3F 3F 11 8A"),
                                        Replace = ByteUtil.HexStringToByteArray("EF 00 85 C0 EB 32 B9 3F 3F 3F 11 8A"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 04 80 BD 2F FC FF FF 00 74 58 8B 3D"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 04 80 BD 2F FC FF FF 00 EB 58 8B 3D"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.5.0.28",
                                EndVersion="3.6.0.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("ED 00 85 C0 74 32 B9"),
                                        Replace = ByteUtil.HexStringToByteArray("ED 00 33 C0 74 32 B9"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("83 C4 04 80 BD 2F FC FF FF 00 74 58 8B 3D"),
                                        Replace = ByteUtil.HexStringToByteArray("83 C4 04 80 BD 2F FC FF FF 00 EB 58 8B 3D"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.4.0.0",
                                EndVersion="3.5.0.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("00 85 C0 74 32 B9 3F 3F 3F 11 8A"),
                                        Replace = ByteUtil.HexStringToByteArray("00 85 C0 EB 32 B9 3F 3F 3F 11 8A"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("E8 6B 00 00 00 84 C0 74 56 56 6A 00"),
                                        Replace = ByteUtil.HexStringToByteArray("E8 6B 00 00 00 84 C0 EB 56 56 6A 00"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.3.5.15",
                                EndVersion="3.4.0.0",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("00 85 C0 74 32 B9 3F 3F 95 11 8A"),
                                        Replace = ByteUtil.HexStringToByteArray("00 85 C0 EB 32 B9 3F 3F 95 11 8A"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("E8 6B 00 00 00 84 C0 74 56 56 6A 00"),
                                        Replace = ByteUtil.HexStringToByteArray("E8 6B 00 00 00 84 C0 EB 56 56 6A 00"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.2.0.00",
                                EndVersion="3.3.5.00",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("00 85 C0 74 7B 8B C8 E8"),
                                        Replace = ByteUtil.HexStringToByteArray("00 85 C0 EB 7B 8B C8 E8"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("E8 6B 00 00 00 84 C0 74 56 56 6A 00"),
                                        Replace = ByteUtil.HexStringToByteArray("E8 6B 00 00 00 84 C0 EB 56 56 6A 00"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="3.1.0.00",
                                EndVersion="3.2.0.00",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("00 85 C0 74 7B 8B C8 E8"),
                                        Replace = ByteUtil.HexStringToByteArray("00 85 C0 EB 7B 8B C8 E8"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("FF E8 9B EF FF FF 84 C0 74 08 83"),
                                        Replace = ByteUtil.HexStringToByteArray("FF 90 90 90 90 90 84 C0 74 08 83"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="2.8.0.88",
                                EndVersion="3.1.0.00",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("00 85 C0 74 7B 8B C8 E8"),
                                        Replace = ByteUtil.HexStringToByteArray("00 85 C0 EB 7B 8B C8 E8"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("C0 C3 CC CC CC CC CC CC CC CC CC CC CC CC CC CC 55 8B EC 83 EC 14 53 56 57 6A FF 0F 57 C0 C7"),
                                        Replace = ByteUtil.HexStringToByteArray("C0 C3 CC CC CC CC CC CC CC CC CC CC CC CC CC CC C3 8B EC 83 EC 14 53 56 57 6A FF 0F 57 C0 C7"),
                                        Category = "多开"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="WeChatWin.dll",
                                StartVersion="2.7.0.00",
                                EndVersion="2.8.0.88",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("00 85 C0 74 32 B9"),
                                        Replace = ByteUtil.HexStringToByteArray("00 85 C0 EB 32 B9"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("C0 C3 CC CC CC CC CC CC CC CC CC CC CC CC CC CC 55 8B EC 83 EC 14 53 56 57 6A FF 0F 57 C0 C7"),
                                        Replace = ByteUtil.HexStringToByteArray("C0 C3 CC CC CC CC CC CC CC CC CC CC CC CC CC CC C3 8B EC 83 EC 14 53 56 57 6A FF 0F 57 C0 C7"),
                                        Category = "多开"
                                    }
                                }
                            }
                        }
                    }
                },
                FileModifyInfos = new Dictionary<string, List<ModifyInfo>>
                {
                    {
                        "WeChat.exe",
                        new List<ModifyInfo>
                        {

                        }
                    },
                    {
                        "WeChatWin.dll",
                        new List<ModifyInfo>
                        {
/*                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="3.5.0.29",
                                SHA1Before="018c3af27ac5d618a89018babc51ed90665ed1cc",
                                SHA1After="8e070a908305061d86c6ff9cc3f30d8cfa802cac",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x003AE6A9,
                                        Content =new byte[] { 0x31 }
                                    },
                                    new Change
                                    {
                                        Position =0x00CA5967,
                                        Content =new byte[] { 0xEB }
                                    }
                                }
                            },*/
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="3.3.5.25",
                                SHA1Before="3e94753ccbc2799d98f3c741377e99bdae33b4cf",
                                SHA1After="ab98f83fc16674ac4911380882c79c3ca4c2fd71",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x003417D9,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x00B98A67,
                                        Content =new byte[] { 0xEB }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="3.3.5.15",
                                SHA1Before="7d06e076c525aec6783e919dde4ee11ba9fdb198",
                                SHA1After="f22ab996a1779c2b41132d42c4fdf98c003284e0",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x0033D769,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x00B8F2C7,
                                        Content =new byte[] { 0xEB }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.8.0.112",
                                SHA1Before="7497fc006f061799138aa33419280e41891a7981",
                                SHA1After="07e681be40af32738d59a9332fe966c83c05e455",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x0028ED79,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x007E7B10,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.8.0.106",
                                SHA1Before="e772c81c8a1b79cac77b22dbe67b375fa340ba30",
                                SHA1After="bcbc491910f07cb995ef154a281450f2d052e90b",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x00289099,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x007E5960,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.8.0.88",
                                SHA1Before="b34c73e38cfec186890b58caac5fc9962377cd9b",
                                SHA1After="8c108e92de0b6b4d9e61ce38ab8dd339a27e505b",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x00288489,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x007E3D80,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.8.0.82",
                                SHA1Before="c359cc1a391441d261753f2844f9156638df8631",
                                SHA1After="d1b4dee8f7f91e34d68501987fd0675b33fe85da",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x00285FC9,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x007E16B0,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.7.2.78",
                                SHA1Before="26a5c5503f1e176676da5657c12812da8aaa0243",
                                SHA1After="d338215a815c09755c04949995ec3e4eab8dce60",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x00285EA9,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x007E1380,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.7.2.76",
                                SHA1Before="0003c7b2c0136a0eb2a6cfc2c694cb57b04b5517",
                                SHA1After="88af6055a0f4d3bdaa6f717ec8b263d4418487b6",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x00285BA9,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x007E0DA0,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.7.1.88",
                                SHA1Before="034059bad50dd793140952391bfa7936133e69b4",
                                SHA1After="dd6d80c30ca9e0ea9f7d2f1add498fc9aa4bc7a0",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x00262389,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x007957B0,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.7.1.85",
                                SHA1Before="de0df4e138b72460450f66c029e33f4510f5e2df",
                                SHA1After="fbd35720aaff3cdcfd3ff18ea503dc06450e5c99",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x00262389,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x00795680,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.7.1.82",
                                SHA1Before="20e111a18872bf6c7148a897c11da26c1ec95520",
                                SHA1After="1e0741d325ca6b1cd2402b829a3d13a2524af617",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x00262389,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x00795650,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.7.1.74",
                                SHA1Before="b1eaf7edc074a88be5d0f89230436cc2084d24d2",
                                SHA1After="eb3d74ccd87a09059a005f4972861898fc3de463",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =0x00262389,
                                        Content =new byte[] { 0xEB }
                                    },
                                    new Change
                                    {
                                        Position =0x00795550,
                                        Content =new byte[] { 0xC3 }
                                    }
                                }
                            },
                            new ModifyInfo {
                                Name="WeChatWin.dll",
                                Version="2.7.1.65",
                                SHA1Before="8346b97d264725da924d240c6eb77df3e693385e",
                                SHA1After="42bab2c9c79ef4f2088c00ea6d817973e14a5e6e",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position =2495545,
                                        Content =new byte[] { 235}
                                    }
                                }
                            },
                            new ModifyInfo {Name="WeChatWin.dll",Version="2.7.1.59",SHA1Before="df954d403edaca89cd5394927a325a0023e93281",SHA1After="6aa22460c91bb5c5e2f0ec1af99b8a5f6d4318c0",Changes = new List<Change> { new Change {Position=2496073,Content=new byte[] { 235} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.7.1.43",SHA1Before="39cd9e09e1a3eac09e6808749bff525c9e3216ce",SHA1After="7b829f1ff0217e346a80f9510fdd7634ddd49445",Changes = new List<Change> { new Change {Position=2494169,Content=new byte[] { 235} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.7.0.70",SHA1Before="3b0601864aff3c1d792f812ad1ca05f02aa761e3",SHA1After="1e8734d32b0a8c12758e30f99c77f729991fb071",Changes = new List<Change> { new Change {Position=2475657,Content=new byte[] { 235} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.7.0.65",SHA1Before="063c2e05a0df1bdb8987c2d978d93499bd2052ba",SHA1After="5ed4c09a4f18643b967f063a824d7e65d0567f8a",Changes = new List<Change> { new Change {Position=2475449,Content=new byte[] { 117} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.6.8.68",SHA1Before="2e9417f4276b12fe32ca7b4fee49272a4a2af334",SHA1After="699602ee3cbb9ae5714f6e6ebc658c875a6c66e6",Changes = new List<Change> { new Change {Position=2454006,Content=new byte[] { 116} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.6.8.65",SHA1Before="e01f6855a96c12c30808960903ed199a33e4952c",SHA1After="d9120569cfd0433aebea107d7b90805cbbac7518",Changes = new List<Change> { new Change {Position=2454265,Content=new byte[] { 117} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.6.8.52",SHA1Before="88131302f664df6a657c9ca49d152da536fe5729",SHA1After="8d1454b73831644181e962c1fa0ea4e2da4124a3",Changes = new List<Change> { new Change {Position=2453049,Content=new byte[] { 117} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.6.8.51",SHA1Before="d0a5517b1292a751501b00b4b1f0702db2d9fc30",SHA1After="53e7b1525d49bf2c3250a8131ff0ba2510779b78",Changes = new List<Change> { new Change {Position=2452614,Content=new byte[] { 116} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.6.8.37",SHA1Before="7e01f8b04a158a4a50bc5a6e67c2fb8b02233170",SHA1After="a1895004415fe9bcd7e690bd6e482b833b515599",Changes = new List<Change> { new Change {Position=2452614,Content=new byte[] { 116} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.6.7.57",SHA1Before="80a91aaf941bcb1c24a7d672838ac73e9ebb2e40",SHA1After="a0d3f9a45a835f97aef7fe0872387d8cfb5c25a4",Changes = new List<Change> { new Change {Position=2433413,Content=new byte[] { 116} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.6.7.40",SHA1Before="04bd0cb28df6630b518f42a3f9c2caa4a9359fbc",SHA1After="13c91cf1d4609959771fd137b9a86a5ca365e1b6",Changes = new List<Change> { new Change {Position=2432934,Content=new byte[] { 116} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.6.7.32",SHA1Before="a02519c1007ee6723947c262c720d63c619f633e",SHA1After="f3007471ca8734c29783c25f0bb49949a783a44",Changes = new List<Change> { new Change {Position=2432806,Content=new byte[] { 116} } } },new ModifyInfo {Name="WeChatWin.dll",Version="2.6.6.28",SHA1Before="0b19cb17a62c3ea0efce0fb675a1d3b17845cba3",SHA1After="260948656725446b818ea668273ceff02ddfb44d",Changes = new List<Change> { new Change {Position=2401678,Content=new byte[] { 116} } } }
                        }
                    }
                }
            };
        }

        public App QQ()
        {
            return new App
            {
                Name = "QQ",
                FileTargetInfos = new Dictionary<string, TargetInfo>
                {
                    {
                        "IM.dll",
                        new TargetInfo
                        {
                            Name = "IM.dll",
                            RelativePath = @"Bin\IM.dll"
                        }
                    }
                },
                FileCommonModifyInfos = new Dictionary<string, List<CommonModifyInfo>>
                {
                    {
                        "IM.dll",
                        new List<CommonModifyInfo>
                        {
                            new CommonModifyInfo
                            {
                                Name="IM.dll",
                                StartVersion="9.4.7.00000",
                                EndVersion="",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC 68 3F 3F 3F 3F 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC EB 09 90 90 90 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 F0 8D 55 EC 52 89 5D EC 68 3F 3F 3F 3F 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 F0 8D 55 EC 52 89 5D EC EB 09 90 90 90 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("8B 75 14 8D 4D F4 83 C4 20 33 FF 89 7D F4 8B 06 51 68 3F 3F 3F 3F 56 FF 50 78 85 C0 79 39 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("8B 75 14 8D 4D F4 83 C4 20 33 FF 89 7D F4 8B 06 EB 08 90 90 90 90 56 FF 50 78 85 C0 79 39 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="IM.dll",
                                StartVersion="9.1.6.00000",
                                EndVersion="9.4.7.00000",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC 68 3F 3F 3F 54 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC EB 09 90 90 90 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 F0 8D 55 EC 52 89 5D EC 68 3F 3F 3F 54 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 F0 8D 55 EC 52 89 5D EC EB 09 90 90 90 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("8B 75 14 8D 4D F4 83 C4 20 33 FF 89 7D F4 8B 06 51 68 3F 3F 3F 54 56 FF 50 78 85 C0 79 39 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("8B 75 14 8D 4D F4 83 C4 20 33 FF 89 7D F4 8B 06 EB 08 90 90 90 90 56 FF 50 78 85 C0 79 39 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    }
                                }
                            }
                        }
                    }
                },
                FileModifyInfos = new Dictionary<string, List<ModifyInfo>>
                {
                    {
                        "IM.dll",
                        new List<ModifyInfo>
                        {
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.2.3.26592",
                                SHA1Before = "9114e7869572b4b868afcbc8b28eae932559ec60",
                                SHA1After = "42e15175fd53768bb48772dc69fb07a4eac5a623",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x0005AB95,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0005ADB2,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0005AF60,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.2.2.26569",
                                SHA1Before = "434254e76c520789558e075af677821258536311",
                                SHA1After = "237c9e489a97858a175f0f7c72ade4ebcbac7a69",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x0005A9CA,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0005ABE7,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0005AD95,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.2.1.26546",
                                SHA1Before = "8d8ea2c2cbf43f5acf8d684b153e90035352d5f5",
                                SHA1After = "7d194dd5be03982b533d7375c93d9a72587fe28d",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x0005A389,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0005A5A6,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0005A754,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.2.0.26453",
                                SHA1Before = "c1935ca6347b0c2a7e6108a7f8ee0643d39deb66",
                                SHA1After = "42811188a7e7b346a6a3c1066936b98c747acaf6",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x00056602,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0005681F,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x000569CF,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.2.0.26389",
                                SHA1Before = "6f8855fb80acfa456f8f69989fe949308fe4d154",
                                SHA1After = "f6b8e05a178b9b10ba17c597fa0a44b7a2a966a8",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x000571C8,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x000573E5,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x00057595,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.1.9.26361",
                                SHA1Before = "022d3433d13d07a354c38816f61cb0b7ac60d3fd",
                                SHA1After = "873a57c1fb51cdd099c8cb7108b5ab5cb4459557",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x000567DE,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x000569FB,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x00056BAB,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.1.9.26346",
                                SHA1Before = "895eb70f707b8222e6460c91492b1281e525059b",
                                SHA1After = "0bb83990e2b5b5f23b7b43249941ff638201af54",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x000567DE,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x000569FB,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x00056BAB,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.1.8.26211",
                                SHA1Before = "a950d3cf5e8925f7775624271105ef78d9c5cb57",
                                SHA1After = "dffc1cb87b91e6467e13c935611f2f7fd76b9a8d",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x000524EF,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0005270C,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x000528BC,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.1.7.25980",
                                SHA1Before = "c6632339fbe675312a70ae4620e70699c258cd36",
                                SHA1After = "e9ddc5cc681950796fc8fe4c55f580428c890b51",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x0005009F,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x000502BC,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0005046C,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "9.0.4.23786",
                                SHA1Before = "69a714f4eadb09f1453f6f022d4adbcd801cfab8",
                                SHA1After = "b48e77a924076b3ebdffc4af514c868c551d2bca",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x0004DB71,
                                        Content = new byte[] { 0xEB, 0x07, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0004DD8E,
                                        Content = new byte[] { 0xEB, 0x07, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0004DF93,
                                        Content = new byte[] { 0xEB, 0x07, 0x90, 0x90, 0x90 }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public App TIM()
        {
            return new App
            {
                Name = "TIM",
                FileTargetInfos = new Dictionary<string, TargetInfo>
                {
                    {
                        "IM.dll",
                        new TargetInfo
                        {
                            Name = "IM.dll",
                            RelativePath = @"Bin\IM.dll"
                        }
                    }
                },
                FileCommonModifyInfos = new Dictionary<string, List<CommonModifyInfo>>
                {
                    {
                        "IM.dll",
                        new List<CommonModifyInfo>
                        {
                            new CommonModifyInfo
                            {
                                Name="IM.dll",
                                StartVersion="3.4.0.00000",
                                EndVersion="",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC 68 3F 3F 3F 3F 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC EB 09 90 90 90 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 F0 8D 55 EC 52 89 5D EC 68 3F 3F 3F 3F 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 F0 8D 55 EC 52 89 5D EC EB 09 90 90 90 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("8B 75 14 8D 4D F4 83 C4 20 33 FF 89 7D F4 8B 06 51 68 3F 3F 3F 3F 56 FF 50 78 85 C0 79 39 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("8B 75 14 8D 4D F4 83 C4 20 33 FF 89 7D F4 8B 06 EB 08 90 90 90 90 56 FF 50 78 85 C0 79 39 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    }
                                }
                            },
                            new CommonModifyInfo
                            {
                                Name="IM.dll",
                                StartVersion="3.0.0.00000",
                                EndVersion="3.4.0.00000",
                                ReplacePatterns = new List<ReplacePattern>
                                {
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC 68 3F 3F 3F 54 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 E8 8D 55 EC 52 89 5D EC EB 09 90 90 90 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 F0 8D 55 EC 52 89 5D EC 68 3F 3F 3F 54 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("1C E9 9D 00 00 00 8B 45 F0 8D 55 EC 52 89 5D EC EB 09 90 90 90 8B 08 50 FF 51 78 85 C0 79 2D 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    },
                                    new ReplacePattern
                                    {
                                        Search = ByteUtil.HexStringToByteArray("8B 75 14 8D 4D F4 83 C4 20 33 FF 89 7D F4 8B 06 51 68 3F 3F 3F 54 56 FF 50 78 85 C0 79 39 8D 45 0C C7 45 0C"),
                                        Replace = ByteUtil.HexStringToByteArray("8B 75 14 8D 4D F4 83 C4 20 33 FF 89 7D F4 8B 06 EB 08 90 90 90 90 56 FF 50 78 85 C0 79 39 8D 45 0C C7 45 0C"),
                                        Category = "防撤回"
                                    }
                                }
                            }
                        }
                    }
                },
                FileModifyInfos = new Dictionary<string, List<ModifyInfo>>
                {
                    {
                        "IM.dll",
                        new List<ModifyInfo>
                        {
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "2.3.2.21173",
                                SHA1Before = "ecf3e69f3fb100ffe2fee095ffded591b9781024",
                                SHA1After = "0514d1304e7ac46b4d33386ec3313888f5ae7171",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x0004D78A,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0004D9A7,
                                        Content = new byte[] { 0xEB, 0x09, 0x90, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x0004DB57,
                                        Content = new byte[] { 0xEB, 0x08, 0x90, 0x90, 0x90, 0x90 }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public App QQLite()
        {
            return new App
            {
                Name = "QQLite",
                FileTargetInfos = new Dictionary<string, TargetInfo>
                {
                    {
                        "IM.dll",
                        new TargetInfo
                        {
                            Name = "IM.dll",
                            RelativePath = @"Bin\IM.dll"
                        }
                    }
                },
                FileModifyInfos = new Dictionary<string, List<ModifyInfo>>
                {
                    {
                        "IM.dll",
                        new List<ModifyInfo>
                        {
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "7.9.14314.0",
                                SHA1Before = "2e97d7671963fa148a1beeda6ce4964314310593",
                                SHA1After = "723c008fb53435ead20fa6f2e951c9a4a8ff46da",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x00024505,
                                        Content = new byte[] { 0xEB, 0x02, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x000248B9,
                                        Content = new byte[] { 0xEB, 0x02, 0x90, 0x90 }
                                    }
                                }
                            },
                            new ModifyInfo
                            {
                                Name = "IM.dll",
                                Version = "7.9.14308.0",
                                SHA1Before = "b8a7a873178706b97be11c25f13bcf09e9e578a2",
                                SHA1After = "c5bf533c7af6996b42d1fb2a0fb3f26dfd52f8bf",
                                Changes = new List<Change>
                                {
                                    new Change
                                    {
                                        Position = 0x00024505,
                                        Content = new byte[] { 0xEB, 0x02, 0x90, 0x90 }
                                    },
                                    new Change
                                    {
                                        Position = 0x000248B9,
                                        Content = new byte[] { 0xEB, 0x02, 0x90, 0x90 }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
