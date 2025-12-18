using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyInvitePlayerDifferentPlatformsPCView : NetLobbyInvitePlayerDifferentPlatformsBaseView
{
	[SerializeField]
	private OwlcatButton m_InviteFromSteamButton;

	[SerializeField]
	private TextMeshProUGUI m_InviteFromSteamLabel;

	[SerializeField]
	private OwlcatButton m_InviteFromEpicGamesButton;

	[SerializeField]
	private TextMeshProUGUI m_InviteFromEpicGamesLabel;

	[SerializeField]
	private OwlcatButton m_CancelInviteButton;

	[SerializeField]
	private TextMeshProUGUI m_CancelInviteLabel;

	[SerializeField]
	private OwlcatButton m_CloseButton;

	public override void Initialize()
	{
		base.Initialize();
		m_InviteFromSteamLabel.text = UIStrings.Instance.NetLobbyTexts.InvitePlayer;
		m_InviteFromEpicGamesLabel.text = UIStrings.Instance.NetLobbyTexts.InviteEpicGamesPlayer;
		m_CancelInviteLabel.text = UIStrings.Instance.CommonTexts.Cancel;
	}

	protected override void OnBind()
	{
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_InviteFromSteamButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnInvitePlayer();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_InviteFromEpicGamesButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnInviteEpicGamesPlayer();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CancelInviteButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OnClose();
		}).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(base.ViewModel.OnClose).AddTo(this);
	}
}
