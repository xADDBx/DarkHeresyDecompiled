using System;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Enums;

[Serializable]
public class EnumIconEntry<TEnum> where TEnum : Enum
{
	public TEnum Type { get; private set; }

	public Sprite Icon { get; private set; }

	public EnumIconEntry(TEnum t, Sprite s)
	{
		Type = t;
		Icon = s;
	}
}
