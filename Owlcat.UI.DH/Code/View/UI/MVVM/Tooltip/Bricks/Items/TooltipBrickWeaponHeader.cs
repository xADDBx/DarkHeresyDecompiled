using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Components.Features;
using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickWeaponHeader : ITooltipBrick
{
	private readonly string m_MainTitle;

	private readonly Sprite m_Image;

	private readonly string m_ItemType;

	private readonly string m_ItemLabel;

	private readonly string m_ItemSubtitle;

	private readonly StatData m_Damage;

	private readonly bool m_HasUpgrade;

	private readonly List<WeaponTagUISettings> m_TagSettings;

	private readonly Dictionary<WeaponTagProperty, int> m_SpecialTagsValues;

	public TooltipBrickWeaponHeader(string mainTitle, Sprite image, StatData damage, bool hasUpgrade, List<WeaponTagUISettings> tagSettings, string itemType = null, string itemLabel = null, string itemSubtitle = null, Dictionary<WeaponTagProperty, int> specialTagsValues = null)
	{
		m_MainTitle = mainTitle;
		m_Image = image;
		m_Damage = damage;
		m_ItemType = itemType;
		m_ItemLabel = itemLabel;
		m_ItemSubtitle = itemSubtitle;
		m_HasUpgrade = hasUpgrade;
		m_TagSettings = tagSettings;
		m_SpecialTagsValues = specialTagsValues ?? new Dictionary<WeaponTagProperty, int>();
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickWeaponHeaderVM(m_MainTitle, m_Image, m_Damage, m_HasUpgrade, m_TagSettings, m_ItemType, m_ItemLabel, m_ItemSubtitle, m_SpecialTagsValues);
	}
}
