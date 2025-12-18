using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarSlotAbilityConsoleView : ActionBarSlotAbilityView, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IHasTooltipTemplate
{
	[Header("ConsoleSlot")]
	[SerializeField]
	private ActionBarSlotConsoleView m_SlotConsoleView;

	public List<ContextMenuCollectionEntity> ContextMenuEntities = new List<ContextMenuCollectionEntity>();

	protected override void OnBind()
	{
		base.OnBind();
		m_SlotConsoleView.Bind(base.ViewModel);
		base.ViewModel.IsEmpty.Subscribe(delegate
		{
			SetContextMenuEntities();
		}).AddTo(this);
		base.ViewModel.IsSelectionBusy.Subscribe(SetSelectionActiveState).AddTo(this);
	}

	private void SetContextMenuEntities()
	{
		ContextMenuEntities.Clear();
		ContextMenuEntities.Add(new ContextMenuCollectionEntity(UIStrings.Instance.CharacterSheet.ChooseActiveAbilityFeatureGroupHint, base.ViewModel.ChooseAbility));
		ContextMenuEntities.Add(new ContextMenuCollectionEntity(UIStrings.Instance.ActionTexts.MoveItem, base.ViewModel.MoveAbility, !base.ViewModel.IsEmpty.CurrentValue));
		ContextMenuEntities.Add(new ContextMenuCollectionEntity(UIStrings.Instance.UIBugReport.ClearButtonText, base.ViewModel.ClearSlot, !base.ViewModel.IsEmpty.CurrentValue));
	}

	public void SetFocus(bool value)
	{
		m_SlotConsoleView.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_SlotConsoleView.IsValid();
	}

	public bool CanConfirmClick()
	{
		if (!m_SlotConsoleView.CanConfirmClick() && !base.ViewModel.MoveAbilityMode.CurrentValue)
		{
			return base.ViewModel.IsInCharScreen;
		}
		return true;
	}

	public void OnConfirmClick()
	{
		ReadOnlyReactiveProperty<bool> moveAbilityMode = base.ViewModel.MoveAbilityMode;
		if (moveAbilityMode != null && moveAbilityMode.CurrentValue)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.SetMoveAbilityMode(on: false);
			});
		}
		else if (base.ViewModel.IsInCharScreen)
		{
			SetSelectionActiveState(isActive: true);
			base.ViewModel.ChooseAbility();
		}
		else
		{
			m_SlotConsoleView.OnConfirmClick();
		}
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return m_SlotConsoleView.TooltipTemplate();
	}

	public void SetSelectionActiveState(bool isActive)
	{
		SetupButton();
	}
}
