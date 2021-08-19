using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using YouYou;

[CreateAssetMenu]
public class AssetBundleSettings : ScriptableObject
{
	public enum CusBuildTarget
	{
		Windows,
		Android,
		IOS
	}

	[HorizontalGroup("Common", LabelWidth = 70)]
	[VerticalGroup("Common/Left")]
	[LabelText("��Դ�汾��")]
	public string ResourceVersion = "1.0.1";

	[PropertySpace(10)]
	[VerticalGroup("Common/Left")]
	[LabelText("Ŀ��ƽ̨")]
	public CusBuildTarget CurrBuildTarget;

	public BuildTarget GetBuildTarget()
	{
		switch (CurrBuildTarget)
		{
			default:
			case CusBuildTarget.Windows:
				return BuildTarget.StandaloneWindows;
			case CusBuildTarget.Android:
				return BuildTarget.Android;
			case CusBuildTarget.IOS:
				return BuildTarget.iOS;
		}
	}

	[PropertySpace(10)]
	[VerticalGroup("Common/Left")]
	[LabelText("����")]
	public BuildAssetBundleOptions Options;

	[VerticalGroup("Common/Right")]
	[Button(ButtonSizes.Medium)]
	[LabelText("���°汾��")]
	public void UpdateResourceVersion()
	{
		string version = ResourceVersion;
		string[] arr = version.Split('.');

		int shortVersion = 0;
		int.TryParse(arr[2], out shortVersion);
		version = string.Format("{0}.{1}.{2}", arr[0], arr[1], ++shortVersion);
		ResourceVersion = version;
	}


	[VerticalGroup("Common/Right")]
	[Button(ButtonSizes.Medium)]
	[LabelText("�����Դ��")]
	public void ClearAssetBundle()
	{
		if (Directory.Exists(TempPath))
		{
			Directory.Delete(TempPath, true);
		}
		EditorUtility.DisplayDialog("", "������", "ȷ��");
	}

	/// <summary>
	/// Ҫ�ռ�����Դ��
	/// </summary>
	List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

	[VerticalGroup("Common/Right")]
	[Button(ButtonSizes.Medium)]
	[LabelText("���")]
	public void BuildAssetBundle()
	{
		builds.Clear();
		int len = Datas.Length;
		for (int i = 0; i < len; i++)
		{
			AssetBundleData assetBundleData = Datas[i];
			if (assetBundleData.IsCanEditor)
			{
				int lenPath = assetBundleData.Path.Length;
				for (int j = 0; j < lenPath; j++)
				{
					//������·��
					string path = assetBundleData.Path[j];
					BuildAssetBundleForPath(path, assetBundleData.Overall);
				}
			}
		}

		if (!Directory.Exists(TempPath)) Directory.CreateDirectory(TempPath);

		if (builds.Count == 0) return;
		Debug.Log("builds count=" + builds.Count);

		BuildPipeline.BuildAssetBundles(TempPath, builds.ToArray(), Options, GetBuildTarget());
		Debug.Log("��ʱ��Դ��������");

		CopyFile(TempPath);
		Debug.Log("���������Ŀ¼���");

		AssetBundleEncrypt();
		Debug.Log("��Դ���������");

		CreateDependenciesFile();
		Debug.Log("AssetInfo==����������ϵ�ļ����");

		CreateVersionFile();
		Debug.Log("VersionFile==���ɰ汾�ļ����");
	}

	#region TempPath OutPath
	/// <summary>
	/// ��ʱĿ¼
	/// </summary>
	public string TempPath
	{
		get
		{
			return Application.dataPath + "/../" + AssetBundleSavePath + "/" + ResourceVersion + "_Temp/" + CurrBuildTarget;
		}
	}

	/// <summary>
	/// ���Ŀ¼
	/// </summary>
	public string OutPath
	{
		get
		{
			return TempPath.Replace("_Temp", "");
		}
	}
	#endregion

