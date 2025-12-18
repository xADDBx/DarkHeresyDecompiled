using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Networking.NetGameFsm;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class NetLobbyWaitingPartBaseView : View<NetLobbyVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_ConnectingText;

	private readonly ReactiveProperty<bool> m_IsWaitingPartActive = new ReactiveProperty<bool>();

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_ConnectingText.text = UIStrings.Instance.NetLobbyTexts.ConnectingLabel;
	}

	protected override void OnBind()
	{
		base.ViewModel.NetGameCurrentState.CombineLatest(base.ViewModel.ReadyToHostOrJoin, base.ViewModel.IsInRoom, (NetGame.State state, bool ready, bool inRoom) => new { state, ready, inRoom }).Subscribe(value =>
		{
			bool isConnectingNetGameCurrentState = base.ViewModel.IsConnectingNetGameCurrentState;
			base.gameObject.SetActive(isConnectingNetGameCurrentState);
			m_IsWaitingPartActive.Value = isConnectingNetGameCurrentState;
			if (isConnectingNetGameCurrentState)
			{
				m_ConnectingText.gameObject.SetActive(value: false);
			}
			else
			{
				isConnectingNetGameCurrentState = !value.inRoom && !value.ready;
				base.gameObject.SetActive(isConnectingNetGameCurrentState);
				m_IsWaitingPartActive.Value = isConnectingNetGameCurrentState;
				m_ConnectingText.gameObject.SetActive(!value.ready);
			}
		}).AddTo(this);
	}
}
