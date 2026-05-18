using Kingmaker.UnitLogic.Levelup.Selections;
using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM;

public class BackgroundFeatureVM : ViewModel
{
	public readonly Sprite Icon;

	public readonly string Name;

	public readonly string FeatureTypeName;

	public FeatureGroup FeatureGroup;

	public readonly TooltipBaseTemplate Tooltip;

	public BackgroundFeatureVM(Sprite icon, string name, string featureTypeName, FeatureGroup group, TooltipBaseTemplate tooltip)
	{
		Icon = icon;
		Name = name;
		FeatureTypeName = featureTypeName;
		FeatureGroup = group;
		Tooltip = tooltip;
	}
}
