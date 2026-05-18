using System;
using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UIDataProvider;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateChargenUnitInformation : TooltipBaseTemplate
{
	private readonly BaseUnitEntity m_Unit;

	private readonly LevelUpManager m_LevelUpManager;

	private readonly IList<CareerPathVM> m_Careers;

	public TooltipTemplateChargenUnitInformation(BaseUnitEntity unit, LevelUpManager levelUpManager, IList<CareerPathVM> careers)
	{
		m_Unit = unit;
		m_LevelUpManager = levelUpManager;
		m_Careers = careers;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(m_Unit.CharacterName);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		AddCareers(list);
		AddBackgroundItems(list);
		AddAbilities(list);
		return list;
	}

	private void AddCareers(List<ITooltipBrick> bricks)
	{
		if (m_Careers.Count != 0)
		{
			bricks.Add(new BrickTitleVM(UIStrings.Instance.CharacterSheet.LevelProgression, TooltipTitleType.H3));
			bricks.Add(new BricksGroupOneColumnVM(((IEnumerable<CareerPathVM>)m_Careers).Select((Func<CareerPathVM, TooltipBrickVM>)((CareerPathVM career) => new BrickTitleWithIconVM(career))).ToList()));
			bricks.Add(new BrickSpaceVM(14f));
		}
	}

	private void AddBackgroundItems(List<ITooltipBrick> bricks)
	{
		bricks.Add(new BrickTitleVM(UIStrings.Instance.CharGen.Background, TooltipTitleType.H3));
		List<TooltipBrickVM> list = new List<TooltipBrickVM>();
		ITooltipBrick backgroundBrick = GetBackgroundBrick(FeatureGroup.ChargenHomeworld);
		if (backgroundBrick != null)
		{
			list.Add((TooltipBrickVM)backgroundBrick);
		}
		ITooltipBrick backgroundBrick2 = GetBackgroundBrick(FeatureGroup.ChargenImperialWorld);
		if (backgroundBrick2 != null)
		{
			list.Add((TooltipBrickVM)backgroundBrick2);
		}
		ITooltipBrick backgroundBrick3 = GetBackgroundBrick(FeatureGroup.ChargenOccupation);
		if (backgroundBrick3 != null)
		{
			list.Add((TooltipBrickVM)backgroundBrick3);
		}
		ITooltipBrick backgroundBrick4 = GetBackgroundBrick(FeatureGroup.ChargenMomentOfTriumph);
		if (backgroundBrick4 != null)
		{
			list.Add((TooltipBrickVM)backgroundBrick4);
		}
		ITooltipBrick backgroundBrick5 = GetBackgroundBrick(FeatureGroup.ChargenDarkestHour);
		if (backgroundBrick5 != null)
		{
			list.Add((TooltipBrickVM)backgroundBrick5);
		}
		bricks.Add(new BricksGroupOneColumnVM(list));
		bricks.Add(new BrickSpaceVM(14f));
	}

	private ITooltipBrick GetBackgroundBrick(FeatureGroup group)
	{
		BlueprintPath path = m_LevelUpManager.Path;
		if (path == null)
		{
			return null;
		}
		BlueprintSelectionFeature blueprintSelectionFeature = UtilityChargen.GetFeatureSelectionsByGroup(path, group).FirstOrDefault();
		if (blueprintSelectionFeature == null)
		{
			return null;
		}
		(BlueprintFeature, int)? selectedFeature = m_Unit.Progression.GetSelectedFeature(path, 0, blueprintSelectionFeature);
		if (selectedFeature.HasValue)
		{
			return new BrickFeatureVM(selectedFeature.Value.Item1, isHeader: false, available: true, showIcon: true, new TooltipTemplateChargenBackground(selectedFeature.Value.Item1, _: false), m_Unit);
		}
		return null;
	}

	private void AddAbilities(List<ITooltipBrick> bricks)
	{
		List<IUIDataProvider> list = new List<IUIDataProvider>();
		list.AddRange(UIUtilityUnit.CollectAbilities(m_Unit));
		list.AddRange(UIUtilityUnit.CollectToggleAbilities(m_Unit));
		bricks.Add(new BrickTitleVM(UIStrings.Instance.CharacterSheet.Abilities, TooltipTitleType.H3));
		bricks.Add(new BricksGroupOneColumnVM(((IEnumerable<IUIDataProvider>)list).Select((Func<IUIDataProvider, TooltipBrickVM>)((IUIDataProvider ability) => new BrickFeatureVM(ability))).ToList()));
	}
}
