using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using R3.Triggers;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyLobbyPartPCView : NetLobbyLobbyPartBaseView
{
	[Header("PC Part")]
	[Space]
	[SerializeField]
	private OwlcatButton m_DisconnectButton;

	[SerializeField]
	private TextMeshProUGUI m_DisconnectButtonText;

	[SerializeField]
	private OwlcatButton m_DlcListButton;

	[SerializeField]
	private TextMeshProUGUI m_DlcListButtonButtonText;

	[SerializeField]
	private OwlcatButton m_LaunchButton;

	[SerializeField]
	private TextMeshProUGUI m_LaunchButtonText;

	[SerializeField]
	private List<NetLobbyPlayerPCView> m_PlayerList;

	[SerializeField]
	private OwlcatButton m_LobbyIdCopyButton;

	[SerializeField]
	private TextMeshProUGUI m_LobbyIdCopyButtonText;

	[SerializeField]
	private OwlcatButton m_LobbyIdShowHideButton;

	[SerializeField]
	private OwlcatButton m_ResetCurrentSave;

	[SerializeField]
	private OwlcatButton m_SaveListBackButton;

	[SerializeField]
	private TextMeshProUGUI m_SaveListBackButtonText;

	[SerializeField]
	private NetLobbyInvitePlayerDifferentPlatformsPCView m_DifferentPlatformsInvitePCView;

	[SerializeField]
	private NetLobbyDlcListPCView m_DlcListPCView;

	public override void Initialize()
	{
		base.Initialize();
		m_LobbyIdCopyButtonText.text = UIStrings.Instance.NetLobbyTexts.CopyLobbyId;
		m_DisconnectButtonText.text = UIStrings.Instance.NetLobbyTexts.DisconnectLobby;
		m_DlcListButtonButtonText.text = UIStrings.Instance.NetLobbyTexts.DlcList;
		m_SaveListBackButtonText.text = UIStrings.Instance.CommonTexts.Back;
		m_DifferentPlatformsInvitePCView.Initialize();
		m_DlcListPCView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.IsInRoom.CombineLatest(base.ViewModel.NetGameCurrentState, base.ViewModel.SaveSlotCollectionVm, base.ViewModel.IsHost, (bool inRoom, NetGame.State state, SaveSlotCollectionVM collection, bool host) => new { inRoom, state, collection, host }).Subscribe(value =>
		{
			m_LaunchButton.gameObject.SetActive(value.host);
		}).AddTo(this);
		LaunchButtonText.Subscribe(delegate(string value)
		{
			UISounds.Instance.SetClickSound(m_LaunchButton, IsLaunchSound ? ButtonSoundsEnum.NoSound : ButtonSoundsEnum.NormalSound);
			m_LaunchButtonText.text = value;
		}).AddTo(this);
		base.ViewModel.IsSaveTransfer.Subscribe(delegate(bool value)
		{
			m_LaunchButton.SetInteractable(!value && LaunchButtonActive.Value);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_DisconnectButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Disconnect("DisconnectButton");
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_DlcListButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ShowDlcList();
		}).AddTo(this);
		LaunchButtonInteractable.Subscribe(m_LaunchButton.SetInteractable).AddTo(this);
		m_LaunchButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			if (!LaunchButtonInteractable.Value)
			{
				base.ViewModel.Launch();
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_LaunchButton.OnLeftClickAsObservable(), delegate
		{
			m_LaunchButton.SetInteractable(!base.ViewModel.Launch());
		}).AddTo(this);
		for (int i = 0; i < m_PlayerList.Count; i++)
		{
			m_PlayerList[i].Bind(base.ViewModel.PlayerVms[i]);
		}
		ObservableSubscribeExtensions.Subscribe(m_LobbyIdCopyButton.OnLeftClickAsObservable(), delegate
		{
			CopyLobbyId();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_LobbyIdShowHideButton.OnLeftClickAsObservable(), delegate
		{
			ShowHideLobbyId();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_EmptySaveSlotButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ChooseSave();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ResetCurrentSave.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ResetCurrentSave();
		}).AddTo(this);
		ResetCurrentSaveActive.Subscribe(m_ResetCurrentSave.gameObject.SetActive).AddTo(this);
		m_ResetCurrentSave.SetHint(UIStrings.Instance.NetLobbyTexts.ResetCurrentSave).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SaveListBackButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_ConnectEpicGamesToSteam.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenEpicGamesLayer();
		}).AddTo(this);
		m_LobbyIdShowHideButton.SetHint(UIStrings.Instance.NetLobbyTexts.ShowLobbyCode).AddTo(this);
		base.ViewModel.DifferentPlatformInviteVM.Subscribe(m_DifferentPlatformsInvitePCView.Bind).AddTo(this);
		base.ViewModel.DlcListVM.Subscribe(m_DlcListPCView.Bind).AddTo(this);
	}

	private void TrySetHints()
	{
		if (!base.ViewModel.IsSaveAllowed.CurrentValue)
		{
			m_LaunchButton.SetHint(UIStrings.Instance.NetLobbyTexts.ImpossibleToStartCoopGameInThisMoment).AddTo(this);
			m_EmptySaveSlotButton.SetHint(UIStrings.Instance.NetLobbyTexts.ImpossibleToStartCoopGameInThisMoment).AddTo(this);
		}
	}
}
