using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Cheats;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.QA.Arbiter.GameCore.AreaChecker;
using Kingmaker.QA.Arbiter.Service;
using Kingmaker.QA.Arbiter.Service.Interfaces;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.QA.Arbiter.GameCore;

public class GameCoreArbiterIntegration : IArbiterIntegration
{
	public void TeleportToLocalPoint(Vector3 vector3)
	{
		CheatsTransfer.LocalTeleport(vector3);
	}

	public void MoveCameraToPoint(Vector3 position, float rotation, float zoom)
	{
		CameraRig instance = CameraRig.Instance;
		instance.ScrollToImmediately(position);
		instance.RotateToImmediately(rotation);
		instance.CameraZoom.ZoomToImmediate(zoom);
	}

	public void LockInput()
	{
		ArbiterService.Logger.Warning("LockInput not implemented");
	}

	public void UnLockInput()
	{
		ArbiterService.Logger.Warning("UnLockInput not implemented");
	}

	public bool IsInDefaultMode()
	{
		if (Game.Instance.CurrentModeType == GameModeType.Default)
		{
			return !Game.Instance.Controllers.TurnController.InCombat;
		}
		return false;
	}

	public void GoToDefaultMode()
	{
		if (IsInDefaultMode())
		{
			return;
		}
		if (Game.Instance.CurrentModeType == GameModeType.Dialog)
		{
			Game.Instance.Controllers.DialogController.StopDialog();
		}
		else if (Game.Instance.CurrentModeType == GameModeType.Cutscene)
		{
			ArbiterService.Logger.Log("Force stop locked cutscenes");
			foreach (CutscenePlayerData item in Game.Instance.EntityPools.Cutscenes.ToTempList())
			{
				if (item.HasActiveLockControl)
				{
					item.Stop();
				}
			}
			Game.Instance.GameCommandQueue.SkipCutscene();
		}
		else if (Game.Instance.CurrentModeType == GameModeType.Pause)
		{
			ArbiterIntegration.SetGamePause(value: false);
		}
		else if (Game.Instance.Controllers.TurnController.InCombat)
		{
			if (Game.Instance.Controllers.TurnController.IsPreparationTurn)
			{
				Game.Instance.Controllers.TurnController.ForceEndPreparationTurn();
				return;
			}
			Game.Instance.Controllers.TurnController.OnStart();
			CheatsCombat.KillAll();
		}
		else
		{
			ArbiterService.Logger.Error("Unawaitable game mode {0}", Game.Instance.CurrentModeType);
		}
	}

	public RandomFactorsState ExcludeRandomFactors()
	{
		RandomFactorsState result = new RandomFactorsState();
		ArbiterIntegration.HideUi();
		ArbiterIntegration.DisableClouds();
		ArbiterIntegration.DisableWind();
		ArbiterIntegration.DisableFog();
		ArbiterIntegration.DisableFow();
		ArbiterIntegration.DisableFx();
		ArbiterIntegration.HideUnits();
		return result;
	}

	public void IncludeRandomFactors(RandomFactorsState oldFactors)
	{
		ArbiterIntegration.ShowUnits();
		ArbiterIntegration.EnableFx();
		ArbiterIntegration.EnableFow();
		ArbiterIntegration.EnableFog();
		ArbiterIntegration.EnableWind();
		ArbiterIntegration.EnableClouds();
		ArbiterIntegration.ShowUi();
	}

	public string GetCurrentlyLoadedAreaName()
	{
		return Game.Instance.CurrentlyLoadedArea.AreaName.Text;
	}

	public void SetGamePause(bool state)
	{
		ArbiterIntegration.SetGamePause(state);
	}

	public void TakeScreenshot(string probeDataDataFolder, int pointSampleId)
	{
		ArbiterIntegration.TakeScreenshot(probeDataDataFolder, pointSampleId);
	}
}
