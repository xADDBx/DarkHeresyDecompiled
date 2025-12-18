using Kingmaker.BarkBanters;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers;

public class BarkBanterController : IControllerTick, IController, IBarkBanterPlayedHandler, ISubscriber, IControllerStop, IPartyCombatHandler, IDialogInteractionHandler, IGameModeHandler
{
	private BarkBanterPlayer m_BarkBanterPlayer;

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (m_BarkBanterPlayer != null)
		{
			m_BarkBanterPlayer.Tick();
			if (m_BarkBanterPlayer.Finished)
			{
				m_BarkBanterPlayer = null;
			}
		}
	}

	public void InterruptBanter()
	{
		m_BarkBanterPlayer?.InterruptBark();
		m_BarkBanterPlayer = null;
	}

	void IControllerStop.OnStop()
	{
		m_BarkBanterPlayer?.InterruptBark();
		m_BarkBanterPlayer = null;
	}

	public void HandleBarkBanter(BlueprintBarkBanter barkBanter)
	{
		if (m_BarkBanterPlayer == null)
		{
			m_BarkBanterPlayer = barkBanter.CreatePlayer();
			Game.Instance.Player.PlayedBanters.Add(barkBanter);
			PFLog.Default.Log($"Play bark banter {barkBanter}");
		}
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (inCombat)
		{
			m_BarkBanterPlayer?.InterruptBark();
			m_BarkBanterPlayer = null;
		}
	}

	public void StartDialogInteraction(BlueprintDialog dialog)
	{
		m_BarkBanterPlayer?.InterruptBark();
		m_BarkBanterPlayer = null;
	}

	public void StopDialogInteraction(BlueprintDialog dialog)
	{
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			m_BarkBanterPlayer?.InterruptBark();
			m_BarkBanterPlayer = null;
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
