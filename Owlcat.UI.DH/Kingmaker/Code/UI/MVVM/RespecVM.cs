using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class RespecVM : ViewModel
{
	private readonly ReactiveProperty<int> m_RespecCost = new ReactiveProperty<int>();

	private readonly ReactiveProperty<bool> m_CanRespec = new ReactiveProperty<bool>();

	public readonly SelectionGroupRadioVM<RespecCharacterVM> CharacterSelectionGroupRadioVM;

	private readonly ReactiveProperty<RespecCharacterVM> m_CurrentCharacterVM = new ReactiveProperty<RespecCharacterVM>();

	private readonly List<RespecCharacterVM> m_Characters = new List<RespecCharacterVM>();

	private readonly Action<BaseUnitEntity> m_SuccessAction;

	private readonly Action m_CloseAction;

	public ReadOnlyReactiveProperty<int> RespecCost => m_RespecCost;

	public ReadOnlyReactiveProperty<bool> CanRespec => m_CanRespec;

	public ReadOnlyReactiveProperty<RespecCharacterVM> CurrentCharacterVM => m_CurrentCharacterVM;

	public RespecVM(List<BaseUnitEntity> characters, Action<BaseUnitEntity> successAction, Action closeAction)
	{
		m_SuccessAction = successAction;
		m_CloseAction = closeAction;
		foreach (BaseUnitEntity character in characters)
		{
			m_Characters.Add(new RespecCharacterVM(character).AddTo(this));
		}
		CharacterSelectionGroupRadioVM = new SelectionGroupRadioVM<RespecCharacterVM>(m_Characters, m_CurrentCharacterVM).AddTo(this);
		CharacterSelectionGroupRadioVM.TrySelectFirstValidEntity();
		CurrentCharacterVM.Subscribe(delegate(RespecCharacterVM ch)
		{
			m_RespecCost.Value = ch.Unit.Progression.GetRespecCost();
		}).AddTo(this);
		RespecCost.Subscribe(delegate
		{
			m_CanRespec.Value = true;
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void OnConfirm()
	{
		BaseUnitEntity unit = CurrentCharacterVM.CurrentValue?.Unit;
		if (unit == null)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(UIStrings.Instance.CharGen.RespecSelectCharacter, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes)
				{
					m_SuccessAction(unit);
					ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
					{
						OnClose();
						OnAfterSuccess();
					});
				}
			});
		});
	}

	public void OnClose()
	{
		m_CloseAction();
	}

	private void OnAfterSuccess()
	{
		Game.Instance.Player.UISettings.SavedUnitProgressionWindowData.CareerPath = null;
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenCharacterInfo(CharInfoPageType.LevelProgression, CurrentCharacterVM.CurrentValue?.Unit);
		});
		Metrics.Interface.Type(InterfaceMetricsEvent.InterfaceTypes.Respec).State(InterfaceMetricsEvent.InterfaceStates.Open).Send();
	}
}
