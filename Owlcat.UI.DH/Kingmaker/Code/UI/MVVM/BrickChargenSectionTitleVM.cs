using Kingmaker.UnitLogic.Levelup.Selections;

namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenSectionTitleVM : TooltipBrickVM
{
	public readonly FeatureGroup FeatureGroup;

	public readonly TitleType TitleType;

	public BrickChargenSectionTitleVM(FeatureGroup featureGroup, TitleType titleType)
	{
		FeatureGroup = featureGroup;
		TitleType = titleType;
	}
}
