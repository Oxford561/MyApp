//===================================================
//��    �ߣ�����  http://www.u3dol.com
//����ʱ�䣺
//��    ע��
//===================================================
using PathologicalGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����ʵ��
/// </summary>
[System.Serializable]
public class GameObjectPoolEntity
{
	/// <summary>
	/// ����ر��
	/// </summary>
	public byte PoolId;

	/// <summary>
	/// ���������
	/// </summary>
	public string PoolName;

	/// <summary>
	/// ��Ӧ����Ϸ��������
	/// </summary>
	[HideInInspector]
	public SpawnPool Pool;
}