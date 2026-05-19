using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Prerequisites;
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
		AddPrerequisites(list);
		AddKeystoneFeaturesOrDescription(list);
		list.Add(new BrickChargenDividerTextLineVM(DividerType.Default));
		UIUtilityFeaturesTooltip.AddStatBonuses(list, m_Feature, StatTypeHelper.Attributes, FeatureGroup.Attribute);
		UIUtilityFeaturesTooltip.AddStatBonuses(list, m_Feature, StatTypeHelper.Skills, FeatureGroup.Skill);
		UIUtilityFeaturesTooltip.ParseByBlockAndAddFeatures(list, m_Feature);
		UIUtilityFeaturesTooltip.AddChargenProgressionLockedFoldable(list, m_Feature);
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

	private void AddPrerequisites(List<ITooltipBrick> bricks)
	{
		PrerequisitesList prerequisitesList = m_Feature?.Prerequisites;
		if (prerequisitesList != null && !prerequisitesList.Empty && FallbackCaster is BaseUnitEntity caster)
		{
			List<AbilityRestrictionGroupVM> list = BuildPrerequisiteGroups(prerequisitesList, caster);
			if (list.Count != 0 && !list.All((AbilityRestrictionGroupVM g) => g.IsPassed))
			{
				bricks.Add(new BrickChargenSelectionRestrictionsVM(list, UIStrings.Instance.Tooltips.Prerequisites));
			}
		}
	}

	private static List<AbilityRestrictionGroupVM> BuildPrerequisiteGroups(PrerequisitesList list, BaseUnitEntity caster)
	{
		List<AbilityRestrictionGroupVM> list2 = new List<AbilityRestrictionGroupVM>();
		bool isLogicalOr = list.Composition == FeaturePrerequisiteComposition.Or;
		Prerequisite[] list3 = list.List;
		foreach (Prerequisite prerequisite in list3)
		{
			string text = (prerequisite.Not ? ((string)UIStrings.Instance.Tooltips.NoFeature) : string.Empty);
			string text2 = ((!(prerequisite is PrerequisiteFact prerequisiteFact)) ? string.Empty : prerequisiteFact.Fact.Name);
			string text3 = text2;
			AbilityRestrictionEntry abilityRestrictionEntry = new AbilityRestrictionEntry(text + " " + text3, prerequisite.Meet(caster));
			list2.Add(new AbilityRestrictionGroupVM(new AbilityRestrictionEntry[1] { abilityRestrictionEntry }, caster, isLogicalOr));
		}
		list2.Sort(delegate(AbilityRestrictionGroupVM a, AbilityRestrictionGroupVM b)
		{
			if (a.ShowLogicalOr == b.ShowLogicalOr)
			{
				return 0;
			}
			return a.ShowLogicalOr ? 1 : (-1);
		});
		return list2;
	}
}
