using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// ��д����Դ������
	/// </summary>
	public class LocalAssetsManager
	{
		/// <summary>
		/// ���ذ汾�ļ�·��
		/// </summary>
		public string LocalVersionFilePath
		{
			get
			{
				return string.Format("{0}/{1}", Application.persistentDataPath, YFConstDefine.VersionFileName);
			}
		}

		#region ��ȡ��д���汾�ļ��Ƿ���� GetVersionFileExists
		/// <summary>
		/// ��ȡ��д���汾�ļ��Ƿ����
		/// </summary>
		/// <returns></returns>
		public bool GetVersionFileExists()
		{
			return File.Exists(LocalVersionFilePath);
		}
		#endregion

		#region GetFileBuffer ��ȡ�����ļ����ֽ�����
		/// <summary>
		/// ��ȡ�����ļ����ֽ�����
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public byte[] GetFileBuffer(string path)
		{
			return IOUtil.GetFileBuffer(string.Format("{0}/{1}", Application.persistentDataPath, path));
		}

		internal bool CheckFileExists(string assetBundlePath)
		{
			return File.Exists(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
		}
		#endregion

		#region SetResourceVersion ������Դ�汾��
		/// <summary>
		/// ������Դ�汾��
		/// </summary>
		/// <param name="version"></param>
		public void SetResourceVersion(string version)
		{
			PlayerPrefs.SetString(YFConstDefine.ResourceVersion, version);
		}
		#endregion

		#region SaveVersionFile ����汾�ļ�
		/// <summary>
		/// ����汾�ļ�
		/// </summary>
		/// <param name="version"></param>
		public void SaveVersionFile(Dictionary<string, AssetBundleInfoEntity> dic)
		{
			string json = dic.ToJson();
			IOUtil.CreateTextFile(LocalVersionFilePath, json);
		}
		#endregion

		#region GetAssetBundleVersionList ���ؿ�д����Դ����Ϣ
		/// <summary>
		/// ���ؿ�д����Դ����Ϣ
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public Dictionary<string, AssetBundleInfoEntity> GetAssetBundleVersionList(ref string version)
		{
			version = PlayerPrefs.GetString(YFConstDefine.ResourceVersion);
			string json = IOUtil.GetFileText(LocalVersionFilePath);
			return json.ToObject<Dictionary<string, AssetBundleInfoEntity>>();
		}
		#endregion
	}
}
