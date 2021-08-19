using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// ��Դ��������
	/// </summary>
	public class AssetBundleLoaderRoutine
	{
		/// <summary>
		/// ��ǰ����Դ����Ϣ
		/// </summary>
		private AssetBundleInfoEntity m_CurrAssetBundleInfo;

		/// <summary>
		/// ��Դ����������
		/// </summary>
		private AssetBundleCreateRequest m_CurrAssetBundleCreateRequest;

		/// <summary>
		/// ��Դ�������������
		/// </summary>
		public Action<float> OnAssetBundleCreateUpdate;

		/// <summary>
		/// ������Դ�����
		/// </summary>
		public Action<AssetBundle> OnLoadAssetBundleComplete;

		#region LoadAssetBundle ������Դ��
		/// <summary>
		/// ������Դ��
		/// </summary>
		public void LoadAssetBundle(string assetBundlePath)
		{
			m_CurrAssetBundleInfo = GameEntry.Resource.ResourceManager.GetAssetBundleInfo(assetBundlePath);

			//����ļ��ڿ�д���Ƿ����
			bool isExistsInLocal = GameEntry.Resource.ResourceManager.LocalAssetsManager.CheckFileExists(assetBundlePath);

			if (isExistsInLocal && !m_CurrAssetBundleInfo.IsEncrypt)
			{
				//�����Դ�������ڿ�д��  ����û�м���
				m_CurrAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
			}
			else
			{
				byte[] buffer = GameEntry.Resource.ResourceManager.LocalAssetsManager.GetFileBuffer(assetBundlePath);
				if (buffer != null)
				{
					//�ӿ�д��������Դ��
					LoadAssetBundleAsync(buffer);
					return;
				}

				//�����д��û�� ��ô�ʹ�ֻ������ȡ
				GameEntry.Resource.ResourceManager.StreamingAssetsManager.ReadAssetBundle(assetBundlePath, (byte[] buff) =>
				{
					if (buff != null)
					{
						//��ֻ����������Դ��
						LoadAssetBundleAsync(buff);
						return;
					}

					//���ֻ����Ҳû��,��CDN����
					GameEntry.Download.BeginDownloadSingle(assetBundlePath, (url, currSize, progress) =>
					{
						//Debug.LogError(progress);
						OnAssetBundleCreateUpdate?.Invoke(progress);
					}, (string fileUrl) =>
					{
						buffer = GameEntry.Resource.ResourceManager.LocalAssetsManager.GetFileBuffer(fileUrl);
						LoadAssetBundleAsync(buffer);
					});
				});
			}
		}

		/// <summary>
		/// �첽������Դ��
		/// </summary>
		/// <param name="buffer"></param>
		private void LoadAssetBundleAsync(byte[] buffer)
		{
			if (m_CurrAssetBundleInfo.IsEncrypt)
			{
				//�����Դ���Ǽ��ܵ�,�����
				buffer = SecurityUtil.Xor(buffer);
			}

			m_CurrAssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(buffer);
		}
		#endregion

		/// <summary>
		/// ����
		/// </summary>
		public void Reset()
		{
			m_CurrAssetBundleCreateRequest = null;
		}

		/// <summary>
		/// ����
		/// </summary>
		internal void OnUpdate()
		{
			UpdateAssetBundleCreateRequest();
		}

		#region UpdateAssetBundleCreateRequest ������Դ������
		/// <summary>
		/// ������Դ������
		/// </summary>
		private void UpdateAssetBundleCreateRequest()
		{
			if (m_CurrAssetBundleCreateRequest == null) return;
			if (m_CurrAssetBundleCreateRequest.isDone)
			{
				AssetBundle assetBundle = m_CurrAssetBundleCreateRequest.assetBundle;
				if (assetBundle != null)
				{
					//GameEntry.Log(LogCategory.Resource, "��Դ��=>{0} �������", m_CurrAssetBundleInfo.AssetBundleName);
				}
				else
				{
					GameEntry.LogError("��Դ��=>{0} ����ʧ��", m_CurrAssetBundleInfo.AssetBundleName);
				}
				OnLoadAssetBundleComplete?.Invoke(assetBundle);
				Reset();//һ��Ҫ���Reset
			}
			else
			{
				//���ؽ���
				//OnAssetBundleCreateUpdate?.Invoke(m_CurrAssetBundleCreateRequest.progress);
			}
		}
		#endregion
	}
}