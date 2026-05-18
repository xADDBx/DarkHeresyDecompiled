using Kingmaker.Code.Gameplay.Components;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public class UnitInfoPartBuffs : UnitInfoPart
{
	[SerializeField]
	private UnitInfoBuffBlockView m_UnitInfoBuffBlockView;

	protected override void OnBind()
	{
		base.OnBind();
		m_UnitInfoBuffBlockView.Bind(base.ViewModel.BuffBlockVM);
	}

	protected override void ShowImpl(UnitInfoPartState state)
	{
		bool active = !base.ViewModel.IsPreciseAttack.CurrentValue && base.ViewModel.BuffBlockVM.HasBuffs && !base.ViewModel.HasSettingsFlags(UnitInspectUIFlags.HideStatusEffects) && !state.IsDeadOrUnconscious;
		base.gameObject.SetActive(active);
	}
}
