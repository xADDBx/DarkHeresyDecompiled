using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Controllers;

public class PauseController : IControllerTick, IController, IControllerReset
{
	private NetPlayerGroup m_PausedPlayers = NetPlayerGroup.Empty;

	private bool m_ManualPause;

	private bool m_UpdateRequired = true;

	public bool IsManualPause => m_ManualPause;

	private static NetPlayerGroup PlayersReadyMask => NetworkingManager.PlayersReadyMask;

	public bool IsPausedByPlayers => m_PausedPlayers.Contains(PlayersReadyMask);

	public bool IsPausedByLocalPlayer => m_PausedPlayers.Contains(NetworkingManager.LocalNetPlayer);

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		if (m_UpdateRequired)
		{
			m_UpdateRequired = false;
			Game.Instance.SetIsPauseForce(m_ManualPause || IsPausedByPlayers);
			EventBus.RaiseEvent(delegate(IPauseHandler h)
			{
				h.OnPauseToggled();
			});
		}
	}

	void IControllerReset.OnReset()
	{
		m_PausedPlayers = NetPlayerGroup.Empty;
		m_ManualPause = false;
		m_UpdateRequired = true;
	}

	public void RequestPauseUi(bool isPaused)
	{
		if (isPaused != IsPausedByLocalPlayer && !LoadingProcess.Instance.IsAnyLoadingScreenActive)
		{
			Game.Instance.GameCommandQueue.RequestPauseUi(isPaused);
		}
	}

	public void SetPlayer(NetPlayer player, bool isPaused)
	{
		NetPlayerGroup netPlayerGroup = (isPaused ? m_PausedPlayers.Add(player) : m_PausedPlayers.Del(player));
		bool flag = !m_PausedPlayers.Equals(netPlayerGroup);
		m_UpdateRequired |= flag;
		m_PausedPlayers = netPlayerGroup;
	}

	public void SetManualPause(bool isPaused)
	{
		if (!IsPausedByPlayers)
		{
			m_ManualPause = isPaused;
			m_UpdateRequired = true;
		}
	}

	public void OnPlayerLeftRoom()
	{
		m_UpdateRequired = true;
	}
}
