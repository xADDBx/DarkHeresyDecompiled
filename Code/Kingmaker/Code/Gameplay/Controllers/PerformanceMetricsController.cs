using System;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Gameplay.Features.Encounter.Events;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers;

public class PerformanceMetricsController : IControllerTick, IController, IGameModeHandler, ISubscriber, IStartEncounterHandler, ISubscriber<ActiveEncounter>, ICombatEndHandler, IAreaHandler
{
	private class MetricsPerformanceSession
	{
		public string Id;

		public string BlueprintGuid;

		public int[] FpsDistribution;

		public DateTime StartTime;
	}

	private readonly int[] m_FpsRanges = new int[15]
	{
		0, 5, 10, 15, 20, 25, 30, 35, 40, 50,
		60, 70, 80, 90, 100
	};

	private readonly int m_TargetFpsBucket;

	private MetricsPerformanceSession m_ActiveSession;

	public TickType GetTickType()
	{
		return TickType.EndOfFrame;
	}

	public PerformanceMetricsController()
	{
		int targetFrameRate = ((Application.targetFrameRate > 0) ? Application.targetFrameRate : 60);
		m_TargetFpsBucket = ((targetFrameRate < m_FpsRanges[^1]) ? Math.Max(0, m_FpsRanges.FindIndex((int fps) => fps >= targetFrameRate)) : m_FpsRanges[^1]);
	}

	public void Tick()
	{
		if (!Metrics.Enabled || m_ActiveSession == null)
		{
			return;
		}
		float num = 1f / Time.unscaledDeltaTime;
		int i = m_TargetFpsBucket;
		if (num >= (float)m_FpsRanges[i])
		{
			for (; i < m_FpsRanges.Length - 1 && num >= (float)m_FpsRanges[i + 1]; i++)
			{
			}
		}
		else
		{
			while (i > 0 && num < (float)m_FpsRanges[i])
			{
				i--;
			}
		}
		m_ActiveSession.FpsDistribution[i]++;
	}

	private void StartSession(string sessionId, string guid)
	{
		if (!Metrics.Enabled)
		{
			return;
		}
		if (m_ActiveSession != null)
		{
			if (m_ActiveSession.Id == sessionId)
			{
				return;
			}
			StopSession();
		}
		m_ActiveSession = new MetricsPerformanceSession
		{
			Id = sessionId,
			BlueprintGuid = guid,
			FpsDistribution = new int[m_FpsRanges.Length],
			StartTime = DateTime.UtcNow
		};
	}

	private void StopSession()
	{
		if (Metrics.Enabled && m_ActiveSession != null && m_ActiveSession.FpsDistribution.Length != 0 && !m_ActiveSession.FpsDistribution.All((int val) => val == 0))
		{
			PerformanceMetricsEvent performanceMetricsEvent = Metrics.Performance.Id(m_ActiveSession.BlueprintGuid).PerformanceSession(m_ActiveSession.Id).Duration((int)(DateTime.UtcNow - m_ActiveSession.StartTime).TotalSeconds)
				.Location(Game.Instance.SceneLoader.CurrentlyLoadedArea.name)
				.FsrMode(SettingsController.Instance.GraphicsSettingsController.FsrMode)
				.VSync(SettingsController.Instance.GraphicsSettingsController.VSyncMode)
				.ShadowQuality(SettingsController.Instance.GraphicsSettingsController.ShadowsQuality)
				.ScreenResolution($"{Screen.currentResolution.width} x {Screen.currentResolution.height}")
				.CPU(SystemInfo.processorType)
				.GPU(SystemInfo.graphicsDeviceName)
				.Ram(SystemInfo.systemMemorySize)
				.DeviceSystem(SystemInfo.operatingSystem);
			for (int i = 0; i < m_FpsRanges.Length - 1; i++)
			{
				performanceMetricsEvent.AddFpsStats(m_FpsRanges[i], m_FpsRanges[i + 1], m_ActiveSession.FpsDistribution[i]);
			}
			performanceMetricsEvent.AddFpsStats(m_FpsRanges[^1], 200, m_ActiveSession.FpsDistribution[m_FpsRanges.Length - 1]);
			performanceMetricsEvent.Send();
			m_ActiveSession = null;
		}
	}

	public void HandleStartEncounter()
	{
		if (Game.Instance.CurrentGameMode?.Type == GameModeType.Default)
		{
			StartDefaultModeSession();
		}
	}

	public void HandleCombatEnd(EncounterCompletionType reason)
	{
		if (Game.Instance.CurrentGameMode?.Type == GameModeType.Default)
		{
			StartDefaultModeSession();
		}
	}

	public void OnAreaDidLoad()
	{
		StartDefaultModeSession();
	}

	public void OnAreaBeginUnloading()
	{
		StopSession();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene)
		{
			BlueprintCutscene cutscene = CutsceneLock.ActiveLockCutscene.Cutscene;
			if (cutscene != null)
			{
				StartSession("cutscene_" + cutscene.name, cutscene.AssetGuid);
			}
		}
		else if (gameMode == GameModeType.Dialog)
		{
			BlueprintDialog dialog = Game.Instance.Controllers.DialogController.Dialog;
			if (dialog != null)
			{
				StartSession("dialog_" + dialog.name, dialog.AssetGuid);
			}
		}
		else if (gameMode == GameModeType.Default)
		{
			StartDefaultModeSession();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		StopSession();
	}

	private void StartDefaultModeSession()
	{
		BlueprintArea currentlyLoadedArea = Game.Instance.SceneLoader.CurrentlyLoadedArea;
		if (currentlyLoadedArea != null)
		{
			ActiveEncounter current = ActiveEncounter.Current;
			if (current != null)
			{
				StartSession("encounter_" + current.Blueprint.name, current.Blueprint.AssetGuid);
			}
			else
			{
				StartSession(currentlyLoadedArea.name, currentlyLoadedArea.AssetGuid);
			}
		}
	}
}
