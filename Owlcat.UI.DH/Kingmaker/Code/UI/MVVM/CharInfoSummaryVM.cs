using Kingmaker.EntitySystem.Entities;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoSummaryVM : CharInfoComponentVM
{
	private readonly ReactiveProperty<ActionPointsVM> m_ActionPointVM = new ReactiveProperty<ActionPointsVM>();

	private readonly ReactiveProperty<bool> m_IsInCombat = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<CharInfoStatusEffectsVM> m_StatusEffects = new ReactiveProperty<CharInfoStatusEffectsVM>();

	public ReadOnlyReactiveProperty<ActionPointsVM> ActionPointVM => m_ActionPointVM;

	public ReadOnlyReactiveProperty<bool> IsInCombat => m_IsInCombat;

	public ReadOnlyReactiveProperty<CharInfoStatusEffectsVM> StatusEffects => m_StatusEffects;

	public CharInfoSummaryVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit)
		: base(unit)
	{
		m_StatusEffects.Value = new CharInfoStatusEffectsVM(unit).AddTo(this);
	}

	protected override void RefreshData()
	{
		base.RefreshData();
		UpdateData();
	}

	private void UpdateData()
	{
		ActionPointVM.CurrentValue?.Dispose();
		m_ActionPointVM.Value = new ActionPointsVM(UnitUIWrapper.MechanicEntity).AddTo(this);
		m_IsInCombat.Value = UnitUIWrapper.IsInCombat;
	}
}