	#region CopyFile �����ļ�����ʽĿ¼
	/// <summary>
	/// �����ļ�����ʽĿ¼
	/// </summary>
	/// <param name="oldPath"></param>
	private void CopyFile(string oldPath)
	{
		if (Directory.Exists(OutPath))
		{
			Directory.Delete(OutPath, true);
		}

		IOUtil.CopyDirectory(oldPath, OutPath);
		DirectoryInfo directory = new DirectoryInfo(OutPath);

		//�õ��ļ����������ļ�
		FileInfo[] arrFiles = directory.GetFiles("*.y", SearchOption.AllDirectories);
		int len = arrFiles.Length;
		for (int i = 0; i < len; i++)
		{
			FileInfo file = arrFiles[i];
			File.Move(file.FullName, file.FullName.Replace(".ab.y", ".assetbundle"));
		}
	}
	#endregion

	#region AssetBundleEncrypt ��Դ������
	/// <summary>
	/// ��Դ������
	/// </summary>
	/// <param name="path"></param>
	private void AssetBundleEncrypt()
	{
		int len = Datas.Length;
		for (int i = 0; i < len; i++)
		{
			AssetBundleData assetBundleData = Datas[i];
			if (assetBundleData.IsEncrypt && assetBundleData.IsCanEditor)
			{
				//�����Ҫ����
				for (int j = 0; j < assetBundleData.Path.Length; j++)
				{
					string path = OutPath + "/" + assetBundleData.Path[j];

					if (assetBundleData.Overall)
					{
						//���Ǳ����ļ��д�� ˵�����·������һ����
						path = path + ".assetbundle";

						AssetBundleEncryptFile(path);
					}
					else
					{
						AssetBundleEncryptFolder(path);
					}
				}
			}
		}
	}

	/// <summary>
	/// �����ļ����������ļ�
	/// </summary>
	/// <param name="folderPath"></param>
	private void AssetBundleEncryptFolder(string folderPath, bool isDelete = false)
	{
		DirectoryInfo directory = new DirectoryInfo(folderPath);

		//�õ��ļ����������ļ�
		FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

		foreach (FileInfo file in arrFiles)
		{
			AssetBundleEncryptFile(file.FullName, isDelete);
		}
	}

	/// <summary>
	/// �����ļ�
	/// </summary>
	/// <param name="filePath"></param>
	private void AssetBundleEncryptFile(string filePath, bool isDelete = false)
	{
		FileInfo fileInfo = new FileInfo(filePath);
		byte[] buffer = null;

		using (FileStream fs = new FileStream(filePath, FileMode.Open))
		{
			buffer = new byte[fs.Length];
			fs.Read(buffer, 0, buffer.Length);
		}

		buffer = SecurityUtil.Xor(buffer);

		using (FileStream fs = new FileStream(filePath, FileMode.Create))
		{
			fs.Write(buffer, 0, buffer.Length);
			fs.Flush();
		}
	}
	#endregion

