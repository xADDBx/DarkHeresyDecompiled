using System;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.Parts.Interaction;

public class OvertipInteractionBlockPCView : View<OvertipInteractionBlockVM>
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private OwlcatMultiButton[] m_ChangeUnitInfoDetailsManualButtons;

	[SerializeField]
	private OwlcatMultiButton m_MoraleTooltipButton;

	private ReactiveProperty<UnitOvertipVisibility> m_Visibility;

	private IDisposable m_TooltipDisposable;

	private bool IsVisible
	{
		get
		{
			if (m_Visibility.Value == UnitOvertipVisibility.Maximized && base.ViewModel.MechanicEntityUIState.IsInCombat.CurrentValue && !base.ViewModel.MechanicEntityUIState.HasHiddenCondition.CurrentValue && base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed.CurrentValue)
			{
				return !base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue;
			}
			return false;
		}
	}

	private bool CheckButtonCanBePressed
	{
		get
		{
			if (base.ViewModel.MechanicEntityUIState.IsInCombat.CurrentValue && !base.ViewModel.MechanicEntityUIState.HasHiddenCondition.CurrentValue && base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed.CurrentValue)
			{
				return !base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue;
			}
			return false;
		}
	}

	public void SetVisibility(ReactiveProperty<UnitOvertipVisibility> visibilityProperty)
	{
		m_Visibility = visibilityProperty;
	}

	protected override void OnBind()
	{
		m_Visibility.CombineLatest(base.ViewModel.MechanicEntityUIState.IsMouseOverUnit, base.ViewModel.MechanicEntityUIState.HasHiddenCondition, base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed, base.ViewModel.MechanicEntityUIState.IsInCombat, base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead, (UnitOvertipVisibility _, bool _, bool _, bool _, bool _, bool _) => true).Subscribe(delegate
		{
			DoVisibility();
		});
		OwlcatMultiButton[] changeUnitInfoDetailsManualButtons = m_ChangeUnitInfoDetailsManualButtons;
		for (int i = 0; i < changeUnitInfoDetailsManualButtons.Length; i++)
		{
			changeUnitInfoDetailsManualButtons[i].OnLeftClickAsObservable().Subscribe(ChangeUnitInfoDetailsManual).AddTo(this);
		}
		base.ViewModel.MechanicEntityUIState.Morale.Subscribe(SetTooltip).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_TooltipDisposable?.Dispose();
		m_TooltipDisposable = null;
	}

	private void DoVisibility()
	{
		if (IsVisible)
		{
			m_Animator.AppearAnimation();
		}
		else
		{
			m_Animator.DisappearAnimation();
		}
	}

	private void ChangeUnitInfoDetailsManual()
	{
		if (CheckButtonCanBePressed)
		{
			EventBus.RaiseEvent(delegate(IUnitInfoDetailsUIHandler h)
			{
				h.HandleUnitManual(base.ViewModel.MechanicEntityUIState.MechanicEntity.MechanicEntity);
			});
		}
	}

	private void SetTooltip(IUIUnitMoraleData morale)
	{
		m_TooltipDisposable?.Dispose();
		if (morale.MoralePhase != 0)
		{
			m_TooltipDisposable = m_MoraleTooltipButton.SetTooltip(new TooltipTemplateMoralePhase(morale.MoralePhase));
		}
	}
}
