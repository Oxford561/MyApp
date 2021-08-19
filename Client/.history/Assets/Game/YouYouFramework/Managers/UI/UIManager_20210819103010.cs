using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace YouYou
{
    public class UIManager
    {
        /// <summary>
        /// 已经打开的UI窗口链表
        /// </summary>
        private LinkedList<UIFormBase> m_OpenUIFormList;
        /// <summary>
        /// 反切UI栈
        /// </summary>
        private Stack<UIFormBase> m_ReverseChangeUIStack;
        /// <summary>
        /// 正在加载中的UI窗口
        /// </summary>
        private LinkedList<int> m_LoadingUIFormList;

        private UILayer m_UILayer;

        private Dictionary<byte, UIGroup> m_UIGroupDic;

        private UIPool m_UIPool;

        /// <summary>
        /// UI回池后过期时间_秒
        /// </summary>
        public float UIExpire { get; private set; }
        /// <summary>
        /// UI释放间隔_秒
        /// </summary>
        public float ClearInterval { get; private set; }

        /// <summary>
        /// 下次运行时间
        /// </summary>
        private float m_NextRunTime = 0f;

        /// <summary>
        /// 标准分辨率比值
        /// </summary>
        private float m_StandardScreen = 0;
        /// <summary>
        /// 当前分辨率比值
        /// </summary>
        private float m_CurrScreen = 0;

        internal UIManager()
        {
            m_OpenUIFormList = new LinkedList<UIFormBase>();
            m_ReverseChangeUIStack = new Stack<UIFormBase>();
            m_LoadingUIFormList = new LinkedList<int>();

            m_UILayer = new UILayer();
            m_UIGroupDic = new Dictionary<byte, UIGroup>();
            m_UIPool = new UIPool();

        }
        internal void Dispose()
        {
        }
        internal void Init()
        {
            UIExpire = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_Expire, GameEntry.CurrDeviceGrade);
            ClearInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_ClearInterval, GameEntry.CurrDeviceGrade);

            m_StandardScreen = GameEntry.Instance.StandardWidth / (float)GameEntry.Instance.StandardHeight;

            for (int i = 0; i < GameEntry.Instance.UIGroups.Length; i++)
            {
                m_UIGroupDic[GameEntry.Instance.UIGroups[i].Id] = GameEntry.Instance.UIGroups[i];
            }
            m_UILayer.Init(GameEntry.Instance.UIGroups);
        }
        internal void OnUpdate()
        {
            if (Time.time > m_NextRunTime + ClearInterval)
            {
                m_NextRunTime = Time.time;

                //释放UI对象池
                m_UIPool.CheckClear();
            }

            if (m_CurrScreen != Screen.width / (float)Screen.height)
            {
                m_CurrScreen = Screen.width / (float)Screen.height;
                LoadingFormCanvasScaler();
            }
        }

        #region UI适配
        /// <summary>
        /// LoadingForm适配缩放
        /// </summary>
        public void LoadingFormCanvasScaler()
        {
            GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = (m_CurrScreen > m_StandardScreen) ? 1 : 0;
        }
        /// <summary>
        /// FullForm适配缩放
        /// </summary>
        public void FullFormCanvasScaler()
        {
            GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = 1;
        }
        /// <summary>
        /// NormalForm适配缩放
        /// </summary>
        public void NormalFormCanvasScaler()
        {
            if (m_CurrScreen > m_StandardScreen)
            {
                //设置为0
                GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = 0;
            }
            else
            {
                GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = m_StandardScreen - m_CurrScreen;
            }
        }

        #endregion

        #region GetUIGroup 根据UI分组编号获取UI分组
        /// <summary>
        /// 根据UI分组编号获取UI分组
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UIGroup GetUIGroup(byte id)
        {
            UIGroup group = null;
            m_UIGroupDic.TryGetValue(id, out group);
            return group;
        }
        #endregion

        #region OpenUIForm 打开UI窗口
        public void OpenUIForm(string uiFormName, object userData = null, Action<UIFormBase> onOpen = null, Action<UIFormBase> onLoadComplete = null)
        {
            OpenUIForm<UIFormBase>(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName), userData, onOpen, onLoadComplete);
        }
        public void OpenUIForm(int uiFormId, object userData = null, Action<UIFormBase> onOpen = null, Action<UIFormBase> onLoadComplete = null)
        {
            OpenUIForm<UIFormBase>(uiFormId, userData, onOpen, onLoadComplete);
        }
        public void OpenUIForm<T>(string uiFormName, object userData = null, Action<T> onOpen = null, Action<UIFormBase> onLoadComplete = null) where T : UIFormBase
        {
            OpenUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName), userData, onOpen, onLoadComplete);
        }
        public void OpenUIForm<T>(int uiFormId, object userData = null, Action<T> onOpen = null, Action<UIFormBase> onLoadComplete = null) where T : UIFormBase
        {
            //1,读表
            Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetDic(uiFormId);
            Debug.Log("进入这里1");
            if (sys_UIForm == null) return;
            Debug.Log("进入这里2");
            if (sys_UIForm.CanMulit == 0 && IsExists(uiFormId)) return;
            Debug.Log("进入这里3");

            UIFormBase formBase = GameEntry.UI.Dequeue(uiFormId);
            if (formBase == null)
            {
                Debug.Log("进入这里4");
                GameEntry.Task.AddTaskCommon((taskRoutine) =>
                {
                    LoadAssetUI(sys_UIForm, (form) =>
                    {
                        taskRoutine.Leave();
                        m_OpenUIFormList.AddLast(form);
                        onLoadComplete?.Invoke(form);
                        Debug.Log("执行 form init");
                        Debug.Log(form.Desc);
                        form.Init(sys_UIForm, userData, () => OpenUI(sys_UIForm, form, onOpen));
                    });
                }, sys_UIForm.LoadType == 1);
            }
            else
            {
                Debug.Log("进入这里5");
                m_OpenUIFormList.AddLast(formBase);
                GameEntry.UI.ShowUI(formBase);
                //Yield是为了防止OnInit没有被执行
                GameEntry.Time.Yield(() =>
                {
                    OpenUI(sys_UIForm, formBase, onOpen);
                    formBase.Open(userData);
                });
            }
        }
        private void LoadAssetUI(Sys_UIFormEntity sys_UIForm, Action<UIFormBase> onComplete = null)
        {
            //异步加载UI需要时间 此处需要处理过滤加载中的UI
            if (sys_UIForm.CanMulit == 0 && IsLoading(sys_UIForm.Id))
            {
                onComplete?.Invoke(null);
                return;
            }
            m_LoadingUIFormList.AddLast(sys_UIForm.Id);

            string assetPath = string.Empty;
            switch (GameEntry.CurrLanguage)
            {
                case YouYouLanguage.Chinese:
                    assetPath = sys_UIForm.AssetPath_Chinese;
                    break;
                case YouYouLanguage.English:
                    assetPath = sys_UIForm.AssetPath_English;
                    break;
            }

            //加载UI资源并克隆
            StringBuilder sbr = StringHelper.PoolNew();
            string str = sbr.AppendFormatNoGC("Assets/Download/UI/UIPrefab/{0}.prefab", assetPath).ToString();
            StringHelper.PoolDel(ref sbr);
            GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(str, isAddReferenceCount: true, onComplete: (ResourceEntity resourceEntity) =>
            {
                GameObject uiObj = UnityEngine.Object.Instantiate((GameObject)resourceEntity.Target, GameEntry.UI.GetUIGroup(sys_UIForm.UIGroupId).Group);
                //把克隆出来的资源 加入实例资源池
                GameEntry.Pool.RegisterInstanceResource(uiObj.GetInstanceID(), resourceEntity);

                //初始化UI
                UIFormBase formBase = uiObj.GetComponent<UIFormBase>();
                if (formBase == null) formBase = uiObj.AddComponent<UIFormBase>();
                formBase.CurrCanvas.overrideSorting = true;
                m_LoadingUIFormList.Remove(sys_UIForm.Id);
                onComplete?.Invoke(formBase);
            });
        }
        private void OpenUI<T>(Sys_UIFormEntity sys_UIFormEntity, UIFormBase formBase, Action<T> onOpen) where T : UIFormBase
        {
            //判断反切UI
            UIFormShowMode uIFormShowMode = (UIFormShowMode)sys_UIFormEntity.ShowMode;
            if (uIFormShowMode == UIFormShowMode.ReverseChange)
            {
                //如果之前栈里面有UI
                if (m_ReverseChangeUIStack.Count > 0)
                {
                    //从栈顶上 拿到UI
                    UIFormBase topUIForm = m_ReverseChangeUIStack.Peek();

                    //禁用 冻结
                    GameEntry.UI.HideUI(topUIForm);
                }

                //把自己加入栈
                //Debug.LogError("入栈==" + formBase.gameObject.GetInstanceID());
                m_ReverseChangeUIStack.Push(formBase);
            }
            onOpen?.Invoke(formBase as T);
        }
        #endregion

        #region CloseUIForm 关闭UI窗口
        public void CloseUIForm(int uiFormId)
        {
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId)
                {
                    CloseUIForm(curr.Value);
                    break;
                }
            }
        }
        internal void CloseUIFormByInstanceID(int instanceID)
        {
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.gameObject.GetInstanceID() == instanceID)
                {
                    CloseUIForm(curr.Value);
                    break;
                }
            }
        }
        internal void CloseUIForm(UIFormBase formBase)
        {
            if (!m_OpenUIFormList.Remove(formBase)) return;
            formBase.ToClose();

            //判断反切UI
            UIFormShowMode uIFormShowMode = (UIFormShowMode)formBase.SysUIForm.ShowMode;
            if (uIFormShowMode == UIFormShowMode.ReverseChange)
            {
                m_ReverseChangeUIStack.Pop();

                if (m_ReverseChangeUIStack.Count > 0)
                {
                    UIFormBase topForms = m_ReverseChangeUIStack.Peek();
                    GameEntry.UI.ShowUI(topForms);
                }
            }
        }
        /// <summary>
        /// 关闭UI窗口
        /// </summary>
        /// <param name="uiFormName"></param>
        public void CloseUIForm(string uiFormName)
        {
            CloseUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName));
        }
        /// <summary>
        /// 关闭所有"Default"组的UI窗口
        /// </summary>
        public void CloseAllDefaultUIForm()
        {
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                UIFormBase formBase = curr.Value;
                if (formBase.SysUIForm.UIGroupId != 2) continue;
                formBase.ToClose();
            }
            m_OpenUIFormList.Clear();
            m_ReverseChangeUIStack.Clear();
        }

        #endregion

        /// <summary>
        /// 显示/激活一个UI
        /// </summary>
        /// <param name="uIFormBase"></param>
        public void ShowUI(UIFormBase uiFormBase)
        {
            if (uiFormBase.SysUIForm.FreezeMode == 0 && uiFormBase.SysUIForm.LoadType != 2)
            {
                uiFormBase.IsActive = true;
                uiFormBase.CurrCanvas.enabled = true;
                uiFormBase.gameObject.layer = 5;
            }
            else
            {
                uiFormBase.gameObject.SetActive(true);
            }
            //Debug.LogError("显示 " + uIFormBase.gameObject.GetInstanceID());
        }
        /// <summary>
        /// 隐藏/冻结一个UI
        /// </summary>
        /// <param name="uIFormBase"></param>
        public void HideUI(UIFormBase uiFormBase)
        {
            if (uiFormBase.SysUIForm.FreezeMode == 0 && uiFormBase.SysUIForm.LoadType != 2)
            {
                uiFormBase.IsActive = false;
                uiFormBase.CurrCanvas.enabled = false;
                uiFormBase.gameObject.layer = 0;
            }
            else
            {
                uiFormBase.gameObject.SetActive(false);
            }
            //Debug.LogError("隐藏 " + uIFormBase.gameObject.GetInstanceID());
        }

        /// <summary>
        /// 预加载UI
        /// </summary>
        public void PreloadUI(Sys_UIFormEntity sys_UIForm, Action onComplete = null)
        {
            if (sys_UIForm == null)
            {
                onComplete?.Invoke();
                return;
            }
            if (sys_UIForm.CanMulit == 0 && IsExists(sys_UIForm.Id))
            {
                onComplete?.Invoke();
                return;
            }
            UIFormBase formBase = GameEntry.UI.Dequeue(sys_UIForm.Id);
            if (formBase == null)
            {
                LoadAssetUI(sys_UIForm, (form) =>
                {
                    form.Init(sys_UIForm, null, null);
                    GameEntry.UI.EnQueue(form);
                    onComplete?.Invoke();
                });
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        public T GetUIForm<T>(string uiFormName) where T : UIFormBase
        {
            int uiFormId = GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName);
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId) return curr.Value as T;
            }
            return null;
        }

        /// <summary>
        /// 设置层级
        /// </summary>
        /// <param name="formBase">窗口</param>
        /// <param name="isAdd">true:增加  false:减少</param>
        internal void SetSortingOrder(UIFormBase formBase, bool isAdd)
        {
            m_UILayer.SetSortingOrder(formBase, isAdd);
        }

        /// <summary>
        /// 从池中获取UI窗口
        /// </summary>
        /// <param name="uiFormId"></param>
        /// <returns></returns>
        internal UIFormBase Dequeue(int uiFormId)
        {
            return m_UIPool.Dequeue(uiFormId);
        }

        /// <summary>
        /// UI窗口回池
        /// </summary>
        /// <param name="form"></param>
        internal void EnQueue(UIFormBase form)
        {
            m_UIPool.EnQueue(form);
        }

        /// <summary>
        /// 检查UI是否已经打开
        /// </summary>
        /// <param name="uiFormId"></param>
        /// <returns></returns>
        private bool IsExists(int uiFormId)
        {
            for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value.SysUIForm.Id == uiFormId)
                {
                    Debug.LogError("不重复打开同一个UI窗口==" + uiFormId);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 检查UI正在加载中
        /// </summary>
        private bool IsLoading(int uiFormId)
        {
            for (LinkedListNode<int> curr = m_LoadingUIFormList.First; curr != null; curr = curr.Next)
            {
                if (curr.Value == uiFormId)
                {
                    GameEntry.LogError("UI=={0}正在加载中, 打开的频率过高", uiFormId);
                    return true;
                }
            }
            return false;
        }


    }
}