	#region OnCreateDependenciesFile ����������ϵ�ļ�
	/// <summary>
	/// ����������ϵ�ļ�
	/// </summary>
	private void CreateDependenciesFile()
	{
		//��һ��ѭ�� �����е�Asset�洢��һ���б���

		//��ʱ�б�
		List<AssetEntity> tempLst = new List<AssetEntity>();

		int len = Datas.Length;
		//ѭ�������ļ��а������ļ���ߵ���
		for (int i = 0; i < len; i++)
		{
			AssetBundleData assetBundleData = Datas[i];
			for (int j = 0; j < assetBundleData.Path.Length; j++)
			{
				string path = Application.dataPath + "/" + assetBundleData.Path[j];
				//Debug.LogError("CreateDependenciesFile path=" + path);
				CollectFileInfo(tempLst, path);
			}
		}

		//
		len = tempLst.Count;

		//��Դ�б�
		List<AssetEntity> assetList = new List<AssetEntity>();

		for (int i = 0; i < len; i++)
		{
			AssetEntity entity = tempLst[i];

			AssetEntity newEntity = new AssetEntity();
			newEntity.AssetFullName = entity.AssetFullName;
			newEntity.AssetBundleName = entity.AssetBundleName;

			assetList.Add(newEntity);

			//��������Ҫ���������
			//if (entity.Category == AssetCategory.Scenes) continue;

			newEntity.DependsAssetList = new List<AssetDependsEntity>();

			string[] arr = AssetDatabase.GetDependencies(entity.AssetFullName);
			foreach (string str in arr)
			{
				if (!str.Equals(newEntity.AssetFullName, StringComparison.CurrentCultureIgnoreCase) && GetIsAsset(tempLst, str))
				{
					AssetDependsEntity assetDepends = new AssetDependsEntity();
					assetDepends.AssetFullName = str;

					//��������Դ ���뵽������Դ�б�
					newEntity.DependsAssetList.Add(assetDepends);
				}
			}
		}

		//����һ��Json�ļ�
		string targetPath = OutPath;
		if (!Directory.Exists(targetPath))
		{
			Directory.CreateDirectory(targetPath);
		}

		string strJsonFilePath = targetPath + "/AssetInfo.json"; //�汾�ļ�·��
		IOUtil.CreateTextFile(strJsonFilePath, assetList.ToJson());
		Debug.Log("���� AssetInfo.json ���");

		MMO_MemoryStream ms = new MMO_MemoryStream();
		//���ɶ������ļ�
		len = assetList.Count;
		ms.WriteInt(len);

		for (int i = 0; i < len; i++)
		{
			AssetEntity entity = assetList[i];
			ms.WriteUTF8String(entity.AssetFullName);
			ms.WriteUTF8String(entity.AssetBundleName);

			if (entity.DependsAssetList != null)
			{
				//���������Դ
				int depLen = entity.DependsAssetList.Count;
				ms.WriteInt(depLen);
				for (int j = 0; j < depLen; j++)
				{
					AssetDependsEntity assetDepends = entity.DependsAssetList[j];
					ms.WriteUTF8String(assetDepends.AssetFullName);
				}
			}
			else
			{
				ms.WriteInt(0);
			}
		}

		string filePath = targetPath + "/AssetInfo.bytes"; //�汾�ļ�·��
		byte[] buffer = ms.ToArray();
		buffer = ZlibHelper.CompressBytes(buffer);
		FileStream fs = new FileStream(filePath, FileMode.Create);
		fs.Write(buffer, 0, buffer.Length);
		fs.Close();
		fs.Dispose();
	}

