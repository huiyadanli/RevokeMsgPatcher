using RevokeMsgPatcher.Model;

namespace RevokeMsgPatcher.Modifier
{
    public static class ModifierFactory
    {
        public static IAppModifier CreateModifier(string appName, App config)
        {
            switch (appName)
            {
                case "WeChat":
                    return new WechatModifier(config);
                case "Weixin":
                    return new WeixinModifier(config);
                case "QQ":
                    return new QQModifier(config);
                case "TIM":
                    return new TIMModifier(config);
                case "QQLite":
                    return new QQLiteModifier(config);
                case "QQNT":
                    return new QQNTModifier(config);
                default:
                    throw new System.ArgumentException($"Unknown application name: {appName}");
            }
        }
    }
}
