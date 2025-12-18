using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class UnitBackgroundBlockVM : CharInfoComponentVM
{
	private readonly ReactiveProperty<BlueprintFeature> m_Homeworld = new ReactiveProperty<BlueprintFeature>();

	private readonly ReactiveProperty<BlueprintFeature> m_Occupation = new ReactiveProperty<BlueprintFeature>();

	private readonly ReactiveProperty<BlueprintFeature> m_MomentOfTriumph = new ReactiveProperty<BlueprintFeature>();

	private readonly ReactiveProperty<BlueprintFeature> m_DarkestHour = new ReactiveProperty<BlueprintFeature>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_HomeworldTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_OccupationTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_MomentOfTriumphTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_DarkestHourTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public ReadOnlyReactiveProperty<BlueprintFeature> Homeworld => m_Homeworld;

	public ReadOnlyReactiveProperty<BlueprintFeature> Occupation => m_Occupation;

	public ReadOnlyReactiveProperty<BlueprintFeature> MomentOfTriumph => m_MomentOfTriumph;

	public ReadOnlyReactiveProperty<BlueprintFeature> DarkestHour => m_DarkestHour;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> HomeworldTooltip => m_HomeworldTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> OccupationTooltip => m_OccupationTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> MomentOfTriumphTooltip => m_MomentOfTriumphTooltip;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> DarkestHourTooltip => m_DarkestHourTooltip;

	public UnitBackgroundBlockVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateSelections();
	}

	private BlueprintFeature GetCharGenSelectionFeature(BaseUnitEntity unit, FeatureGroup group)
	{
		BlueprintOriginPath unitOriginPath = UtilityChargen.GetUnitOriginPath(unit);
		if (unitOriginPath == null)
		{
			return null;
		}
		BlueprintSelectionFeature blueprintSelectionFeature = UtilityChargen.GetFeatureSelectionsByGroup(unitOriginPath, group).FirstOrDefault();
		if (blueprintSelectionFeature != null)
		{
			return unit.Progression.GetSelectedFeature(unitOriginPath, 0, blueprintSelectionFeature)?.Feature;
		}
		return null;
	}

	private void UpdateSelections(BaseUnitEntity unit = null)
	{
		if (unit == null)
		{
			unit = Unit.CurrentValue;
		}
		if (unit != null)
		{
			bool flag = !unit.IsMainCharacter;
			m_Homeworld.Value = GetCharGenSelectionFeature(unit, FeatureGroup.ChargenHomeworld);
			m_Occupation.Value = GetCharGenSelectionFeature(unit, FeatureGroup.ChargenOccupation);
			m_MomentOfTriumph.Value = (flag ? null : GetCharGenSelectionFeature(unit, FeatureGroup.ChargenMomentOfTriumph));
			m_DarkestHour.Value = (flag ? null : GetCharGenSelectionFeature(unit, FeatureGroup.ChargenDarkestHour));
			m_HomeworldTooltip.Value = new TooltipTemplateChargenBackground(Homeworld.CurrentValue, isInfoWindow: false);
			m_OccupationTooltip.Value = new TooltipTemplateChargenBackground(Occupation.CurrentValue, isInfoWindow: false);
			m_MomentOfTriumphTooltip.Value = new TooltipTemplateChargenBackground(MomentOfTriumph.CurrentValue, isInfoWindow: false);
			m_DarkestHourTooltip.Value = new TooltipTemplateChargenBackground(DarkestHour.CurrentValue, isInfoWindow: false);
		}
	}

	public void UpdateSelectionsFromUnit(BaseUnitEntity unit)
	{
		UpdateSelections(unit);
	}
}
