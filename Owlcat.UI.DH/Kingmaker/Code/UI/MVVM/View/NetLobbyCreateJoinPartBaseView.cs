using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Networking.NetGameFsm;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyCreateJoinPartBaseView : View<NetLobbyVM>
{
	[Space]
	[SerializeField]
	private CanvasGroup m_ShowHideLobbyIcon;

	[Space]
	[SerializeField]
	protected TMP_InputField m_LobbyCodeInputField;

	[SerializeField]
	private TextMeshProUGUI m_LobbyCodeInputFieldPlaceholder;

	[Space]
	[SerializeField]
	protected OwlcatDropdown m_RegionDropdown;

	[SerializeField]
	private GameObject m_RegionWaiting;

	[SerializeField]
	private TextMeshProUGUI m_RegionHeader;

	[Space]
	[SerializeField]
	private TextMeshProUGUI m_VersionText;

	[SerializeField]
	private TextMeshProUGUI m_VersionHeader;

	[SerializeField]
	private TextMeshProUGUI m_NeedSameRegionAndCoopVerDescription;

	[SerializeField]
	protected OwlcatDropdown m_JoinableUserTypesDropdown;

	[SerializeField]
	private TextMeshProUGUI m_JoinableUserTypesLabel;

	[SerializeField]
	protected OwlcatDropdown m_InvitableUserTypesDropdown;

	[SerializeField]
	private TextMeshProUGUI m_InvitableUserTypesLabel;

	protected readonly ReactiveProperty<bool> ShowLobbyCode = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> IsInCreateJoinPart = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> ReadyToJoin = new ReactiveProperty<bool>();

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_LobbyCodeInputFieldPlaceholder.text = UIStrings.Instance.NetLobbyTexts.JoinLobbyCodePlaceholder;
		m_VersionHeader.text = UIStrings.Instance.NetLobbyTexts.CoopVer;
		m_RegionHeader.text = UIStrings.Instance.NetLobbyTexts.RegionHeader;
		m_NeedSameRegionAndCoopVerDescription.text = UIStrings.Instance.NetLobbyTexts.NeedSameRegionAndCoopVer;
	}

	protected override void OnBind()
	{
		base.ViewModel.IsInRoom.CombineLatest(base.ViewModel.NetGameCurrentState, base.ViewModel.ReadyToHostOrJoin, (bool inRoom, NetGame.State state, bool ready) => new { inRoom, state, ready }).Subscribe(value =>
		{
			bool isConnectingNetGameCurrentState = base.ViewModel.IsConnectingNetGameCurrentState;
			IsInCreateJoinPart.Value = !value.inRoom && !isConnectingNetGameCurrentState && value.ready;
			base.gameObject.SetActive(!value.inRoom && !isConnectingNetGameCurrentState && value.ready);
		}).AddTo(this);
		base.ViewModel.HasCodeForLobby.CombineLatest(base.ViewModel.RegionDropdownVM, (bool lobbyCode, OwlcatDropdownVM region) => lobbyCode && region != null).Subscribe(delegate(bool value)
		{
			ReadyToJoin.Value = value;
		}).AddTo(this);
		base.ViewModel.RegionDropdownVM.Subscribe(delegate(OwlcatDropdownVM value)
		{
			m_RegionWaiting.gameObject.SetActive(value == null);
			m_RegionDropdown.gameObject.SetActive(value != null);
			m_RegionDropdown.Bind(value);
		}).AddTo(this);
		ShowLobbyCode.Subscribe(delegate(bool value)
		{
			m_ShowHideLobbyIcon.alpha = (value ? 1f : 0.25f);
			m_LobbyCodeInputField.contentType = ((!value) ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard);
			m_LobbyCodeInputField.ForceLabelUpdate();
		}).AddTo(this);
		m_LobbyCodeInputField.text = string.Empty;
		Observable.EveryValueChanged(m_LobbyCodeInputField, (TMP_InputField f) => f.text).Subscribe(delegate(string t)
		{
			base.ViewModel.SetLobbyCode(t);
		}).AddTo(this);
		base.ViewModel.Version.Subscribe(delegate(string value)
		{
			m_VersionText.text = value;
		}).AddTo(this);
		m_VersionText.SetHint(UIStrings.Instance.NetLobbyTexts.CoopVerTooltip).AddTo(this);
		SetUserTypesDropdowns();
	}

	private void SetUserTypesDropdowns()
	{
		bool flag = base.ViewModel.JoinableUserTypesDropdownVM != null;
		m_JoinableUserTypesDropdown.gameObject.SetActive(flag);
		if (flag)
		{
			m_JoinableUserTypesLabel.text = UIStrings.Instance.NetLobbyTexts.JoinableUserTypesLabel;
			m_JoinableUserTypesDropdown.Bind(base.ViewModel.JoinableUserTypesDropdownVM);
			m_JoinableUserTypesDropdown.SetIndex((int)base.ViewModel.CurrentJoinableUserType.CurrentValue);
			m_JoinableUserTypesDropdown.Index.Subscribe(base.ViewModel.SetJoinableUserType).AddTo(this);
		}
		bool flag2 = base.ViewModel.InvitableUserTypesDropdownVM != null;
		m_InvitableUserTypesDropdown.gameObject.SetActive(flag2);
		if (flag2)
		{
			m_InvitableUserTypesLabel.text = UIStrings.Instance.NetLobbyTexts.InvitableUserTypesLabel;
			m_InvitableUserTypesDropdown.Bind(base.ViewModel.InvitableUserTypesDropdownVM);
			m_InvitableUserTypesDropdown.SetIndex((int)base.ViewModel.CurrentInvitableUserType.CurrentValue);
			m_InvitableUserTypesDropdown.Index.Subscribe(base.ViewModel.SetInvitableUserType).AddTo(this);
		}
	}
}
