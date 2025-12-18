using System.Collections.Generic;
using Kingmaker.Code.Gameplay.Components.Features;
using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickArmourHeader : ITooltipBrick
{
	private readonly string m_MainTitle;

	private readonly Sprite m_Image;

	private readonly string m_ItemType;

	private readonly string m_ItemLabel;

	private readonly StatData m_ArmourDurability;

	private readonly StatData m_DamageReduction;

	private readonly bool m_HasUpgrade;

	private readonly List<ArmourTagUISettings> m_TagSettings;

	public TooltipBrickArmourHeader(string mainTitle, Sprite image, StatData armourDurability, StatData damageReduction, bool hasUpgrade, List<ArmourTagUISettings> tagSettings, string itemType = null, string itemLabel = null)
	{
		m_MainTitle = mainTitle;
		m_Image = image;
		m_ArmourDurability = armourDurability;
		m_DamageReduction = damageReduction;
		m_ItemType = itemType;
		m_ItemLabel = itemLabel;
		m_HasUpgrade = hasUpgrade;
		m_TagSettings = tagSettings;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickArmourHeaderVM(m_MainTitle, m_Image, m_ArmourDurability, m_DamageReduction, m_HasUpgrade, m_TagSettings, m_ItemType, m_ItemLabel);
	}
}
