using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// ��Դʵ��(AssetBundle��Assetʵ��)
	/// </summary>
	public class ResourceEntity
	{
		/// <summary>
		/// ��Դ����
		/// </summary>
		public string ResourceName;

		/// <summary>
		/// �Ƿ�AssetBundle
		/// </summary>
		public bool IsAssetBundle;

		/// <summary>
		/// ����Ŀ��
		/// </summary>
		public object Target;

		/// <summary>
		/// �ϴ�ʹ��ʱ��
		/// </summary>
		public float LastUseTime { get; private set; }

		/// <summary>
		/// ���ü���
		/// </summary>
		public int ReferenceCount { get; private set; }

		/// <summary>
		/// ��������Դʵ������
		/// </summary>
		public LinkedList<ResourceEntity> DependsResourceList { private set; get; }


		public ResourceEntity()
		{
			DependsResourceList = new LinkedList<ResourceEntity>();
		}

		/// <summary>
		/// ����ȡ��
		/// </summary>
		public void Spawn(bool reference)
		{
			LastUseTime = Time.time;

			if (!IsAssetBundle)
			{
				if (reference) ReferenceCount++;
			}
			else
			{
				//�����������Դ�� ���ͷ�
				if (GameEntry.Pool.CheckAssetBundleIsLock(ResourceName))
				{
					ReferenceCount = 1;
				}
			}
		}

		/// <summary>
		/// ����س�
		/// </summary>
		public void Unspawn()
		{
			LastUseTime = Time.time;

			if (!IsAssetBundle)
			{
				ReferenceCount--;
				if (ReferenceCount < 0) ReferenceCount = 0;
			}
		}
		/// <summary>
		/// �����Ƿ�����ͷ�
		/// </summary>
		/// <returns></returns>
		public bool GetCanRelease()
		{
			if (ReferenceCount == 0 && Time.time - LastUseTime > (IsAssetBundle ? GameEntry.Pool.ReleaseAssetBundleInterval : GameEntry.Pool.ReleaseAssetInterval))
			{
				return true;
			}
			return false;
		}
		/// <summary>
		/// �ͷ���Դ
		/// </summary>
		public void Release()
		{
			if (IsAssetBundle)
			{
				AssetBundle bundle = Target as AssetBundle;
				bundle.Unload(false);
			}

			ResourceName = null;
			ReferenceCount = 0;
			Target = null;

			DependsResourceList.Clear(); //���Լ���������Դʵ�����
			GameEntry.Pool.EnqueueClassObject(this); //�������Դʵ��س�
		}
	}
}