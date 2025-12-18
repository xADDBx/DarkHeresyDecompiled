using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoArchetypeFeatureVM : ViewModel
{
	private const string NO_EMPTY_FIELD_TEXT = "---/ ? /---";

	public string Name { get; }

	public Sprite Icon { get; }

	public BlueprintFeature Feature { get; }

	public TooltipBaseTemplate Tooltip { get; protected set; }

	public CharInfoArchetypeFeatureVM(BlueprintFeature feature, TooltipBaseTemplate customTooltip = null, BaseUnitEntity unit = null)
	{
		Feature = feature;
		if (feature == null)
		{
			Name = "---/ ? /---";
			Icon = null;
			Tooltip = null;
		}
		else
		{
			Name = feature.LocalizedName;
			Icon = feature.Icon;
			Tooltip = customTooltip ?? new TooltipTemplateFeature(feature, withVariants: false, unit);
		}
	}
}
