using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateChargenOccupation : TooltipBaseTemplate
{
	protected readonly BlueprintFeature m_Feature;

	private readonly MechanicEntity m_Unit;

	private readonly LevelUpManager m_LevelUpManager;

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

	public TooltipTemplateChargenOccupation(BlueprintFeature feature, MechanicEntity unit = null, LevelUpManager levelUpManager = null)
	{
		m_Feature = feature;
		m_Unit = unit;
		m_LevelUpManager = levelUpManager;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickOriginTitleVM(m_Feature.Icon, m_Feature.Name, UIStrings.Instance.CharGen.Origin.Text);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddKeystoneFeaturesOrDescription(list);
		list.Add(new BrickChargenDividerTextLineVM(DividerType.Default));
		UIUtilityFeaturesTooltip.AddStatBonuses(list, m_Feature, StatTypeHelper.Attributes, FeatureGroup.Attribute);
		UIUtilityFeaturesTooltip.AddStatBonuses(list, m_Feature, StatTypeHelper.Skills, FeatureGroup.Skill);
		UIUtilityFeaturesTooltip.ParseByBlockAndAddFeatures(list, m_Feature);
		UIUtilityFeaturesTooltip.AddAllLevelUpStatsAndFeatures(list, m_Feature);
		return list;
	}

	private void AddKeystoneFeaturesOrDescription(List<ITooltipBrick> bricks)
	{
		List<AddKeystoneFeatureInfo> list = m_Feature?.GetComponents<AddKeystoneFeatureInfo>().ToList();
		if (!list.Empty())
		{
			list.ForEach(delegate(AddKeystoneFeatureInfo k)
			{
				bricks.Add(new BrickFeatureDescriptionVM(k, FallbackCaster));
			});
		}
		else
		{
			UIUtilityFeaturesTooltip.AddDescription(bricks, m_Feature);
		}
	}
}
