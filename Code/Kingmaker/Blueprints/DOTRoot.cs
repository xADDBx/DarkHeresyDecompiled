using System;
using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[ComponentName("Root/DOTRoot")]
[TypeId("e2d29d7bccfc4029a207c773a3e55f3e")]
public class DOTRoot : BlueprintScriptableObject
{
	public class DOTRootReference : BlueprintReference<DOTRoot>
	{
		public DOTRootReference()
		{
			guid = "43d4c8def0c849869b6fe325074eeaf7";
		}
	}

	[Serializable]
	public class ListItem
	{
		public DOT Type;

		public BlueprintBuffReference Value;
	}

	private static readonly DOTRootReference s_Instance = new DOTRootReference();

	[SerializeField]
	private List<ListItem> DOTBuffs = new List<ListItem>();

	private Dictionary<DOT, BlueprintBuffReference> DOTBuffsMap = new Dictionary<DOT, BlueprintBuffReference>();

	public static DOTRoot Instance => s_Instance;

	public bool TryGetDOTBuffOfType(DOT type, out BlueprintBuff buff)
	{
		if (DOTBuffsMap.Count == 0)
		{
			DOTBuffsMap = DOTBuffs.ToDictionary((ListItem x) => x.Type, (ListItem x) => x.Value);
		}
		if (!DOTBuffsMap.TryGetValue(type, out var value))
		{
			buff = null;
			return false;
		}
		buff = value;
		return true;
	}
}
