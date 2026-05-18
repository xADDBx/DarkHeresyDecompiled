using System;
using Kingmaker.Blueprints.Root;
using UnityEngine;

namespace Kingmaker.Code.View.UI.UIUtilities;

public static class UIUtilityImage
{
	public static Sprite GetDefaultIfNull(this Sprite sprite, DefaultImageType type)
	{
		if (sprite != null)
		{
			return sprite;
		}
		return GetDefault(type);
	}

	public static Sprite GetDefault(DefaultImageType type)
	{
		return type switch
		{
			DefaultImageType.Item => UIConfig.Instance.UIIcons.DefaultIcons.DefaultItemIcon, 
			DefaultImageType.Ability => UIConfig.Instance.UIIcons.DefaultIcons.DefaultAbilityIcon, 
			DefaultImageType.Modifier => UIConfig.Instance.UIIcons.DefaultIcons.DefaultModifierIcon, 
			DefaultImageType.Appearance => UIConfig.Instance.UIIcons.DefaultIcons.DefaultAppearanceIcon, 
			DefaultImageType.AppearanceEmpty => UIConfig.Instance.UIIcons.DefaultIcons.DefaultAppearanceIconEmpty, 
			_ => throw new ArgumentOutOfRangeException("type", type, null), 
		};
	}
}
