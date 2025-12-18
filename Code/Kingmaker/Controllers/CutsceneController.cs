using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Code.View.Bridge.Canvas;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers;

public class CutsceneController : IControllerEnable, IController, IControllerDisable, IControllerTick, IGameModeHandler, ISubscriber
{
	private static readonly LogChannel Logger = PFLog.Cutscene;

	private readonly bool m_TickBackground;

	private static bool s_ShouldStartSkipping;

	public static bool Skipping { get; private set; }

	public static CountableFlag LockSkipBarkBanter { get; } = new CountableFlag();


	public CutsceneController(bool tickBackground)
	{
		m_TickBackground = tickBackground;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.StarSystem && Game.Instance.CurrentModeType != GameModeType.CutsceneGlobalMap)
		{
			return;
		}
		CutsceneLock.CheckRequest();
		if (s_ShouldStartSkipping && LoadingProcess.Instance.IsLoadingScreenShown)
		{
			StartCutsceneSkip();
		}
		bool active = CutsceneLock.Active;
		foreach (CutscenePlayerData cutscene in Game.Instance.EntityPools.Cutscenes)
		{
			using (ProfileScope.New("Tick Scene"))
			{
				if (active || !cutscene.RequireLockControl)
				{
					cutscene.TickScene(Skipping);
				}
			}
		}
		CutsceneLock.CheckRelease();
	}

	public void OnEnable()
	{
		if (Game.Instance.CurrentModeType == GameModeType.Cutscene)
		{
			foreach (UnitGroup unitGroup in Game.Instance.UnitGroups)
			{
				if (!unitGroup.IsInCombat)
				{
					continue;
				}
				for (int i = 0; i < unitGroup.Count; i++)
				{
					BaseUnitEntity baseUnitEntity = unitGroup[i];
					if (baseUnitEntity != null && baseUnitEntity.IsInCombat && !baseUnitEntity.Passive)
					{
						baseUnitEntity.Commands.InterruptAllInterruptible();
					}
				}
			}
		}
		if (m_TickBackground)
		{
			return;
		}
		foreach (CutscenePlayerData cutscene in Game.Instance.EntityPools.Cutscenes)
		{
			if (cutscene.Cutscene.IsBackground)
			{
				cutscene.SetPaused(value: true, CutscenePauseReason.GameModePauseBackgroundCutscenes);
			}
		}
	}

	public void OnDisable()
	{
		if (m_TickBackground)
		{
			return;
		}
		foreach (CutscenePlayerData cutscene in Game.Instance.EntityPools.Cutscenes)
		{
			if (cutscene.Cutscene.IsBackground)
			{
				cutscene.SetPaused(value: false, CutscenePauseReason.GameModePauseBackgroundCutscenes);
			}
		}
	}

	public static void SkipCutsceneInternal()
	{
		if (!Skipping && !s_ShouldStartSkipping && (!LoadingProcess.Instance.IsLoadingScreenActive || LoadingProcess.Instance.IsFadeActive) && (Game.Instance.CurrentModeType == GameModeType.Cutscene || Game.Instance.CurrentModeType == GameModeType.CutsceneGlobalMap))
		{
			if (Game.Instance.EntityPools.Cutscenes.TryFind((CutscenePlayerData p) => p.HasActiveLockControl && p.Cutscene.NonSkippable, out var result))
			{
				Logger.Log($"Can't skip non-skippable cutscene {result}");
				return;
			}
			Logger.Log("Start skipping cutscene");
			SoundState.Instance?.StartCutsceneSkip();
			FadeCanvas.Instance?.ShowLoadingScreen();
			StartCutsceneSkip();
		}
	}

	public static bool SkipBarkBanter()
	{
		if ((bool)LockSkipBarkBanter)
		{
			return false;
		}
		if (Game.Instance.CurrentModeType == GameModeType.Cutscene || Game.Instance.CurrentModeType == GameModeType.CutsceneGlobalMap)
		{
			List<CutscenePlayerData> list = Game.Instance.EntityPools.Cutscenes.Where((CutscenePlayerData p) => p.HasActiveLockControl).ToTempList();
			foreach (CutscenePlayerData item in list)
			{
				item.InterruptBark();
			}
			return list.Count > 0;
		}
		return false;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if ((gameMode == GameModeType.Cutscene || gameMode == GameModeType.CutsceneGlobalMap) && (Skipping || s_ShouldStartSkipping))
		{
			Logger.Log("Stop skipping cutscene");
			Skipping = false;
			s_ShouldStartSkipping = false;
			Game.Instance.Controllers.TimeController.DebugTimeScale = 1f;
			SoundState.Instance.StopCutsceneSkip();
			FadeCanvas.Instance.HideLoadingScreen();
			FadeCanvas.Instance.Fadeout(fade: false);
		}
	}

	private static void StartCutsceneSkip()
	{
		s_ShouldStartSkipping = false;
		Skipping = true;
	}
}
