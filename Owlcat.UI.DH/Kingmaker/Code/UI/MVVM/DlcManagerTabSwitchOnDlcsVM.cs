using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class DlcManagerTabSwitchOnDlcsVM : DlcManagerTabBaseVM, ISettingsDescriptionUIHandler, ISubscriber
{
	private readonly ReactiveProperty<DlcManagerSwitchOnDlcEntityVM> m_SelectedEntity = new ReactiveProperty<DlcManagerSwitchOnDlcEntityVM>();

	public SelectionGroupRadioVM<DlcManagerSwitchOnDlcEntityVM> SelectionGroup;

	public bool HaveDlcs;

	public readonly InfoSectionVM InfoVM;

	private readonly ReactiveProperty<TooltipBaseTemplate> m_ReactiveTooltipTemplate = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveCommand<bool> m_CheckModNeedToResaveCommand = new ReactiveCommand<bool>();

	private readonly ReactiveProperty<bool> m_NeedResave = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<DlcManagerSwitchOnDlcEntityVM> SelectedEntity => m_SelectedEntity;

	public Observable<bool> CheckModNeedToResaveCommand => m_CheckModNeedToResaveCommand;

	public ReadOnlyReactiveProperty<bool> NeedResave => m_NeedResave;

	public DlcManagerTabSwitchOnDlcsVM()
	{
		AddDlcs();
		InfoVM = new InfoSectionVM().AddTo(this);
		m_ReactiveTooltipTemplate.Subscribe(InfoVM.SetTemplate).AddTo(this);
		CheckModNeedToResaveCommand.Subscribe(delegate
		{
			m_NeedResave.Value = CheckNeedToResaveGame();
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	private void AddDlcs()
	{
		IEnumerable<IBlueprintDlc> availableAdditionalContentDlcForCurrentCampaign = Game.Instance.Player.GetAvailableAdditionalContentDlcForCurrentCampaign();
		IEnumerable<IBlueprintDlc> second = from dlc in StoreManager.GetPurchasableDLCs()
			where dlc.DlcType == DlcTypeEnum.CosmeticDlc && dlc.IsAvailable
			select dlc;
		List<DlcManagerSwitchOnDlcEntityVM> list = (from dlcEntity in (from dlc in availableAdditionalContentDlcForCurrentCampaign.Concat(second)
				orderby dlc.DlcType
				select dlc).ToList()
			select new DlcManagerSwitchOnDlcEntityVM(dlcEntity as BlueprintDlc, m_CheckModNeedToResaveCommand).AddTo(this)).ToList();
		HaveDlcs = list.Any();
		SelectionGroup = new SelectionGroupRadioVM<DlcManagerSwitchOnDlcEntityVM>(list, m_SelectedEntity).AddTo(this);
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

	public bool CheckNeedToResaveGame()
	{
		return SelectionGroup.EntitiesCollection.Any((DlcManagerSwitchOnDlcEntityVM e) => e.WarningResaveGame.CurrentValue);
	}

	public void SetDlcsCurrentState()
	{
		List<BlueprintDlc> dlcList = (from dlc in SelectionGroup.EntitiesCollection
			where dlc.WarningResaveGame.CurrentValue
			select dlc.BlueprintDlc).ToList();
		Game.Instance.Player.ApplySwitchOnDlc(dlcList);
	}

	public void ResetDlcsCurrentState()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(DlcManagerSwitchOnDlcEntityVM e)
		{
			e.ResetTempDlcState();
		});
	}
}
