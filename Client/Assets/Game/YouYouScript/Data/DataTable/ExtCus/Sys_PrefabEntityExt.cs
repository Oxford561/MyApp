using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public partial class Sys_PrefabEntity
	{
		public string AssetFullName
		{
			get { return string.Format("{0}.{1}", AssetPath, Suffixes); }
		}
	}
}