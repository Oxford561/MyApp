using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

[CreateAssetMenu]
public class ParamsSettings : ScriptableObject
{
	[BoxGroup("InitUrl")] public string WebAccountUrl;
	[BoxGroup("InitUrl")] public string TestWebAccountUrl;
	[BoxGroup("InitUrl")] public bool IsTest;
	[BoxGroup("InitUrl")] public bool PostIsEncrypt;//�Ƿ����(��ʱ���)
	[BoxGroup("InitUrl")] public string PostContentType;//����ContentType



	[BoxGroup("GeneralParams")]
	[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
	[HideLabel]
	public GeneralParamData[] GeneralParams;

	[BoxGroup("GradeParams")]
	[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
	[HideLabel]
	public GradeParamData[] GradeParams;

	/// <summary>
	/// ��������
	/// </summary>
	[Serializable]
	public class GeneralParamData
	{
		[TableColumnWidth(260, Resizable = false)]
		/// <summary>
		/// ����Key
		/// </summary>
		public string Key;

		/// <summary>
		/// ����ֵ
		/// </summary>
		public int Value;
	}

	/// <summary>
	/// �豸�ȼ�
	/// </summary>
	public enum DeviceGrade
	{
		Low = 0,
		Middle = 1,
		High = 2
	}

	/// <summary>
	/// �ȼ���������
	/// </summary>
	[Serializable]
	public class GradeParamData
	{
		[TableColumnWidth(260, Resizable = false)]
		/// <summary>
		/// ����Key
		/// </summary>
		public string Key;

		/// <summary>
		/// ����ֵ
		/// </summary>
		public int LowValue;

		/// <summary>
		/// ����ֵ
		/// </summary>
		public int MiddleValue;

		/// <summary>
		/// ����ֵ
		/// </summary>
		public int HighValue;

		/// <summary>
		/// ��ȡ����ֵ
		/// </summary>
		/// <param name="grade"></param>
		/// <returns></returns>
		public int GetValueByGrade(DeviceGrade grade)
		{
			switch (grade)
			{
				default:
				case DeviceGrade.Low:
					return LowValue;
				case DeviceGrade.Middle:
					return MiddleValue;
				case DeviceGrade.High:
					return HighValue;
			}
		}
	}

	private int m_LenGradeParams = 0;

	/// <summary>
	/// ����key���豸�ȼ���ȡ����
	/// </summary>
	/// <param name="key"></param>
	/// <param name="grade"></param>
	/// <returns></returns>
	public int GetGradeParamData(string key, DeviceGrade grade)
	{
		m_LenGradeParams = GradeParams.Length;
		for (int i = 0; i < m_LenGradeParams; i++)
		{
			GradeParamData gradeParamData = GradeParams[i];
			if (gradeParamData.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))
			{
				return gradeParamData.GetValueByGrade(grade);
			}
		}

		GameEntry.Logger.LogError(string.Format("GetGradeParamData Fail key={0}", key));
		return 0;
	}
}