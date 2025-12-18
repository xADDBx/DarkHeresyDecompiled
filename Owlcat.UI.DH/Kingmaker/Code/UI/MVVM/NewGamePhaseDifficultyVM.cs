using System;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class NewGamePhaseDifficultyVM : NewGamePhaseBaseVm, ISettingsDescriptionUIHandler, ISubscriber, IOptionsWindowUIHandler
{
	private readonly ObservableList<VirtualListElementVMBase> m_SettingEntities = new ObservableList<VirtualListElementVMBase>();

	public readonly InfoSectionVM InfoVM;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveProperty<bool> m_IsDefaultButtonInteractable = new ReactiveProperty<bool>();

	public ObservableList<VirtualListElementVMBase> SettingEntities => m_SettingEntities;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> ReactiveTooltipTemplate => m_ReactiveTooltipTemplate;

	public ReadOnlyReactiveProperty<bool> IsDefaultButtonInteractable => m_IsDefaultButtonInteractable;

	public NewGamePhaseDifficultyVM(Action backStep, Action nextStep)
		: base(backStep, nextStep)
	{
		SwitchSettingsScreen(UISettingsManager.SettingsScreen.Difficulty);
		foreach (VirtualListElementVMBase settingEntity in m_SettingEntities)
		{
			settingEntity.AddTo(this);
		}
		m_IsDefaultButtonInteractable.Value = CheckDefaultButton();
		InfoVM = new InfoSectionVM().AddTo(this);
		ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		HandleHideSettingsDescription();
	}

	private void SwitchSettingsScreen(UISettingsManager.SettingsScreen settingsScreen)
	{
		m_SettingEntities.Clear();
		foreach (UISettingsGroup settings in Game.Instance.UISettingsManager.GetSettingsList(settingsScreen))
		{
			if (!settings.IsVisible)
			{
				continue;
			}
			m_SettingEntities.Add(new SettingsEntityHeaderVM(settings.Title));
			foreach (UISettingsEntityBase visibleSettings in settings.VisibleSettingsList)
			{
				m_SettingEntities.Add(SettingsVM.GetVMForSettingsItem(visibleSettings, isNewGame: true));
			}
		}
		foreach (UISettingsGroup settings2 in Game.Instance.UISettingsManager.GetSettingsList(settingsScreen))
		{
			if (!settings2.IsVisible)
			{
				continue;
			}
			{
				foreach (UISettingsEntityBase visibleSettings2 in settings2.VisibleSettingsList)
				{
					if (!(visibleSettings2 is UISettingsEntityDisplayImages) && !(visibleSettings2 is UISettingsEntityAccessiabilityImage))
					{
						HandleShowSettingsDescription(visibleSettings2);
						break;
					}
				}
				break;
			}
		}
	}

	public void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		SetupTooltipTemplate(entity, ownTitle, ownDescription);
	}

	public void HandleHideSettingsDescription()
	{
	}

	private void SetupTooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		m_ReactiveTooltipTemplate.Value = ((entity != null || ownTitle != null || ownDescription != null) ? TooltipTemplate(entity, ownTitle, ownDescription) : null);
	}

	private TooltipBaseTemplate TooltipTemplate(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		return new TooltipTemplateSettingsEntityDescription(entity, ownTitle, ownDescription);
	}

	public void OpenDefaultSettingsDialog()
	{
		string text = string.Format(ConfigRoot.Instance.LocalizedTexts.UserInterfacesText.SettingsUI.RestoreAllDefaultsMessage, UIStrings.Instance.NewGameWin.DifficultyMenuLabel.Text);
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler w)
		{
			w.HandleOpen(text, DialogMessageBoxType.Dialog, OnDefaultDialogAnswer);
		});
	}

	private void OnDefaultDialogAnswer(DialogMessageBoxButton button)
	{
		if (button == DialogMessageBoxButton.Yes)
		{
			SettingsRoot.Difficulty.GameDifficulty.SetValueAndConfirm(GameDifficultyOption.Normal);
		}
	}

	public void HandleItemChanged(string key)
	{
		m_IsDefaultButtonInteractable.Value = CheckDefaultButton();
	}

	private bool CheckDefaultButton()
	{
		return (from @group in Game.Instance.UISettingsManager.GetSettingsList(UISettingsManager.SettingsScreen.Difficulty)
			where @group.IsVisible
			select @group).Any((UISettingsGroup group) => group.VisibleSettingsList.Where((UISettingsEntityBase setting) => !setting.NoDefaultReset).Any((UISettingsEntityBase setting) => setting is IUISettingsEntityWithValueBase iUISettingsEntityWithValueBase && iUISettingsEntityWithValueBase.SettingsEntity.CurrentValueIsNotDefault()));
	}
}
