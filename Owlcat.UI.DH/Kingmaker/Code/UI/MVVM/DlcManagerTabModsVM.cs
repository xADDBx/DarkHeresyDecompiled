using System.Collections.Generic;
using System.Linq;
using Code.GameCore.Modding;
using Code.Utility.ExtendedModInfo;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabModsVM : DlcManagerTabBaseVM, ISettingsDescriptionUIHandler, ISubscriber
{
	private readonly ReactiveProperty<DlcManagerModEntityVM> m_SelectedEntity = new ReactiveProperty<DlcManagerModEntityVM>();

	public SelectionGroupRadioVM<DlcManagerModEntityVM> SelectionGroup;

	public bool HaveMods;

	public readonly InfoSectionVM InfoVM;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly bool m_IsMainMenu;

	private readonly ReactiveProperty<bool> m_IsSteam = new ReactiveProperty<bool>();

	private readonly ReactiveCommand<bool> m_CheckModNeedToReloadCommand = new ReactiveCommand<bool>();

	private readonly ReactiveProperty<bool> m_NeedReload = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<DlcManagerModEntityVM> SelectedEntity => m_SelectedEntity;

	public ReadOnlyReactiveProperty<bool> IsSteam => m_IsSteam;

	public Observable<bool> CheckModNeedToReloadCommand => m_CheckModNeedToReloadCommand;

	public ReadOnlyReactiveProperty<bool> NeedReload => m_NeedReload;

	public DlcManagerTabModsVM(bool isMainMenu)
	{
		m_IsMainMenu = isMainMenu;
		m_IsSteam.Value = StoreManager.Store == StoreType.Steam;
		AddMods();
		InfoVM = new InfoSectionVM().AddTo(this);
		m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate).AddTo(this);
		CheckModNeedToReloadCommand.Subscribe(delegate
		{
			m_NeedReload.Value = CheckNeedToReloadGame();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private void AddMods()
	{
		ModInitializer.CheckForModUpdates();
		List<DlcManagerModEntityVM> list = (from modEntity in (from m in ModInitializer.GetAllModsInfo()
				orderby m.Id
				select m).ToArray()
			select new DlcManagerModEntityVM(modEntity, m_IsMainMenu, m_CheckModNeedToReloadCommand).AddTo(this)).ToList();
		HaveMods = list.Any();
		SelectionGroup = new SelectionGroupRadioVM<DlcManagerModEntityVM>(list, m_SelectedEntity).AddTo(this);
		m_SelectedEntity.Value = list.FirstOrDefault();
	}

	public void HandleShowSettingsDescription(IUISettingsEntityBase entity, string ownTitle = null, string ownDescription = null)
	{
		SetupTooltipTemplate(ownTitle, ownDescription);
	}

	public void HandleHideSettingsDescription()
	{
	}

	private void SetupTooltipTemplate(string ownTitle = null, string ownDescription = null)
	{
		m_ReactiveTooltipTemplate.Value = ((ownTitle != null || ownDescription != null) ? TooltipTemplate(ownTitle, ownDescription) : null);
	}

	private TooltipBaseTemplate TooltipTemplate(string ownTitle = null, string ownDescription = null)
	{
		return new TooltipTemplateSettingsEntityDescription(null, ownTitle, ownDescription);
	}

	public bool CheckNeedToReloadGame()
	{
		return SelectionGroup.EntitiesCollection.Any((DlcManagerModEntityVM e) => e.WarningReloadGame.CurrentValue);
	}

	public void SetModsCurrentState()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(DlcManagerModEntityVM e)
		{
			e.SetActualModState();
		});
	}

	public void ResetModsCurrentState()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(DlcManagerModEntityVM e)
		{
			e.ResetTempModState();
		});
	}

	public void OpenNexusMods()
	{
		Application.OpenURL("https://www.nexusmods.com/warhammer40kroguetrader");
	}

	public void OpenSteamWorkshop()
	{
		Application.OpenURL("https://steamcommunity.com/app/2186680/workshop/");
	}
}
