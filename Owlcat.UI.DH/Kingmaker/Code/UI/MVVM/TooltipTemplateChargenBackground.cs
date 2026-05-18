using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateChargenBackground : TooltipBaseTemplate
{
	protected readonly BlueprintFeature m_Feature;

	public TooltipTemplateChargenBackground(BlueprintFeature feature, bool _ = true)
	{
		m_Feature = feature;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickOriginTitleVM(m_Feature.Icon, m_Feature.Name, UIStrings.Instance.CharGen.Origin.Text);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		UIUtilityFeaturesTooltip.AddDescription(list, m_Feature);
		list.Add(new BrickSpaceVM(10f));
		UIUtilityFeaturesTooltip.AddStatBonuses(list, m_Feature, StatTypeHelper.Attributes, FeatureGroup.Attribute);
		UIUtilityFeaturesTooltip.AddStatBonuses(list, m_Feature, StatTypeHelper.Skills, FeatureGroup.Skill);
		UIUtilityFeaturesTooltip.ParseByBlockAndAddFeatures(list, m_Feature);
		UIUtilityFeaturesTooltip.AddAllLevelUpStatsAndFeatures(list, m_Feature);
		return list;
	}
}
