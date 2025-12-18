using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.Components.Text.ScrambledTextMeshPro;
using Kingmaker.DLC;
using Kingmaker.Networking;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.Stores.DlcInterfaces;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyLobbyPartBaseView : View<NetLobbyVM>
{
	[Header("Lobby Part")]
	[SerializeField]
	private TextMeshProUGUI m_LobbyIdText;

	[SerializeField]
	private TextMeshProUGUI m_LobbyIdHintText;

	[SerializeField]
	private CanvasGroup m_ShowHideIcon;

	[SerializeField]
	private ScrambledTMP m_ScrambledLobbyCode;

	[SerializeField]
	private RectTransform m_LobbyIdTooltipPlace;

	[SerializeField]
	private TextMeshProUGUI m_LobbyNotEnoughPlayersText;

	[SerializeField]
	protected OwlcatButton m_ConnectEpicGamesToSteam;

	[SerializeField]
	private TextMeshProUGUI m_EpicGamesUserName;

	[Header("Region Part")]
	[SerializeField]
	private TextMeshProUGUI m_CurrentRegionText;

	[SerializeField]
	private TextMeshProUGUI m_CurrentVersion;

	[SerializeField]
	private TextMeshProUGUI m_CurrentVersionHeader;

	[SerializeField]
	private RectTransform m_RegionTooltipPlace;

	[Header("Save Part")]
	[SerializeField]
	private TextMeshProUGUI m_SaveBlockHeader;

	[SerializeField]
	private TextMeshProUGUI m_LaunchInGameHintText;

	[SerializeField]
	private TextMeshProUGUI m_SaveBlockHintHeader;

	[SerializeField]
	protected OwlcatButton m_EmptySaveSlotButton;

	[SerializeField]
	private SaveSlotBaseView m_SaveSlot;

	[Space]
	[SerializeField]
	private GameObject m_TransferSavePart;

	[SerializeField]
	private Image m_TransferSaveProgress;

	[SerializeField]
	private TextMeshProUGUI m_TransferSaveProgressPercent;

	[SerializeField]
	private TextMeshProUGUI m_TransferSaveProgressSize;

	[SerializeField]
	private TextMeshProUGUI m_WillShowNotAllSavesBecauseOfDlcLabel;

	[Header("Save List Part")]
	[SerializeField]
	private GameObject m_WaitingForSaveList;

	[SerializeField]
	private TextMeshProUGUI m_EmptyListHint;

	[SerializeField]
	private TextMeshProUGUI m_EmptyListHintBecauseDlcs;

	[Header("Save List Part")]
	[SerializeField]
	private Image m_CanBeAProblemsWithModsAttentionMark;

	[SerializeField]
	private TextMeshProUGUI m_CanBeAProblemsWithModsText;

	[SerializeField]
	private SaveFullScreenshotBaseView m_FullScreenshotBaseView;

	protected readonly ReactiveProperty<bool> ShowCode = new ReactiveProperty<bool>();

	private const string HideLobbyCodeString = "**********";

	protected readonly ReactiveProperty<bool> IsInLobbyPart = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> ResetCurrentSaveActive = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<string> LaunchButtonText = new ReactiveProperty<string>();

	protected readonly ReactiveProperty<bool> LaunchButtonActive = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> LaunchButtonInteractable = new ReactiveProperty<bool>();

	private IDisposable m_EpicGamesButtonHintDisposable;

	private IDisposable m_ProblemsWithModsDisposable;

	protected bool IsLaunchSound;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_LobbyIdHintText.text = UIStrings.Instance.NetLobbyTexts.CopyLobbyIdHint;
		m_SaveBlockHintHeader.text = UIStrings.Instance.NetLobbyTexts.ChooseSaveHint;
		m_WillShowNotAllSavesBecauseOfDlcLabel.text = UIStrings.Instance.NetLobbyTexts.WillShowNotAllSavesBecauseOfDlc;
		m_EmptyListHint.text = UIStrings.Instance.SaveLoadTexts.EmptySaveListHint.Text + Environment.NewLine + Environment.NewLine + UIStrings.Instance.NetLobbyTexts.NeedSaveForStartGame.Text;
		m_CurrentVersionHeader.text = UIStrings.Instance.NetLobbyTexts.CoopVer;
		m_LobbyNotEnoughPlayersText.text = UIStrings.Instance.NetLobbyTexts.IsNotEnoughPlayersForGame;
		if (m_CanBeAProblemsWithModsText != null)
		{
			m_CanBeAProblemsWithModsText.text = UIStrings.Instance.NetLobbyTexts.CanBeAProblemsWithMods;
		}
	}

	protected override void OnBind()
	{
		LaunchButtonInteractable.Value = (base.ViewModel.CurrentSave.CurrentValue?.Reference == null && !PhotonManager.Sync.HasDesync && base.ViewModel.IsSaveAllowed.CurrentValue) || (base.ViewModel.CurrentSave.CurrentValue?.Reference != null && !base.ViewModel.IsMainMenu);
		m_SaveBlockHeader.text = UIStrings.Instance.NetLobbyTexts.ChooseSaveHeader;
		base.ViewModel.IsInRoom.CombineLatest(base.ViewModel.NetGameCurrentState, base.ViewModel.SaveSlotCollectionVm, base.ViewModel.IsHost, (bool inRoom, NetGame.State state, SaveSlotCollectionVM collection, bool host) => new { inRoom, state, collection, host }).Subscribe(value =>
		{
			bool isConnectingNetGameCurrentState = base.ViewModel.IsConnectingNetGameCurrentState;
			IsInLobbyPart.Value = value.inRoom && !isConnectingNetGameCurrentState && value.collection == null;
			base.gameObject.SetActive(value.inRoom && !isConnectingNetGameCurrentState && value.collection == null);
		}).AddTo(this);
		m_ShowHideIcon.alpha = (ShowCode.Value ? 1f : 0.25f);
		ShowCode.Skip(1).Subscribe(delegate(bool value)
		{
			string startText = (value ? "**********" : base.ViewModel.LobbyCode.CurrentValue);
			string endText = (value ? base.ViewModel.LobbyCode.CurrentValue : "**********");
			m_ScrambledLobbyCode.SetText(startText, endText);
			m_ShowHideIcon.alpha = (value ? 1f : 0.25f);
		}).AddTo(this);
		base.ViewModel.IsEnoughPlayersForGame.Subscribe(delegate(bool value)
		{
			m_LobbyNotEnoughPlayersText.gameObject.SetActive(!value);
		}).AddTo(this);
		base.ViewModel.LobbyCode.Subscribe(delegate(string value)
		{
			m_LobbyIdText.text = (ShowCode.Value ? value : "**********");
		}).AddTo(this);
		base.ViewModel.IsSaveAllowed.CombineLatest(base.ViewModel.IsHost, base.ViewModel.NeedReconnect, base.ViewModel.IsEnoughPlayersForGame, (bool isSaveAllowed, bool isHost, bool reconnect, bool enoughPlayers) => new { isSaveAllowed, isHost, reconnect, enoughPlayers }).Subscribe(value =>
		{
			m_LaunchInGameHintText.gameObject.SetActive(value.isSaveAllowed && value.isHost && !base.ViewModel.IsMainMenu);
			string arg3 = (value.reconnect ? UIStrings.Instance.NetLobbyTexts.Reconnect : UIStrings.Instance.NetLobbyTexts.Launch);
			m_LaunchInGameHintText.text = string.Format(UIStrings.Instance.NetLobbyTexts.LaunchInGameHint, arg3);
			LaunchButtonInteractable.Value = (base.ViewModel.CurrentSave.CurrentValue?.Reference == null && !PhotonManager.Sync.HasDesync) || (!value.reconnect && value.enoughPlayers && value.isSaveAllowed) || base.ViewModel.CurrentSave.CurrentValue?.Reference != null;
			LaunchButtonInteractable.ForceNotify();
		}).AddTo(this);
		base.ViewModel.CurrentRegion.Subscribe(delegate(string value)
		{
			m_CurrentRegionText.text = value;
		}).AddTo(this);
		base.ViewModel.Version.Subscribe(delegate(string value)
		{
			m_CurrentVersion.text = value;
		}).AddTo(this);
		base.ViewModel.IsSaveTransfer.Subscribe(delegate(bool value)
		{
			m_TransferSavePart.SetActive(value);
			m_SaveBlockHintHeader.gameObject.SetActive(!value && !base.ViewModel.IsHost.CurrentValue);
		}).AddTo(this);
		base.ViewModel.CurrentSave.CombineLatest(base.ViewModel.IsHost, (SaveSlotVM save, bool isHost) => new { save, isHost }).Subscribe(value =>
		{
			m_SaveSlot.Bind(value.save);
			m_SaveBlockHintHeader.gameObject.SetActive(!value.isHost && value.save == null);
			m_EmptySaveSlotButton.gameObject.SetActive(value.save == null && value.isHost);
			ResetCurrentSaveActive.Value = value.save != null && value.isHost;
		}).AddTo(this);
		base.ViewModel.SaveTransferProgress.CombineLatest(base.ViewModel.SaveTransferTarget, (int progress, int target) => new { progress, target }).Subscribe(transfer =>
		{
			if (transfer.target == 0 || !base.ViewModel.IsSaveTransfer.CurrentValue)
			{
				m_TransferSaveProgressSize.text = string.Empty;
				m_TransferSaveProgressPercent.text = string.Empty;
				m_TransferSaveProgress.fillAmount = 0f;
			}
			else
			{
				m_TransferSaveProgress.fillAmount = (float)transfer.progress / (float)transfer.target;
				m_TransferSaveProgressSize.text = $"{transfer.progress / 1024}/{transfer.target / 1024} KB";
				m_TransferSaveProgressPercent.text = $"{100f * ((float)transfer.progress / (float)transfer.target):00}%";
			}
		}).AddTo(this);
		base.ViewModel.NeedReconnect.CombineLatest(base.ViewModel.IsSaveAllowed, base.ViewModel.CurrentSave, base.ViewModel.IsEnoughPlayersForGame, (bool reconnect, bool saveAllowed, SaveSlotVM save, bool enoughPlayers) => new { reconnect, saveAllowed, save, enoughPlayers }).Subscribe(value =>
		{
			LaunchButtonActive.Value = value.save != null || (!value.reconnect && value.saveAllowed && !base.ViewModel.IsMainMenu);
			if (value.save != null)
			{
				IsLaunchSound = true;
				LaunchButtonText.Value = UIStrings.Instance.NetLobbyTexts.Launch;
				LaunchButtonInteractable.Value = value.enoughPlayers;
				LaunchButtonInteractable.ForceNotify();
			}
			else if (value.reconnect)
			{
				IsLaunchSound = true;
				LaunchButtonText.Value = UIStrings.Instance.NetLobbyTexts.Reconnect;
				LaunchButtonInteractable.Value = value.saveAllowed;
				LaunchButtonInteractable.ForceNotify();
			}
			else
			{
				IsLaunchSound = value.saveAllowed && !base.ViewModel.IsMainMenu;
				LaunchButtonText.Value = ((value.saveAllowed && !base.ViewModel.IsMainMenu) ? UIStrings.Instance.NetLobbyTexts.Launch : UIStrings.Instance.NetLobbyTexts.ChooseSaveHeader);
				LaunchButtonInteractable.Value = (base.ViewModel.CurrentSave.CurrentValue?.Reference == null && !PhotonManager.Sync.HasDesync) || (value.enoughPlayers && value.saveAllowed);
				LaunchButtonInteractable.ForceNotify();
			}
		}).AddTo(this);
		base.ViewModel.ShowWaitingSaveAnim.Subscribe(m_WaitingForSaveList.SetActive).AddTo(this);
		base.ViewModel.SaveListAreEmpty.Subscribe(delegate(bool value)
		{
			m_EmptyListHint.gameObject.SetActive(value && !base.ViewModel.ProblemsToShowInSaveList.Any());
			m_EmptyListHintBecauseDlcs.transform.parent.gameObject.SetActive(value: false);
			if (base.ViewModel.ProblemsToShowInSaveList.Any() && value)
			{
				IEnumerable<string> values = (from dlc in base.ViewModel.ProblemsToShowInSaveList.Values.SelectMany((List<IBlueprintDlc> dlcs) => dlcs)
					orderby dlc.DlcType
					select dlc).Select(delegate(IBlueprintDlc playerDLC)
				{
					if (!(playerDLC is BlueprintDlc blueprintDlc))
					{
						return (string)null;
					}
					return string.IsNullOrEmpty(blueprintDlc.GetDlcName()) ? blueprintDlc.name : blueprintDlc.GetDlcName();
				});
				string arg = string.Join(", ", values);
				string arg2 = string.Join(", ", base.ViewModel.ProblemsToShowInSaveList.Keys.ToList());
				m_EmptyListHintBecauseDlcs.text = string.Format(UIStrings.Instance.NetLobbyTexts.CantChooseAnySavesBecauseOfDlc, arg, arg2);
				m_EmptyListHintBecauseDlcs.transform.parent.gameObject.SetActive(value: true);
			}
		}).AddTo(this);
		base.ViewModel.CheckProblemsWithDlcs.Subscribe(delegate(Dictionary<string, List<IBlueprintDlc>> value)
		{
			m_WillShowNotAllSavesBecauseOfDlcLabel.transform.parent.gameObject.SetActive(value.Count > 0);
		}).AddTo(this);
		m_CurrentVersion.SetHint(UIStrings.Instance.NetLobbyTexts.CoopVerTooltip).AddTo(this);
		m_CurrentRegionText.SetHint(UIStrings.Instance.NetLobbyTexts.CoopRegionTooltip).AddTo(this);
		m_LobbyIdText.SetHint(UIStrings.Instance.NetLobbyTexts.CoopLobbyCodeTooltip).AddTo(this);
		base.ViewModel.EpicGamesButtonActive.CombineLatest(base.ViewModel.EpicGamesUserName, base.ViewModel.EpicGamesAuthorized, (bool buttonsActive, string userName, bool isAuthorized) => new { buttonsActive, userName, isAuthorized }).Subscribe(value =>
		{
			m_ConnectEpicGamesToSteam.gameObject.SetActive(value.buttonsActive);
			bool flag2 = string.IsNullOrWhiteSpace(value.userName);
			m_EpicGamesButtonHintDisposable?.Dispose();
			m_EpicGamesButtonHintDisposable = m_ConnectEpicGamesToSteam.SetHint((flag2 || !value.isAuthorized) ? ((string)UIStrings.Instance.NetLobbyTexts.SignInToEpicGamesStore) : value.userName);
			m_EpicGamesUserName.gameObject.SetActive(!flag2 && value.isAuthorized && value.buttonsActive);
			if (!flag2 && value.isAuthorized)
			{
				m_EpicGamesUserName.text = value.userName;
			}
		}).AddTo(this);
		base.ViewModel.PlayersDifferentMods.Subscribe(delegate(string value)
		{
			if (m_CanBeAProblemsWithModsText == null || m_CanBeAProblemsWithModsAttentionMark == null)
			{
				m_ProblemsWithModsDisposable?.Dispose();
				m_ProblemsWithModsDisposable = null;
			}
			else
			{
				bool flag = !string.IsNullOrWhiteSpace(value);
				m_CanBeAProblemsWithModsText.transform.parent.gameObject.SetActive(flag);
				m_ProblemsWithModsDisposable?.Dispose();
				m_ProblemsWithModsDisposable = null;
				if (flag)
				{
					m_ProblemsWithModsDisposable = m_CanBeAProblemsWithModsAttentionMark.Or(null)?.SetHint(value);
				}
			}
		}).AddTo(this);
		base.ViewModel.SaveFullScreenshot.Subscribe(m_FullScreenshotBaseView.Bind).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_EpicGamesButtonHintDisposable?.Dispose();
		m_EpicGamesButtonHintDisposable = null;
		m_ProblemsWithModsDisposable?.Dispose();
		m_ProblemsWithModsDisposable = null;
	}

	protected void CopyLobbyId()
	{
		base.ViewModel.CopyLobbyId();
		CanvasGroup target = m_LobbyIdHintText.EnsureComponent<CanvasGroup>();
		Sequence sequence = DOTween.Sequence();
		sequence.Append(target.DOFade(1f, 0.2f));
		sequence.Append(target.DOFade(5f, 0.2f));
		sequence.Append(target.DOFade(0f, 0.2f));
		sequence.Play();
	}

	protected void ShowHideLobbyId()
	{
		ShowCode.Value = !ShowCode.Value;
	}
}
