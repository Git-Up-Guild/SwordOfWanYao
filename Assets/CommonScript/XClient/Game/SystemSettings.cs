using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame.Def;
using XGame.Quality;
using XGame.SysSetting;
/// <summary>
/// 提供系统设置的接口
/// </summary>
namespace XClient.Game
{
    public static class SystemSettings
    {
        const string MusicVolume = "MusicVolume";
        const string SFXMusicVolume = "SFXMusicVolume";
        const string GraphicQuality = "GraphicQuality";
        static int BgMusicGroupType = 1;//背景音乐组
        static int[] SFXMusicGroupTypes = { 2, 3, 4, 5 }; //音效组
        /// <summary>
        /// 设置音乐音量
        /// </summary>
        public static void SetMusicVolume(float volume)
        {
            var sysCom = XGame.XGameComs.Get<ISysSettingCom>();
            sysCom.SetFloat(MusicVolume, volume, true);
        }
        public static float GetMusicVolume()
        {
            return XGame.XGameComs.Get<ISysSettingCom>().GetFloat(MusicVolume);
        }
        /// <summary>
        /// 设置音效音量
        /// </summary>
        /// <param name="volume"></param>
        public static void SetSFXMusicVolume(float volume)
        {
            var sysCom = XGame.XGameComs.Get<ISysSettingCom>();
            sysCom.SetFloat(SFXMusicVolume, volume, true);
        }
        public static float GetSFXMusicVolume()
        {
            return XGame.XGameComs.Get<ISysSettingCom>().GetFloat(SFXMusicVolume);
        }

        // <summary>
        /// 设置画质
        /// </summary>
        public static void SetGraphicsQuality(int quality)
        {
            XGame.XGameComs.Get<ISysSettingCom>().SetInt(GraphicQuality, quality);
        }

        public static int GetGraphicsQuality()
        {
            return XGame.XGameComs.Get<ISysSettingCom>().GetInt(GraphicQuality);
        }

        //画质修改响应
        public static void OnSystemSettingChanged(List<string> lsOptionNames)
        {
            foreach (var opt in lsOptionNames)
            {
                if (opt == MusicVolume)
                {
                    float volume = XGame.XGameComs.Get<ISysSettingCom>().GetFloat(MusicVolume);
                    GameGlobal.AudioCom.SetAudioGroupVolume(BgMusicGroupType, volume);
                }
                else if (opt == SFXMusicVolume)
                {
                    float volume = XGame.XGameComs.Get<ISysSettingCom>().GetFloat(SFXMusicVolume);
                    foreach (int type in SFXMusicGroupTypes)//有多种音效种类
                        GameGlobal.AudioCom.SetAudioGroupVolume(type, volume);
                }
                else if (opt == GraphicQuality)
                {
                    int quality = XGame.XGameComs.Get<ISysSettingCom>().GetInt(GraphicQuality);
                    EMPerformanceLevel[] levels = new EMPerformanceLevel[] { EMPerformanceLevel.Lower, EMPerformanceLevel.Low, EMPerformanceLevel.Middle, EMPerformanceLevel.High };
                    XGame.XGameComs.Get<IQualityCom>().SetPerformanceLevel(levels[quality]);
                }
            }
        }

        //初始化系统设置
        public static void InitSystemSettings()
        {
            OnSystemSettingChanged(new List<string>() { MusicVolume, SFXMusicVolume, GraphicQuality });
        }
    }

}