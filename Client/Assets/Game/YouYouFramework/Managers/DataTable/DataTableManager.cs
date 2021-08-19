using System;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    public class DataTableManager : IDisposable
    {
        internal void Init()
        {
            InitDBModel();
            m_TaskGroup = GameEntry.Task.CreateTaskGroup();
        }

        private TaskGroup m_TaskGroup;
        internal Action OnLoadDataTableComplete;


        public Sys_CodeDBModel Sys_CodeDBModel { get; private set; }
        public LocalizationDBModel LocalizationDBModel { get; private set; }
        public Sys_PrefabDBModel Sys_PrefabDBModel { get; private set; }
        public Sys_UIFormDBModel Sys_UIFormDBModel { get; private set; }
        public Sys_SceneDBModel Sys_SceneDBModel { get; private set; }
        public Sys_SceneDetailDBModel Sys_SceneDetailDBModel { get; private set; }
        public Sys_AudioDBModel Sys_AudioDBModel { get; private set; }
        public Sys_AnimationDBModel Sys_AnimationDBModel { get; private set; }


        internal void InitDBModel()
        {
            Sys_CodeDBModel = new Sys_CodeDBModel();
            LocalizationDBModel = new LocalizationDBModel();
            Sys_PrefabDBModel = new Sys_PrefabDBModel();
            Sys_UIFormDBModel = new Sys_UIFormDBModel();
            Sys_SceneDBModel = new Sys_SceneDBModel();
            Sys_SceneDetailDBModel = new Sys_SceneDetailDBModel();
            Sys_AudioDBModel = new Sys_AudioDBModel();
            Sys_AnimationDBModel = new Sys_AnimationDBModel();
        }
        /// <summary>
        /// 加载表格
        /// </summary>
        private void LoadDataTable()
        {
            Sys_CodeDBModel.LoadData(m_TaskGroup);
            LocalizationDBModel.LoadData(m_TaskGroup);
            Sys_PrefabDBModel.LoadData(m_TaskGroup);
            Sys_UIFormDBModel.LoadData(m_TaskGroup);
            Sys_SceneDBModel.LoadData(m_TaskGroup);
            Sys_SceneDetailDBModel.LoadData(m_TaskGroup);
            Sys_AudioDBModel.LoadData(m_TaskGroup);
            Sys_AnimationDBModel.LoadData(m_TaskGroup);

            m_TaskGroup.OnComplete = OnLoadDataTableComplete;
            m_TaskGroup.Run(true);
        }

        /// <summary>
        /// 表格资源包
        /// </summary>
        private AssetBundle m_DataTableBundle;

        /// <summary>
        /// 加载表格
        /// </summary>
        internal void LoadDataAllTable(Action onComplete = null)
        {
            OnLoadDataTableComplete = onComplete;
#if ASSETBUNDLE
            GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.DataTableAssetBundlePath, onComplete: (AssetBundle bundle) =>
            {
                m_DataTableBundle = bundle;
                LoadDataTable();
            });
#else
            LoadDataTable();
#endif
        }

        /// <summary>
        /// 获取表格的字节数组
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public void GetDataTableBuffer(string dataTableName, Action<byte[]> onComplete)
        {
#if EDITORLOAD
            GameEntry.Time.Yield(() =>
            {
                byte[] buffer = IOUtil.GetFileBuffer(string.Format("{0}/Download/DataTable/{1}.bytes", GameEntry.Resource.LocalFilePath, dataTableName));
                if (onComplete != null) onComplete(buffer);
            });
#elif RESOURCES
			GameEntry.Time.Yield(() =>
			{
				TextAsset asset = Resources.Load<TextAsset>(string.Format("DataTable/{0}", dataTableName));
				if (onComplete != null) onComplete(asset.bytes);
			});
#else
            GameEntry.Resource.ResourceLoaderManager.LoadAsset(GameEntry.Resource.GetLastPathName(dataTableName), m_DataTableBundle, onComplete: (UnityEngine.Object obj, bool isNew) =>
            {
                if (obj == null) return;
                TextAsset asset = obj as TextAsset;
                if (onComplete != null) onComplete(asset.bytes);
            });
#endif
        }

        public void Dispose()
        {
            Sys_CodeDBModel.Clear();
            LocalizationDBModel.Clear();
            Sys_PrefabDBModel.Clear();
            Sys_UIFormDBModel.Clear();
            Sys_SceneDBModel.Clear();
            Sys_SceneDetailDBModel.Clear();
            Sys_AnimationDBModel.Clear();
        }
    }
}