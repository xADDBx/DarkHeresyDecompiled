using System;
using Code.View.UI.MVVM;
using Kingmaker.UnitLogic.Levelup.Selections;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class TooltipsConfig
{
	[field: SerializeField]
	public Color SingleAcronymColor { get; private set; }

	[field: SerializeField]
	public Color GroupAcronymColor { get; private set; }

	[field: SerializeField]
	public EnumToObjectSelector<FeatureGroup, Sprite> FeatureGroupsIcons { get; private set; } = new EnumToObjectSelector<FeatureGroup, Sprite>();


	[field: SerializeField]
	public EnumToObjectSelector<TalentGroup, TalentGroupData> TalentGroupsData { get; private set; } = new EnumToObjectSelector<TalentGroup, TalentGroupData>();


	public Color GetAcronymColor(bool? hasGroups)
	{
		if (!hasGroups.GetValueOrDefault())
		{
			return SingleAcronymColor;
		}
		return GroupAcronymColor;
	}
}
