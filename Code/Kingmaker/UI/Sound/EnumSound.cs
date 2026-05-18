using System;

namespace Kingmaker.UI.Sound;

[Serializable]
public class EnumSound<TEnum> where TEnum : struct, Enum
{
	public TEnum Enum;

	public UISound Sound;
}
