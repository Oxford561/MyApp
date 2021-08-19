using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// 多语言枚举
    /// </summary>
    public enum YouYouLanguage
    {
        /// <summary>
        /// 中文
        /// </summary>
        Chinese = 0,
        /// <summary>
        /// 英文
        /// </summary>
        English = 1
    }


    public class LocalizationManager : IDisposable
    {
        internal void Init()
        {
#if !UNITY_EDITOR
            switch (Application.systemLanguage)
            {
                default:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                case SystemLanguage.Chinese:
                    GameEntry.CurrLanguage = YouYouLanguage.Chinese;
                    break;
                case SystemLanguage.English:
                    GameEntry.CurrLanguage = YouYouLanguage.English;
                    break;
            }
#endif
        }

        /// <summary>
        /// 获取本地化文本内容
        /// </summary>
        /// <param name="key"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetString(string key, params object[] args)
        {
            string value = null;
            if (GameEntry.DataTable.LocalizationDBModel.LocalizationDic.TryGetValue(key, out value))
            {
                return string.Format(value, args);
            }
            return value;
        }

        public void Dispose()
        {

        }


    }
}