using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
    /// <summary>
    /// AssetBundle�汾�ļ���Ϣʵ��
    /// </summary>
    public class AssetBundleInfoEntity
    {
        /// <summary>
        /// ��Դ������
        /// </summary>
        public string AssetBundleName;

        /// <summary>
        /// MD5��
        /// </summary>
        public string MD5;

        /// <summary>
        /// �ļ���С(�ֽ�)
        /// </summary>
        public ulong Size;

        /// <summary>
        /// �Ƿ��ʼ����
        /// </summary>
        public bool IsFirstData;

        /// <summary>
        /// �Ƿ��Ѿ�����
        /// </summary>
        public bool IsEncrypt;

    }
}