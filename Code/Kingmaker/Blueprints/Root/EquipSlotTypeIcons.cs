using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class EquipSlotTypeIcons
{
	[Serializable]
	public class TypeIconsPair
	{
		public EquipSlotType Type;

		public EquipSlotSubtype Subtype;

		public Sprite Icon;
	}

	public List<TypeIconsPair> Icons;

	public Sprite GetTypeIcon(EquipSlotType type, EquipSlotSubtype subtype = EquipSlotSubtype.None)
	{
		return Icons.FirstOrDefault((TypeIconsPair i) => i.Type == type && i.Subtype == subtype)?.Icon;
	}
}
