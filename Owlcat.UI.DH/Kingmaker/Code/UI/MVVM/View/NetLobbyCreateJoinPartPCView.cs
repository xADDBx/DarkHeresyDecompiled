using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyCreateJoinPartPCView : NetLobbyCreateJoinPartBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private OwlcatButton m_CreateLobbyButton;

	[SerializeField]
	private TextMeshProUGUI m_CreateLobbyButtonText;

	[Space]
	[SerializeField]
	private OwlcatButton m_JoinLobbyButton;

	[SerializeField]
	private TextMeshProUGUI m_JoinLobbyButtonText;

	[SerializeField]
	private OwlcatButton m_ShowHideLobbyCodeButton;

	[SerializeField]
	private OwlcatButton m_LobbyIdPasteButton;

	public override void Initialize()
	{
		base.Initialize();
		m_CreateLobbyButtonText.text = UIStrings.Instance.NetLobbyTexts.CreateLobby;
		m_JoinLobbyButtonText.text = UIStrings.Instance.NetLobbyTexts.JoinLobby;
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.RegionDropdownVM.Subscribe(delegate(OwlcatDropdownVM value)
		{
			m_CreateLobbyButton.SetInteractable(value != null);
		}).AddTo(this);
		ReadyToJoin.Subscribe(m_JoinLobbyButton.SetInteractable).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_LobbyIdPasteButton.OnLeftClickAsObservable(), delegate
		{
			m_LobbyCodeInputField.text = base.ViewModel.GetCopiedLobbyId();
		}).AddTo(this);
		m_CreateLobbyButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.CreateLobby).AddTo(this);
		m_JoinLobbyButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.JoinLobby).AddTo(this);
		OwlcatR3UnitExtensions.Subscribe(m_ShowHideLobbyCodeButton.OnLeftClickAsObservable(), delegate
		{
			ShowLobbyCode.Value = !ShowLobbyCode.Value;
		}).AddTo(this);
		m_LobbyIdPasteButton.SetHint(UIStrings.Instance.NetLobbyTexts.PasteLobbyId).AddTo(this);
		m_ShowHideLobbyCodeButton.SetHint(UIStrings.Instance.NetLobbyTexts.ShowLobbyCode).AddTo(this);
	}
}
