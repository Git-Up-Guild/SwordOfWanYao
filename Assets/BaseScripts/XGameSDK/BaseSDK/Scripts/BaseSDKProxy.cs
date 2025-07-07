
using XGame.AssetScript.SDK.Core;

namespace XGame.AssetScript.SDK.Base
{
    public static class BaseSDKProxy
    {
        private static IBaseSDK s_sdk;
        public static IBaseSDK sdk
        {
            get
            {
                if (s_sdk == null)
                {
#if UNITY_EDITOR
                    s_sdk = GameSDKManager.Instance.RegisterSDK<BaseSDK_Default>() as IBaseSDK;
#elif UNITY_ANDROID
                    s_sdk = GameSDKManager.Instance.RegisterSDK<BaseSDK_Android>() as IBaseSDK;
#elif UNITY_IOS || UNITY_IPHONE
                    s_sdk = GameSDKManager.Instance.RegisterSDK<BaseSDK_IOS>() as IBaseSDK;
#else
                    s_sdk = GameSDKManager.Instance.RegisterSDK<BaseSDK_Default>() as IBaseSDK;
#endif
                }

                return s_sdk;
            }
        }
    }
}