	/// <summary>
	/// �ж�ĳ����Դ�Ƿ��������Դ�б�
	/// </summary>
	/// <param name="tempLst"></param>
	/// <param name="assetFullName"></param>
	/// <returns></returns>
	private bool GetIsAsset(List<AssetEntity> tempLst, string assetFullName)
	{
		int len = tempLst.Count;
		for (int i = 0; i < len; i++)
		{
			AssetEntity entity = tempLst[i];
			if (entity.AssetFullName.Equals(assetFullName, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
	#endregion



	#region CollectFileInfo �ռ��ļ���Ϣ
	/// <summary>
	/// �ռ��ļ���Ϣ
	/// </summary>
	/// <param name="tempLst"></param>
	/// <param name="folderPath"></param>
	private void CollectFileInfo(List<AssetEntity> tempLst, string folderPath)
	{
		DirectoryInfo directory = new DirectoryInfo(folderPath);

		//�õ��ļ����������ļ�
		FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

		for (int i = 0; i < arrFiles.Length; i++)
		{
			FileInfo file = arrFiles[i];
			if (file.Extension == ".meta") continue;
			if (file.FullName.IndexOf(".idea") != -1) continue;

			//����·��
			string filePath = file.FullName;
			//Debug.LogError("filePath==" + filePath);

			AssetEntity entity = new AssetEntity();
			//���·��
			entity.AssetFullName = filePath.Substring(filePath.IndexOf("Assets\\")).Replace("\\", "/");
			//Debug.LogError("AssetFullName==" + entity.AssetFullName);

			entity.AssetBundleName = (GetAssetBundleName(entity.AssetFullName) + ".assetbundle").ToLower();
			tempLst.Add(entity);
		}
	}
	#endregion

	#region CreateVersionFile ���ɰ汾�ļ�
	/// <summary>
	/// ���ɰ汾�ļ�
	/// </summary>
	private void CreateVersionFile()
	{
		string path = OutPath;
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		string strVersionFilePath = path + "/VersionFile.txt"; //�汾�ļ�·��

		//����汾�ļ����� ��ɾ��
		IOUtil.DeleteFile(strVersionFilePath);

		StringBuilder sbContent = new StringBuilder();

		DirectoryInfo directory = new DirectoryInfo(path);

		//�õ��ļ����������ļ�
		FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

		sbContent.AppendLine(this.ResourceVersion);
		for (int i = 0; i < arrFiles.Length; i++)
		{
			FileInfo file = arrFiles[i];

			if (file.Extension == ".manifest")
			{
				continue;
			}
			string fullName = file.FullName; //ȫ�� ����·����չ��

			//���·��
			string name = fullName.Substring(fullName.IndexOf(CurrBuildTarget.ToString()) + CurrBuildTarget.ToString().Length + 1);

			string md5 = EncryptUtil.GetFileMD5(fullName); //�ļ���MD5
			if (md5 == null) continue;

			string size = file.Length.ToString(); //�ļ���С

			bool isFirstData = false; //�Ƿ��ʼ����
			bool isEncrypt = false;
			bool isBreak = false;

			for (int j = 0; j < Datas.Length; j++)
			{
				foreach (string mPath in Datas[j].Path)
				{
					string tempPath = mPath;

					name = name.Replace("\\", "/");
					if (name.IndexOf(tempPath, StringComparison.CurrentCultureIgnoreCase) != -1)
					{
						isFirstData = Datas[j].IsFirstData;
						isEncrypt = Datas[j].IsEncrypt;
						isBreak = true;
						break;
					}
				}
				if (isBreak) break;
			}

			string strLine = string.Format("{0}|{1}|{2}|{3}|{4}", name, md5, size, isFirstData ? 1 : 0, isEncrypt ? 1 : 0);
			sbContent.AppendLine(strLine);
		}

		IOUtil.CreateTextFile(strVersionFilePath, sbContent.ToString());

		MMO_MemoryStream ms = new MMO_MemoryStream();
		string str = sbContent.ToString().Trim();
		string[] arr = str.Split('\n');
		int len = arr.Length;
		ms.WriteInt(len);
		for (int i = 0; i < len; i++)
		{
			if (i == 0)
			{
				ms.WriteUTF8String(arr[i]);
			}
			else
			{
				string[] arrInner = arr[i].Split('|');
				ms.WriteUTF8String(arrInner[0]);
				ms.WriteUTF8String(arrInner[1]);
				ms.WriteULong(ulong.Parse(arrInner[2]));
				ms.WriteByte(byte.Parse(arrInner[3]));
				ms.WriteByte(byte.Parse(arrInner[4]));
			}
		}

		string filePath = path + "/VersionFile.bytes"; //�汾�ļ�·��
		byte[] buffer = ms.ToArray();
		buffer = ZlibHelper.CompressBytes(buffer);
		FileStream fs = new FileStream(filePath, FileMode.Create);
		fs.Write(buffer, 0, buffer.Length);
		fs.Close();
		fs.Dispose();
	}

	#endregion

	#region GetAssetBundleName ��ȡ��Դ��������
	/// <summary>
	/// ��ȡ��Դ��������
	/// </summary>
	/// <param name="assetFullName"></param>
	/// <returns></returns>
	private string GetAssetBundleName(string assetFullName)
	{
		int len = Datas.Length;
		//ѭ�������ļ��а������ļ���ߵ���
		for (int i = 0; i < len; i++)
		{
			AssetBundleData assetBundleData = Datas[i];
			for (int j = 0; j < assetBundleData.Path.Length; j++)
			{
				if (assetFullName.IndexOf(assetBundleData.Path[j], StringComparison.CurrentCultureIgnoreCase) > -1)
				{
					if (assetBundleData.Overall)
					{
						//�ļ����Ǹ����� �򷵻�������ļ�������
						return assetBundleData.Path[j].ToLower();
					}
					else
					{
						//��ɢ��Դ
						return assetFullName.Substring(0, assetFullName.LastIndexOf('.')).ToLower().Replace("assets/", "");
					}
				}
			}
		}
		return null;
	}
	#endregion


	/// <summary>
	/// ����·�������Դ
	/// </summary>
	/// <param name="path"></param>
	/// <param name="overall">���һ����Դ��</param>
	private void BuildAssetBundleForPath(string path, bool overall)
	{
		string fullPath = Application.dataPath + "/" + path;
		//Debug.LogError("fullPath=" + fullPath);
		//1.�õ��ļ����������ļ�
		DirectoryInfo directory = new DirectoryInfo(fullPath);

		//�õ��ļ����������ļ�
		FileInfo[] arrFiles = directory.GetFiles("*", SearchOption.AllDirectories);

		//Debug.LogError("arrFile=" + arrFile.Length);
		if (overall)
		{
			//���һ����Դ��
			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = path + ".ab";
			build.assetBundleVariant = "y";
			string[] arr = GetValidateFiles(arrFiles);
			build.assetNames = arr;
			builds.Add(build);
		}
		else
		{
			//ÿ���ļ����һ����
			string[] arr = GetValidateFiles(arrFiles);
			for (int i = 0; i < arr.Length; i++)
			{
				AssetBundleBuild build = new AssetBundleBuild();
				build.assetBundleName = arr[i].Substring(0, arr[i].LastIndexOf('.')).Replace("Assets/", "") + ".ab";
				build.assetBundleVariant = "y";
				build.assetNames = new string[] { arr[i] };

				//Debug.LogError("assetBundleName==" + build.assetBundleName);
				builds.Add(build);
			}
		}
	}

	private string[] GetValidateFiles(FileInfo[] arrFiles)
	{
		List<string> lst = new List<string>();

		int len = arrFiles.Length;
		for (int i = 0; i < len; i++)
		{
			FileInfo file = arrFiles[i];
			if (!file.Extension.Equals(".meta", StringComparison.CurrentCultureIgnoreCase))
			{
				lst.Add("Assets" + file.FullName.Replace("\\", "/").Replace(Application.dataPath, ""));
			}
		}

		return lst.ToArray();
	}


	[LabelText("��Դ������·��")]
	[FolderPath]
	/// <summary>
	/// ��Դ������·��
	/// </summary>
	public string AssetBundleSavePath;

	[LabelText("��ѡ���б༭")]
	public bool IsCanEditor;

	[EnableIf("IsCanEditor")]
	[BoxGroup("AssetBundleSettings")]
	public AssetBundleData[] Datas;

	//������Ͽ����л����
	[Serializable]
	public class AssetBundleData
	{
		[LabelText("����")]
		/// <summary>
		/// ����
		/// </summary>
		public string Name;

		[LabelText("�Ƿ�Ҫ���")]
		public bool IsCanEditor = true;

		[LabelText("�ļ���Ϊһ����Դ��")]
		/// <summary>
		/// ���һ����Դ��
		/// </summary>
		public bool Overall;

		[LabelText("�Ƿ��ʼ��Դ")]
		/// <summary>
		/// �Ƿ��ʼ��Դ
		/// </summary>
		public bool IsFirstData;

		[LabelText("�Ƿ����")]
		/// <summary>
		/// �Ƿ����
		/// </summary>
		public bool IsEncrypt;

		[FolderPath(ParentFolder = "Assets")]
		/// <summary>
		/// ·��
		/// </summary>
		public string[] Path;
	}
}