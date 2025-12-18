using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Localization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class NewGameVM : ViewModel
{
	public readonly SelectionGroupRadioVM<NewGameMenuEntityVM> MenuSelectionGroup;

	private readonly ReactiveProperty<NewGameMenuEntityVM> m_SelectedMenuEntity = new ReactiveProperty<NewGameMenuEntityVM>();

	private readonly List<NewGameMenuEntityVM> m_MenuEntitiesList = new List<NewGameMenuEntityVM>();

	private readonly ReactiveCommand<Unit> m_ChangeTab = new ReactiveCommand<Unit>();

	private Action m_CloseCallback;

	private Action m_FinishCallback;

	public ReadOnlyReactiveProperty<NewGameMenuEntityVM> SelectedMenuEntity => m_SelectedMenuEntity;

	public NewGamePhasePresetVM PresetVM { get; }

	public NewGamePhaseDifficultyVM DifficultyVM { get; }

	public Observable<Unit> ChangeTab => m_ChangeTab;

	public NewGameVM(Action closeCallback, Action finishCallback)
	{
		m_CloseCallback = closeCallback;
		m_FinishCallback = finishCallback;
		PresetVM = new NewGamePhasePresetVM(m_CloseCallback, delegate
		{
			GoToPhase(DifficultyVM);
		}).AddTo(this);
		DifficultyVM = new NewGamePhaseDifficultyVM(delegate
		{
			SettingsController.Instance.ConfirmAllTempValues();
			SettingsController.Instance.SaveAll();
			GoToPhase(PresetVM);
		}, DifficultyNexStep).AddTo(this);
		CreateMenuEntity(UIStrings.Instance.NewGameWin.ScenarioMenuLabel, PresetVM, OnPresetMenuSelect);
		CreateMenuEntity(UIStrings.Instance.NewGameWin.DifficultyMenuLabel, DifficultyVM, OnDifficultyMenuSelect);
		MenuSelectionGroup = new SelectionGroupRadioVM<NewGameMenuEntityVM>(m_MenuEntitiesList, m_SelectedMenuEntity);
		m_SelectedMenuEntity.Value = m_MenuEntitiesList.First();
		MenuSelectionGroup.AddTo(this);
	}

	private void GoToPhase(NewGamePhaseBaseVm phase)
	{
		m_SelectedMenuEntity.Value = m_MenuEntitiesList.Find((NewGameMenuEntityVM e) => e.NewGamePhaseVM == phase);
	}

	private void DifficultyNexStep()
	{
		if (SettingsRoot.Difficulty.GameDifficulty.GetTempValue() >= GameDifficultyOption.Core)
		{
			EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
			{
				h.HandleOpen(UIStrings.Instance.NewGameWin.AreYouSureChooseVeryHardDifficulty, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton buttonPressed)
				{
					if (buttonPressed == DialogMessageBoxButton.Yes)
					{
						m_FinishCallback();
					}
				});
			});
		}
		else
		{
			m_FinishCallback();
		}
	}

	private void OnNextMenuEntityAvailable(bool value)
	{
		for (int i = MenuSelectionGroup.EntitiesCollection.IndexOf(MenuSelectionGroup.SelectedEntity.Value) + 1; i < MenuSelectionGroup.EntitiesCollection.Count; i++)
		{
			MenuSelectionGroup.EntitiesCollection[i].SetAvailable(value);
		}
	}

	private void CreateMenuEntity(LocalizedString localizedString, NewGamePhaseBaseVm newGamePhaseVM, Action callback)
	{
		NewGameMenuEntityVM newGameMenuEntityVM = new NewGameMenuEntityVM(localizedString, newGamePhaseVM, callback);
		newGameMenuEntityVM.AddTo(this);
		m_MenuEntitiesList.Add(newGameMenuEntityVM);
	}

	private void OnPresetMenuSelect()
	{
		DifficultyVM.SetEnabled(value: false);
		PresetVM.SetEnabled(value: true);
		m_ChangeTab.Execute();
	}

	private void OnStoryMenuSelect()
	{
		DifficultyVM.SetEnabled(value: false);
		m_ChangeTab.Execute();
	}

	private void OnDifficultyMenuSelect()
	{
		DifficultyVM.SetEnabled(value: true);
		m_ChangeTab.Execute();
	}

	public void OnStart()
	{
	}

	public void OnButtonBack()
	{
		MenuSelectionGroup.SelectedEntity.Value.OnBack();
	}

	public void OnButtonNext()
	{
		MenuSelectionGroup.SelectedEntity.Value.OnNext();
	}

	public void OnClose()
	{
		m_CloseCallback();
	}
}
