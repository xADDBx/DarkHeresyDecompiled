using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelUpSpecialization : TooltipBaseTemplate
{
	private readonly UIFeature m_UIFeature;

	private readonly MechanicEntity m_Unit;

	private readonly LevelUpManager m_LevelUpManager;

	private BlueprintFeature Feature => m_UIFeature.Feature;

	private MechanicEntity FallbackCaster
	{
		get
		{
			object obj = m_Unit;
			if (obj == null)
			{
				LevelUpManager levelUpManager = m_LevelUpManager;
				if (levelUpManager == null)
				{
					return null;
				}
				obj = levelUpManager.PreviewUnit;
			}
			return (MechanicEntity)obj;
		}
	}

	public TooltipTemplateLevelUpSpecialization(UIFeature feature, MechanicEntity unit = null, LevelUpManager levelUpManager = null)
	{
		m_UIFeature = feature;
		m_Unit = unit;
		m_LevelUpManager = levelUpManager;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		Sprite defaultIfNull = m_UIFeature.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		yield return new BrickLevelUpHeaderVM(new LevelUpFeatureUIData(new TextValueElement(m_UIFeature.Name), null, null, null, defaultIfNull));
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddKeystoneFeature(list);
		list.Add(new BrickSpaceVM(10f));
		UIUtilityFeaturesTooltip.AddStatBonuses(list, Feature, StatTypeHelper.Attributes, FeatureGroup.Attribute);
		UIUtilityFeaturesTooltip.AddStatBonuses(list, Feature, StatTypeHelper.Skills, FeatureGroup.Skill);
		UIUtilityFeaturesTooltip.ParseByBlockAndAddFeatures(list, Feature, null, new FeatureGroup[1] { FeatureGroup.ActiveAbility });
		UIUtilityFeaturesTooltip.AddAllLevelUpStatsAndFeatures(list, Feature);
		return list;
	}

	private void AddKeystoneFeature(List<ITooltipBrick> bricks)
	{
		List<AddKeystoneFeatureInfo> list = Feature.GetComponents<AddKeystoneFeatureInfo>().ToList();
		if (!list.Empty())
		{
			list.ForEach(delegate(AddKeystoneFeatureInfo k)
			{
				bricks.Add(new BrickFeatureDescriptionVM(k, FallbackCaster));
			});
			return;
		}
		BlueprintFeature blueprintFeature = Feature.GetComponents<AddFacts>().SelectMany((AddFacts i) => i.Facts).OfType<BlueprintFeature>()
			.FirstOrDefault();
		if (blueprintFeature != null)
		{
			bricks.Add(new BrickFeatureDescriptionVM(blueprintFeature, FallbackCaster));
			return;
		}
		UIUtilityFeaturesTooltip.AddDescription(bricks, Feature, FallbackCaster);
		bricks.Add(new BrickSpaceVM(10f));
	}
}
