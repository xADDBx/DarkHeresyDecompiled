using System;
using Kingmaker.Gameplay.Parts;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.Parts.AdditionalCombat;

public class OvertipAdditionalCombatObjectBlockVM : ViewModel
{
	private readonly MechanicEntityUIState m_EntityState;

	private readonly ReactiveProperty<bool> m_IsNewAdditionalCombatObject = new ReactiveProperty<bool>();

	private IDisposable m_TBMDisposable;

	public ReadOnlyReactiveProperty<bool> IsNewAdditionalCombatObject => m_IsNewAdditionalCombatObject;

	public OvertipAdditionalCombatObjectBlockVM(MechanicEntityUIState entityState)
	{
		m_EntityState = entityState;
		if (entityState.MechanicEntity.AdditionalCombatObjective != null)
		{
			GameUIState.Instance.IsInCombat.Subscribe(UpdateCombatMode).AddTo(this);
		}
	}

	private void UpdateCombatMode(bool isInCombat)
	{
		if (!isInCombat)
		{
			m_TBMDisposable?.Dispose();
			return;
		}
		m_TBMDisposable = ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			PartAdditionalCombatObjectiveUnit additionalCombatObjective = m_EntityState.MechanicEntity.AdditionalCombatObjective;
			bool flag = additionalCombatObjective != null && !additionalCombatObjective.ObjectIsViewed;
			m_IsNewAdditionalCombatObject.Value = m_EntityState.IsInCombat.CurrentValue && flag;
		}).AddTo(this);
	}
}
