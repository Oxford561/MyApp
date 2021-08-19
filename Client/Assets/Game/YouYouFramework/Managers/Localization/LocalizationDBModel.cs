using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{

    /// <summary>
    /// LocalizationDBModel���ݹ���
    /// </summary>
    public partial class LocalizationDBModel : DataTableDBModelBase<LocalizationDBModel, DataTableEntityBase>
    {
        /// <summary>
        /// �ļ�����
        /// </summary>
        public override string DataTableName { get { return "Localization/" + GameEntry.CurrLanguage.ToString(); } }

        /// <summary>
        /// ��ǰ�����ֵ�
        /// </summary>
        public Dictionary<string, string> LocalizationDic = new Dictionary<string, string>();

        /// <summary>
        /// �����б�
        /// </summary>
        /// <param name="ms"></param>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                LocalizationDic[ms.ReadUTF8String()] = ms.ReadUTF8String();
            }
        }
    }
}