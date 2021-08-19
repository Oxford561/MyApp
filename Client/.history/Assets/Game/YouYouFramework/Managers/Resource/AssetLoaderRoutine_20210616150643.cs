using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// ��Դ������
	/// </summary>
	public class AssetLoaderRoutine
	{
		/// <summary>
		/// ��Դ��������
		/// </summary>
		private AssetBundleRequest m_CurrAssetBundleRequest;

		private string m_CurrAssetName;

		/// <summary>
		/// ��Դ�������
		/// </summary>
		public Action<float> OnAssetUpdate;

		/// <summary>
		/// ������Դ���
		/// </summary>
		public Action<UnityEngine.Object> OnLoadAssetComplete;


		/// <summary>
		/// �첽������Դ
		/// </summary>
		/// <param name="assetName"></param>
		/// <param name="assetBundle"></param>
		internal void LoadAsset(string assetName, AssetBundle assetBundle)
		{
			if (assetName.LastIndexOf(".unity") != -1)
			{
				if (OnLoadAssetComplete != null) OnLoadAssetComplete(null);
				return;
			}
			m_CurrAssetName = assetName;
			m_CurrAssetBundleRequest = assetBundle.LoadAssetAsync(assetName);
		}

		/// <summary>
		/// ����
		/// </summary>
		public void Reset()
		{
			m_CurrAssetBundleRequest = null;
		}

		/// <summary>
		/// ����
		/// </summary>
		internal void OnUpdate()
		{
			UpdateAssetBundleRequest();
		}

		/// <summary>
		/// ���� ��Դ���� ����
		/// </summary>
		private void UpdateAssetBundleRequest()
		{
			if (m_CurrAssetBundleRequest != null)
			{
				if (m_CurrAssetBundleRequest.isDone)
				{
					UnityEngine.Object obj = m_CurrAssetBundleRequest.asset;
					if (obj != null)
					{
						//GameEntry.Log(LogCategory.Resource, "��Դ=>{0} �������", m_CurrAssetName);
						Reset();//һ��Ҫ���Reset

						if (OnLoadAssetComplete != null) OnLoadAssetComplete(obj);
					}
					else
					{
						GameEntry.LogError("��Դ=>{0} ����ʧ��", m_CurrAssetName);
						Reset();//һ��Ҫ���Reset

						if (OnLoadAssetComplete != null) OnLoadAssetComplete(null);
					}
				}
				else
				{
					//���ؽ���
					if (OnAssetUpdate != null) OnAssetUpdate(m_CurrAssetBundleRequest.progress);
				}
			}
		}
	}
}