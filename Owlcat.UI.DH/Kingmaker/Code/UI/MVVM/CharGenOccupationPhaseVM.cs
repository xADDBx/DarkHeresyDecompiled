using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.Visual.CharacterSystem;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenOccupationPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	private readonly Dictionary<CharGenBackgroundBaseItemVM, (int, int)> m_RampIndicesMap = new Dictionary<CharGenBackgroundBaseItemVM, (int, int)>();

	public CharGenOccupationPhaseVM(CharGenContext charGenContext, SelectionStateFeature selectionStateFeature, InfoSectionVM infoSectionVM)
		: base(charGenContext, selectionStateFeature, CharGenPhaseType.Occupation, infoSectionVM, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
		base.DisplayMode = CharGenDisplayMode.DollOnly;
		m_OnSelectionApplied = UpdateColorsForSelected;
		AddDisposable(ObservableSubscribeExtensions.Subscribe(m_CharGenContext.Doll.UpdateCommand, delegate
		{
			DollState doll = m_CharGenContext.Doll;
			if (doll != null && base.SelectedItem.CurrentValue != null && m_RampIndicesMap.ContainsKey(base.SelectedItem.CurrentValue))
			{
				m_RampIndicesMap[base.SelectedItem.CurrentValue] = (doll.EquipmentRampIndex, doll.EquipmentRampIndexSecondary);
			}
		}));
	}

	private void UpdateColorsForSelected()
	{
		if (m_RampIndicesMap.TryGetValue(base.SelectedItem.CurrentValue, out var value))
		{
			m_CharGenContext.Doll.SetEquipColors(value.Item1, value.Item2);
			return;
		}
		UtilityChargen.GetClothesColorsProfile(m_CharGenContext.Doll.Clothes, out var colorPreset);
		if (!(colorPreset == null) && colorPreset.IndexPairs.Count > 0)
		{
			RampColorPreset.IndexSet indexSet = colorPreset.IndexPairs[0];
			m_CharGenContext.Doll.SetEquipColors(indexSet.PrimaryIndex, indexSet.SecondaryIndex);
			m_RampIndicesMap.Add(base.SelectedItem.CurrentValue, (indexSet.PrimaryIndex, indexSet.SecondaryIndex));
		}
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenOccupationItemVM(selectionItem, selectionStateFeature, phaseType, base.OnHoverItem, m_CharGenContext.LevelUpManager.CurrentValue);
	}

	public void HandleDollStateUpdated(DollState dollState)
	{
		if (m_RampIndicesMap.ContainsKey(base.SelectedItem.CurrentValue))
		{
			m_RampIndicesMap[base.SelectedItem.CurrentValue] = (dollState.EquipmentRampIndex, dollState.EquipmentRampIndexSecondary);
		}
	}
}
