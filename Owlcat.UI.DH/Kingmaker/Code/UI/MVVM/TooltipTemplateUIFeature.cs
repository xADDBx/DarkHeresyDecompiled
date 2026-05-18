using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateUIFeature : TooltipBaseTemplate
{
	public readonly UIFeature UIFeature;

	private BlueprintFeature BlueprintFeature => UIFeature.Feature;

	public TooltipTemplateUIFeature(UIFeature uiFeature)
	{
		UIFeature = uiFeature;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		Sprite defaultIfNull = UIFeature.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		TooltipTemplateFeature tooltipTemplateFeature = new TooltipTemplateFeature(UIFeature.Feature);
		TextValueElement title = new TextValueElement(UIFeature.Name);
		TooltipBaseTemplate tooltip = tooltipTemplateFeature;
		yield return new BrickLevelUpHeaderVM(new LevelUpFeatureUIData(title, null, null, null, defaultIfNull, default(Color), IconDecor.Default, null, tooltip));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		UIUtilityFeaturesTooltip.AddDescription(list, BlueprintFeature);
		list.Add(new BrickSpaceVM(10f));
		UIUtilityFeaturesTooltip.AddStatBonuses(list, BlueprintFeature, StatTypeHelper.Attributes, FeatureGroup.Attribute);
		UIUtilityFeaturesTooltip.AddStatBonuses(list, BlueprintFeature, StatTypeHelper.Skills, FeatureGroup.Skill);
		UIUtilityFeaturesTooltip.ParseByBlockAndAddFeatures(list, BlueprintFeature);
		UIUtilityFeaturesTooltip.AddAllLevelUpStatsAndFeatures(list, BlueprintFeature);
		return list;
	}

	protected virtual void AddDescription(List<ITooltipBrick> bricks)
	{
		bricks.Add(new BrickTextVM(UIFeature.Description));
	}
}
