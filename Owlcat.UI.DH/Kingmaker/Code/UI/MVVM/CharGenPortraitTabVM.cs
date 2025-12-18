using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using Photon.Realtime;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenPortraitTabVM : SelectionGroupEntityVM, INetLobbyPlayersHandler, ISubscriber
{
	private readonly ReactiveProperty<bool> m_IsMainCharacter = new ReactiveProperty<bool>();

	public readonly CharGenPortraitTab Tab;

	public ReadOnlyReactiveProperty<bool> IsMainCharacter => m_IsMainCharacter;

	public CharGenPortraitTabVM(CharGenPortraitTab tab)
		: base(allowSwitchOff: false)
	{
		Tab = tab;
		m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
		AddDisposable(EventBus.Subscribe(this));
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		m_IsMainCharacter.Value = UtilityNet.IsControlMainCharacter();
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}

	protected override void DoSelectMe()
	{
	}
}
