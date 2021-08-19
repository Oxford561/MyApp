using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AboutUs : ScriptableObject
{
	[BoxGroup("AboutUs")]
	[HorizontalGroup("AboutUs/Split", 80)]
	[VerticalGroup("AboutUs/Split/Left")]
	[HideLabel, PreviewField(80, ObjectFieldAlignment.Center)]
	public Texture Icon;

	[HorizontalGroup("AboutUs/Split", LabelWidth = 70)]

	[VerticalGroup("AboutUs/Split/Right")]
	[DisplayAsString]
	[LabelText("�������")]
	[GUIColor(2, 6, 6, 1)]
	public string Name = "YouYouFramework";

	[PropertySpace(10)]
	[VerticalGroup("AboutUs/Split/Right")]
	[DisplayAsString]
	[LabelText("�汾��")]
	public string Version = "1.0.0";

	[VerticalGroup("AboutUs/Split/Right")]
	[DisplayAsString]
	[LabelText("����")]
	public string Author = "����";

	[VerticalGroup("AboutUs/Split/Right")]
	[DisplayAsString]
	[LabelText("��ϵ��ʽ")]
	public string Contact = "http://www.u3dol.com";

}
