using System;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.EntitySystem.Entities;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class AbilitySelectorWindowVM : SelectorWindowVM<FeatureSelectorSlotVM>
{
	public readonly Action<CharInfoFeatureVM> OnFeatureFocused;

	public AbilitySelectorWindowVM(Action<CharInfoFeatureVM> onConfirm, Action onDecline, BaseUnitEntity unit, Action<CharInfoFeatureVM> onFeatureFocused)
		: base((Action<FeatureSelectorSlotVM>)onConfirm, onDecline, UIUtilityUnit.CollectAbilitiesVMs(unit).ToList(), (ReactiveProperty<FeatureSelectorSlotVM>)null, (EquipSlotVM)null)
	{
		OnFeatureFocused = onFeatureFocused;
	}
}
