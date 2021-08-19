using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// Http����ص������� ���ڴ洢��Httpվ�����ص�����
    /// </summary>
    public class HttpCallBackArgs : EventArgs
    {
        /// <summary>
        /// �Ƿ��д�
        /// </summary>
        public bool HasError;

        /// <summary>
        /// Json����ֵ
        /// </summary>
        public string Value;

        /// <summary>
        /// bytes����ֵ
        /// </summary>
        public byte[] Data;
    }
}