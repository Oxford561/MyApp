using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using XLua;

namespace YouYou
{

    public enum LogColor
    {
        black,
        green,
        yellow,
        red,
    }
    /// <summary>
    /// 日志管理器
    /// </summary>
    public class LoggerManager : IDisposable
    {
        #region 接入 report 写入文件
        private List<string> m_LogArray;

        private string m_LogPath = null;
        private string ReporterPath = Application.persistentDataPath + "//Reporter";
        private int m_LogMaxCapacity = 500;
        private int m_CurrLogCount = 0;

        private int m_LogBufferMaxNumber = 10;
        #endregion

        // 普通调试日志开关
        public bool s_debugLogEnable = true;
        // 警告日志开关
        public bool s_warningLogEnable = true;
        // 错误日志开关
        public bool s_errorLogEnable = true;

        // 优化 字符串重复构造
        private StringBuilder s_logStr = new StringBuilder();
        // 日志文件存储位置
        private string s_logFileSavePath;

        internal void Init()
        {
            m_LogArray = new List<string>();

            if (string.IsNullOrEmpty(m_LogPath))
            {
                m_LogPath = ReporterPath + "//" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + "-Start.txt";
            }

            var t = System.DateTime.Now.ToString("yyyyMMddhhmmss");
            s_logFileSavePath = string.Format("{0}/output_{1}.log", Application.persistentDataPath, t);
            Application.logMessageReceived += OnLogCallBack;

            Debug.Log("Logger Init");
        }

        // d打印日志回调
        private void OnLogCallBack(string condition, string stackTrace, LogType type)
        {
            s_logStr.Append(condition);
            s_logStr.Append("\n");
            s_logStr.Append(stackTrace);
            s_logStr.Append("\n");

            if (s_logStr.Length <= 0) return;
            if (!File.Exists(s_logFileSavePath))
            {
                var fs = File.Create(s_logFileSavePath);
                fs.Close();
            }
            using (var sw = File.AppendText(s_logFileSavePath))
            {
                sw.WriteLine(s_logStr.ToString());
            }

            s_logStr.Remove(0, s_logStr.Length);
        }

        public static void UploadLog(string desc)
        {
            // TODO
        }

        #region 正常日志功能封装

        public void Log(object message, UnityEngine.Object context = null)
        {
            if(!s_debugLogEnable) return;
            Debug.Log(message,context);
        }

        public void LogFormat(string format, params object[] args)
        {
            if (!s_debugLogEnable) return;
            Debug.LogFormat(format, args);
        }

        public void LogWithColor(object message, LogColor color, UnityEngine.Object context = null)
        {
            if (!s_debugLogEnable) return;
            switch (color)
            {
                case LogColor.black:
                    Debug.LogFormat("<color={0}>{1}</color>", color.ToString(), message, context);
                    break;
                case LogColor.green:
                    Debug.LogFormat("<color={0}>{1}</color>", color.ToString(), message, context);
                    break;
                case LogColor.yellow:
                    Debug.LogFormat("<color={0}>{1}</color>", color.ToString(), message, context);
                    break;
                case LogColor.red:
                    Debug.LogFormat("<color={0}>{1}</color>", color.ToString(), message, context);
                    break;
            }
        }

        public void LogWarning(object message, UnityEngine.Object context = null)
        {
            if (!s_warningLogEnable) return;
            Debug.LogWarning(message, context);
        }

        public void LogError(object message, UnityEngine.Object context = null)
        {
            if (!s_errorLogEnable) return;
            Debug.LogError(message, context);
        }

        #endregion

        #region 解决日志双击溯源的问题

#if UNITY_EDITOR
        [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
        static bool OnOpenAsset(int instanceID, int line)
        {
            string stackTrace = GetStackTrace();
            if (!string.IsNullOrEmpty(stackTrace) && stackTrace.Contains("LoggerManager:Log"))
            {
                // 使用正则表达式匹配at的哪个脚本的哪一行
                var matches = System.Text.RegularExpressions.Regex.Match(stackTrace, @"\(at (.+)\)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                string pathLine = "";
                while (matches.Success)
                {
                    pathLine = matches.Groups[1].Value;
                    if (!pathLine.Contains("LoggerManager.cs"))
                    {
                        int splitIndex = pathLine.LastIndexOf(":");
                        // 脚本路径
                        string path = pathLine.Substring(0, splitIndex);
                        // 行号
                        line = System.Convert.ToInt32(pathLine.Substring(splitIndex + 1));
                        string fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                        fullPath = fullPath + path;
                        // 跳转到目标代码的特定行
                        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullPath.Replace('/', '\\'), line);
                        break;
                    }
                    matches = matches.NextMatch();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取当前日志窗口选中的日志的堆栈信息
        /// </summary>
        /// <returns></returns>
        static string GetStackTrace()
        {
            // 通过反射获取ConsoleWindow类
            var ConsoleWindowType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            // 获取窗口实例
            var fieldInfo = ConsoleWindowType.GetField("ms_ConsoleWindow",
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.NonPublic);
            var consoleInstance = fieldInfo.GetValue(null);
            if (consoleInstance != null)
            {
                if ((object)UnityEditor.EditorWindow.focusedWindow == consoleInstance)
                {
                    // 获取m_ActiveText成员
                    fieldInfo = ConsoleWindowType.GetField("m_ActiveText",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic);
                    // 获取m_ActiveText的值
                    string activeText = fieldInfo.GetValue(consoleInstance).ToString();
                    return activeText;
                }
            }
            return null;
        }
#endif
        #endregion

        #region 接入 Report 写入日志文件
        public void Write(string writeFileData, LogType type)
        {
            if (m_CurrLogCount >= m_LogMaxCapacity)
            {
                m_LogPath = ReporterPath + "//" + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".txt";
                m_LogMaxCapacity = 0;
            }
            m_CurrLogCount++;

            if (!string.IsNullOrEmpty(writeFileData))
            {
                writeFileData = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff") + "|" + type.ToString() + "|" + writeFileData + "\r\n";
                AppendDataToFile(writeFileData);
            }
        }

        private void AppendDataToFile(string writeFileDate)
        {
            if (!string.IsNullOrEmpty(writeFileDate))
            {
                m_LogArray.Add(writeFileDate);
            }

            if (m_LogArray.Count % m_LogBufferMaxNumber == 0)
            {
                SyncLog();
            }
        }

        private void CreateFile(string pathAndName, string info)
        {
            if (!Directory.Exists(ReporterPath)) Directory.CreateDirectory(ReporterPath);

            StreamWriter sw;
            FileInfo t = new FileInfo(pathAndName);
            if (!t.Exists)
            {
                sw = t.CreateText();
            }
            else
            {
                sw = t.AppendText();
            }

            sw.WriteLine(info);

            sw.Close();

            sw.Dispose();
        }

        private void ClearLogArray()
        {
            if (m_LogArray != null)
            {
                m_LogArray.Clear();
            }
        }

        public void SyncLog()
        {
            if (!string.IsNullOrEmpty(m_LogPath))
            {
                int len = m_LogArray.Count;
                for (int i = 0; i < len; i++)
                {
                    CreateFile(m_LogPath, m_LogArray[i]);
                }
                ClearLogArray();
            }
        }
        #endregion

        public void Dispose()
        {
            m_LogArray.Clear();
        }
    }
}