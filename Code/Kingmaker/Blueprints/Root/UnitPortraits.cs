using System;
using Kingmaker.UI.Common.UIConfigComponents;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class UnitPortraits
{
	[Header("Placeholder Portraits")]
	[SerializeField]
	private BlueprintPortraitReference m_MalePlaceholderPortrait;

	[SerializeField]
	private BlueprintPortraitReference m_FemalePlaceholderPortrait;

	[SerializeField]
	private BlueprintPortraitReference m_LeaderPlaceholderPortrait;

	[Header("Unit Subtype Icons")]
	[SerializeField]
	private EnumUnitSubtypeIconsReference m_UnitSubtypePortrait;

	[SerializeField]
	private EnumUnitSubtypeIconsReference m_UnitSubtypeIcons;

	public BlueprintPortrait MalePlaceholderPortrait => m_MalePlaceholderPortrait?.Get();

	public BlueprintPortrait FemalePlaceholderPortrait => m_FemalePlaceholderPortrait?.Get();

	public EnumUnitSubtypeIcons UnitSubtypePortrait => m_UnitSubtypePortrait?.Get();

	public EnumUnitSubtypeIcons UnitSubtypeIcons => m_UnitSubtypeIcons?.Get();
}
