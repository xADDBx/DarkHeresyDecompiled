using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.Visual.CharacterSystem;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenOccupationPhaseVM : CharGenBackgroundBasePhaseVM<CharGenBackgroundBaseItemVM>
{
	private readonly Dictionary<CharGenBackgroundBaseItemVM, (int, int)> m_RampIndicesMap = new Dictionary<CharGenBackgroundBaseItemVM, (int, int)>();

	public CharGenOccupationPhaseVM(CharGenContext charGenContext)
		: base(charGenContext, FeatureGroup.ChargenOccupation, CharGenPhaseType.Occupation, (ReactiveProperty<CharGenPhaseBaseVM>)null)
	{
		OnSelectionApplied = UpdateColorsForSelected;
		AddDisposable(ObservableSubscribeExtensions.Subscribe(CharGenContext.Doll.UpdateCommand, delegate
		{
			DollState doll = CharGenContext.Doll;
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
			CharGenContext.Doll.SetEquipColors(value.Item1, value.Item2);
			return;
		}
		UtilityChargen.GetClothesColorsProfile(CharGenContext.Doll.Clothes, out var colorPreset);
		if (!(colorPreset == null) && colorPreset.IndexPairs.Count > 0)
		{
			RampColorPreset.IndexSet indexSet = colorPreset.IndexPairs[0];
			CharGenContext.Doll.SetEquipColors(indexSet.PrimaryIndex, indexSet.SecondaryIndex);
			m_RampIndicesMap.Add(base.SelectedItem.CurrentValue, (indexSet.PrimaryIndex, indexSet.SecondaryIndex));
		}
	}

	protected override CharGenBackgroundBaseItemVM CreateItem(FeatureSelectionItem selectionItem, SelectionStateFeature selectionStateFeature, CharGenPhaseType phaseType)
	{
		return new CharGenOccupationItemVM(selectionItem, selectionStateFeature, phaseType);
	}

	public void HandleDollStateUpdated(DollState dollState)
	{
		if (m_RampIndicesMap.ContainsKey(base.SelectedItem.CurrentValue))
		{
			m_RampIndicesMap[base.SelectedItem.CurrentValue] = (dollState.EquipmentRampIndex, dollState.EquipmentRampIndexSecondary);
		}
	}
}
