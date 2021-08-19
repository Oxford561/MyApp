using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu]
public class MacroSettings : ScriptableObject
{
	[Serializable]
	public class MacroData
	{
		[TableColumnWidth(80, Resizable = false)]
		public bool Enabled;

		/// <summary>
		/// �������õ�Key(���Ľ���)
		/// </summary>
		public string Name;

		/// <summary>
		/// �������õ�ֵ
		/// </summary>
		public string Macro;
	}
	/// <summary>
	/// ��Դ���ط�ʽ
	/// </summary>
	public enum AssetLoadTarget
	{
		RESOURCES,
		ASSETBUNDLE,
		EDITORLOAD
	}
	private string m_Macor;

	[LabelText("��Դ���ط�ʽ")]
	public AssetLoadTarget CurrAssetLoadTarget;

	[PropertySpace(10)]
	[BoxGroup("MacroSettings")]
	[TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
	[HideLabel]
	public MacroData[] Settings;


	[Button(ButtonSizes.Medium), ResponsiveButtonGroup("DefaultButtonSize"), PropertyOrder(1)]
	public void SaveMacro()
	{
#if UNITY_EDITOR
		m_Macor = string.Empty;
		foreach (var item in Settings)
		{
			if (item.Enabled)
			{
				m_Macor += string.Format("{0};", item.Macro);
			}
		}
		m_Macor += string.Format("{0};", CurrAssetLoadTarget.ToString());

		//����BuildSetting�еĳ������úͽ���
		EditorBuildSettingsScene[] arrScene = EditorBuildSettings.scenes;
		for (int i = 0; i < arrScene.Length; i++)
		{
			if (arrScene[i].path.IndexOf("download", StringComparison.CurrentCultureIgnoreCase) > -1)
			{
				arrScene[i].enabled = !CurrAssetLoadTarget.ToString().Equals("ASSETBUNDLE");
			}
		}
		EditorBuildSettings.scenes = arrScene;

		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, m_Macor);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, m_Macor);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, m_Macor);
		Debug.Log("Sava Macro Success");
#endif
	}

	void OnEnable()
	{
#if UNITY_EDITOR
		//��ʼ��m_Macor
		m_Macor = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);

		if (!string.IsNullOrEmpty(m_Macor))
		{
			//���ַ�������AssetLoadTargets[i]
			AssetLoadTarget[] AssetLoadTargets = (AssetLoadTarget[])Enum.GetValues(typeof(AssetLoadTarget));
			for (int i = 0; i < AssetLoadTargets.Length; i++)
			{
				if (m_Macor.IndexOf(AssetLoadTargets[i].ToString()) != -1)
				{
					CurrAssetLoadTarget = AssetLoadTargets[i];
				}
			}

			//���ַ�������Settings[i].Macro
			for (int i = 0; i < Settings.Length; i++)
			{
				if (m_Macor.IndexOf(Settings[i].Macro) != -1)
				{
					Settings[i].Enabled = true;
				}
				else
				{
					Settings[i].Enabled = false;
				}
			}
		}
#endif
	}

}

