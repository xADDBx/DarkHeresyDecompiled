using Kingmaker.Code.Gameplay.Controllers.Combat;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class UnitSelectionButtonView : View<CombatMechanicEntityVM>
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	protected override void OnBind()
	{
		m_Button.OnPointerEnterAsObservable().Subscribe(delegate
		{
			SetSelected(value: true);
			base.ViewModel.UnitAsBaseUnitEntity?.View.HandleHoverChange(isHover: true);
		}).AddTo(this);
		m_Button.OnPointerExitAsObservable().Subscribe(delegate
		{
			SetSelected(value: false);
			base.ViewModel.UnitAsBaseUnitEntity?.View.HandleHoverChange(isHover: false);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnRightClickAsObservable(), delegate
		{
			InvokeUnitInspect();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftDoubleClickAsObservable(), delegate
		{
			HandleLeftClick(isDoubleClick: true);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnSingleLeftClickAsObservable(), delegate
		{
			HandleLeftClick(isDoubleClick: false);
		}).AddTo(this);
	}

	private void SetSelected(bool value)
	{
		if (base.ViewModel?.MechanicEntity != null && base.ViewModel.UnitAsBaseUnitEntity != null && base.ViewModel.UnitAsBaseUnitEntity.View.MouseHoverHighlighting != value)
		{
			base.ViewModel.SetMouseHighlighted(value);
			m_Button.SetFocus(value);
		}
	}

	private void InvokeUnitInspect()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitRightClick(base.ViewModel.UnitAsBaseUnitEntity);
		});
	}

	private void HandleLeftClick(bool isDoubleClick)
	{
		if (!base.ViewModel.HasUnit)
		{
			return;
		}
		PreciseAttackController preciseAttackController = Game.Instance.Controllers.PreciseAttackController;
		if (preciseAttackController != null && preciseAttackController.HasTarget)
		{
			preciseAttackController.SelectTargetManual(base.ViewModel?.MechanicEntity, out var unavailabilityReason);
			if (!string.IsNullOrEmpty(unavailabilityReason))
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(unavailabilityReason, addToLog: false, WarningNotificationFormat.Attention);
				});
			}
		}
		else
		{
			Game.Instance.Controllers.CameraController?.Follower?.ScrollTo(base.ViewModel.MechanicEntity);
			base.ViewModel.HandleUnitClick(isDoubleClick);
		}
	}
}
