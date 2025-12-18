using Kingmaker.Code.View.Bridge.Data;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Code.UI.MVVM;

public class UIFeature : FeatureUIData
{
	public int Level;

	public int Rank;

	public bool isBead;

	public FeatureGroup Type;

	public TalentIconInfo TalentIconsInfo;

	public UIFeature(BlueprintFeature feature, FeatureParam param = null)
		: base(feature, param)
	{
		TalentIconsInfo = feature.TalentIconInfo;
	}

	public UIFeature(Feature feature, FeatureParam param = null)
		: this(feature.Blueprint, param)
	{
		Rank = feature.Rank;
	}
}
