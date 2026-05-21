using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using UnityEngine;

namespace Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateChargenCareerPath : TooltipBaseTemplate
{
	private readonly CareerPathDevelopmentVector[] m_SortedDevelopmentVectors = new CareerPathDevelopmentVector[6]
	{
		CareerPathDevelopmentVector.Range,
		CareerPathDevelopmentVector.Psykana,
		CareerPathDevelopmentVector.Movement,
		CareerPathDevelopmentVector.Buff,
		CareerPathDevelopmentVector.Defence,
		CareerPathDevelopmentVector.Melee
	};

	private readonly FeatureGroup[] m_DefaultSelections = new FeatureGroup[6]
	{
		FeatureGroup.AbilityUpgrade,
		FeatureGroup.KeystoneFeature,
		FeatureGroup.ActiveAbility,
		FeatureGroup.Specialization,
		FeatureGroup.Talent,
		FeatureGroup.Modifier
	};

	protected readonly BlueprintCareerPath m_CareerPath;

	private readonly MechanicEntity m_Unit;

	private readonly LevelUpManager m_LevelUpManager;

	private readonly BlueprintSelectionWithUI m_BlueprintSelectionWithUI;

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

	public TooltipTemplateChargenCareerPath(BlueprintCareerPath careerPath, MechanicEntity unit = null, LevelUpManager levelUpManager = null, BlueprintSelectionWithUI blueprintSelectionWithUI = null)
	{
		m_CareerPath = careerPath;
		m_Unit = unit;
		m_LevelUpManager = levelUpManager;
		m_BlueprintSelectionWithUI = blueprintSelectionWithUI;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		Sprite defaultIfNull = m_CareerPath.Icon.GetDefaultIfNull(DefaultImageType.Ability);
		string name = m_CareerPath.Name;
		LocalizedString localizedString = m_BlueprintSelectionWithUI?.Title;
		yield return new BrickCareerTitleVM(defaultIfNull, name, (localizedString != null) ? ((string)localizedString) : UIStrings.Instance.CharGen.Career.Text);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddDescription(list);
		list.Add(new BrickSpaceVM(10f));
		AddKeystoneFeature(list);
		list.Add(new BrickSpaceVM(10f));
		AddLevelUpFeatures(list);
		return list;
	}

	private void AddLevelUpFeatures(List<ITooltipBrick> bricks)
	{
		BlueprintFeature blueprintFeature = m_CareerPath?.GetComponents<AddFacts>()?.SelectMany((AddFacts f) => f.Facts).OfType<BlueprintFeature>().FirstOrDefault((BlueprintFeature f) => f.FeatureTypes.Contains(BlueprintFeature.FeatureType.LevelUpFeature));
		if (blueprintFeature != null)
		{
			UIUtilityFeaturesTooltip.ParseByBlockAndAddFeatures(bricks, blueprintFeature);
			UIUtilityFeaturesTooltip.AddChargenProgressionLockedFoldable(bricks, blueprintFeature);
		}
	}

	protected void AddDescription(List<ITooltipBrick> bricks)
	{
		string fullDescription = TooltipTemplateUtils.GetFullDescription(m_CareerPath);
		fullDescription = (fullDescription.Empty() ? UIStrings.Instance.CharacterSheet.CareerPathDescription.Text : fullDescription);
		List<float> schemeData = m_SortedDevelopmentVectors.Select((CareerPathDevelopmentVector v) => (float)m_CareerPath.GetDevelopmentVectorValue(v) / 4f).ToList();
		bricks.Add(new BrickChargenArchetypeInfoBlockVM(fullDescription, schemeData));
	}

	private void AddKeystoneFeature(List<ITooltipBrick> bricks)
	{
		List<AddKeystoneFeatureInfo> list = m_CareerPath?.GetComponents<AddKeystoneFeatureInfo>().ToList();
		if (!list.Empty())
		{
			list.ForEach(delegate(AddKeystoneFeatureInfo k)
			{
				bricks.Add(new BrickFeatureDescriptionVM(k, FallbackCaster));
			});
			return;
		}
		BlueprintCareerPath careerPath = m_CareerPath;
		object obj;
		if (careerPath == null)
		{
			obj = null;
		}
		else
		{
			BlueprintPath.RankEntry rankEntry = careerPath.GetRankEntry(1);
			obj = ((rankEntry != null) ? rankEntry.Features.Where((BlueprintFeature f) => !f.HideInUI).ToList() : null);
		}
		List<BlueprintFeature> list2 = (List<BlueprintFeature>)obj;
		if (list2 != null)
		{
			list2.AddRange(GetKeystonesFromSelections(isInstant: true));
			list2.ForEach(delegate(BlueprintFeature a)
			{
				bricks.Add(new BrickFeatureDescriptionVM(a, FallbackCaster));
			});
			List<BlueprintFeature> list3 = (from f in m_CareerPath.RankEntries.SelectMany((BlueprintPath.RankEntry i) => i.Features).Except(list2)
				where !f.HideInUI
				select f).ToList();
			list3.AddRange(GetKeystonesFromSelections(isInstant: false));
			if (list3.Any())
			{
				bricks.Add(new BrickChargenDividerTextLineVM(DividerType.Dark, UIStrings.Instance.CharGen.LevelupNeededLabel.Text));
				bricks.Add(new BrickChargenSectionTitleVM(FeatureGroup.KeystoneFeature, TitleType.LevelupNeeded));
			}
			list3.ForEach(delegate(BlueprintFeature a)
			{
				bricks.Add(new BrickFeatureDescriptionVM(a, FallbackCaster));
			});
		}
	}

	private List<BlueprintFeature> GetKeystonesFromSelections(bool isInstant)
	{
		if (m_CareerPath == null)
		{
			return new List<BlueprintFeature>();
		}
		List<FeatureGroup> nonDefaultSelections = new List<FeatureGroup>();
		for (int i = 0; i < m_CareerPath.RankEntries.Length; i++)
		{
			nonDefaultSelections.AddRange(from s in m_CareerPath.RankEntries[i].Selections.OfType<BlueprintSelectionFeature>()
				where !m_DefaultSelections.Contains(s.Group)
				select s.Group);
			if (isInstant)
			{
				break;
			}
		}
		AddFacts component = m_CareerPath.GetComponent<AddFacts>();
		if (component == null)
		{
			return null;
		}
		return (from f in component.Facts.OfType<BlueprintFeature>()
			where f.FeatureTypes.Contains(BlueprintFeature.FeatureType.LevelUpFeature)
			select f).SelectMany((BlueprintFeature f) => f.GetComponents<AddFeaturesToLevelUp>()?.Where((AddFeaturesToLevelUp feat) => nonDefaultSelections.Contains(feat.Group)).ToList()).SelectMany((AddFeaturesToLevelUp f) => f.Features).ToList();
	}
}
