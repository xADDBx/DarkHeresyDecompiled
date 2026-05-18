using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.Parts.AdditionalCombat;

public class OvertipCombatUnitInteractionVM : ViewModel
{
	private readonly MechanicEntityUIState m_EntityState;

	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<IUnitInteraction> m_Interaction = new ReactiveProperty<IUnitInteraction>();

	private IDisposable m_TBMDisposable;

	public MechanicEntityUIState MechanicEntityUIState => m_EntityState;

	public ReadOnlyReactiveProperty<IUnitInteraction> Interaction => m_Interaction;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public OvertipCombatUnitInteractionVM(MechanicEntityUIState entityState)
	{
		m_EntityState = entityState;
		GameUIState.Instance.IsInCombat.Subscribe(UpdateCombatMode).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_TBMDisposable?.Dispose();
		m_TBMDisposable = null;
	}

	private void UpdateCombatMode(bool isInCombat)
	{
		if (!isInCombat)
		{
			m_TBMDisposable?.Dispose();
			m_IsVisible.Value = false;
			m_Interaction.Value = null;
		}
		else
		{
			m_TBMDisposable = ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
			{
				UpdateVisibility();
			});
		}
	}

	private void UpdateVisibility()
	{
		if (!m_EntityState.IsInCombat.CurrentValue || GameUIState.Instance.GameMode.CurrentValue == GameModeType.Cutscene)
		{
			m_IsVisible.Value = false;
			return;
		}
		m_Interaction.Value = UtilityUnit.GetUnitInteractionFrom(m_EntityState.MechanicEntity.MechanicEntity);
		PartAdditionalCombatObjectiveUnit additionalCombatObjective = m_EntityState.MechanicEntity.AdditionalCombatObjective;
		bool flag = additionalCombatObjective != null && !additionalCombatObjective.ObjectIsViewed;
		if (!flag && Interaction.CurrentValue != null)
		{
			flag = !((m_EntityState.MechanicEntity.MechanicEntity.View as AbstractUnitEntityView)?.WasHighlightedOnHover ?? false);
		}
		m_IsVisible.Value = flag;
	}
}
