using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// UI�㼶����
    /// </summary>
    public class UILayer
    {
        private Dictionary<byte, ushort> m_UILayerDic;

        public UILayer()
        {
            m_UILayerDic = new Dictionary<byte, ushort>();
        }

        /// <summary>
        /// ��ʼ����������
        /// </summary>
        /// <param name="groups"></param>
        internal void Init(UIGroup[] groups)
        {
            int len = groups.Length;
            for (int i = 0; i < len; i++)
            {
                UIGroup group = groups[i];
                m_UILayerDic[group.Id] = group.BaseOrder;
            }
        }

        /// <summary>
        /// ���ò㼶
        /// </summary>
        /// <param name="formBase">����</param>
        /// <param name="isAdd">true:����  false:����</param>
        internal void SetSortingOrder(UIFormBase formBase, bool isAdd)
        {
            if (!m_UILayerDic.ContainsKey(formBase.SysUIForm.UIGroupId)) return;

            if (isAdd)
            {
                m_UILayerDic[formBase.SysUIForm.UIGroupId] += 10;
            }
            else
            {
                if (formBase.CurrCanvas.sortingOrder == m_UILayerDic[formBase.SysUIForm.UIGroupId])
                {
                    m_UILayerDic[formBase.SysUIForm.UIGroupId] -= 10;
                }
            }

            formBase.CurrCanvas.sortingOrder = m_UILayerDic[formBase.SysUIForm.UIGroupId];
        }
    }
}