using System;
using System.Linq;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Gameplay.Features.Encounter.Events;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers;

public class PerformanceMetricsController : IControllerTick, IController, IControllerEnable, IControllerDisable, IStartEncounterHandler, ISubscriber<ActiveEncounter>, ISubscriber, ICombatEndHandler, IAreaHandler
{
	private class MetricsPerformanceSession
	{
		public string Id;

		public int[] FpsDistribution;

		public DateTime StartTime;
	}

	private static readonly int[] m_FpsRanges = new int[15]
	{
		0, 5, 10, 15, 20, 25, 30, 35, 40, 50,
		60, 70, 80, 90, 100
	};

	private int m_TargetFpsBucket;

	private MetricsPerformanceSession m_ActiveSession;

	public TickType GetTickType()
	{
		return TickType.EndOfFrame;
	}

	public void OnEnable()
	{
		int targetFrameRate = ((Application.targetFrameRate > 0) ? Application.targetFrameRate : 60);
		m_TargetFpsBucket = Math.Max(0, m_FpsRanges.FindIndex((int fps) => fps >= targetFrameRate));
		EventBus.Subscribe(this);
	}

	public void OnDisable()
	{
		StopSession();
		EventBus.Unsubscribe(this);
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

	private void StartSession(string sessionId)
	{
		if (Metrics.Enabled)
		{
			if (m_ActiveSession != null)
			{
				StopSession();
			}
			m_ActiveSession = new MetricsPerformanceSession
			{
				Id = sessionId,
				FpsDistribution = new int[m_FpsRanges.Length],
				StartTime = DateTime.UtcNow
			};
		}
	}

	private void StopSession()
	{
		if (Metrics.Enabled && m_ActiveSession != null && m_ActiveSession.FpsDistribution.Length != 0 && !m_ActiveSession.FpsDistribution.All((int val) => val == 0))
		{
			PerformanceMetricsEvent performanceMetricsEvent = Metrics.Performance.PerformanceSession(m_ActiveSession.Id).Duration((int)(DateTime.UtcNow - m_ActiveSession.StartTime).TotalSeconds).Location(Game.Instance.SceneLoader.CurrentlyLoadedArea.name)
				.FsrMode(SettingsController.Instance.GraphicsSettingsController.FsrMode)
				.VSync(SettingsController.Instance.GraphicsSettingsController.VSyncMode)
				.ShadowQuality(SettingsController.Instance.GraphicsSettingsController.ShadowsQuality)
				.ScreenResolution($"{Screen.currentResolution.width} x {Screen.currentResolution.height}")
				.CPU(SystemInfo.processorType)
				.GPU(SystemInfo.graphicsDeviceName)
				.Ram(SystemInfo.systemMemorySize);
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
		StopSession();
		ActiveEncounter current = ActiveEncounter.Current;
		if (current != null)
		{
			StartSession("encounter_" + current.Blueprint.name);
		}
	}

	public void HandleCombatEnd(EncounterCompletionType reason)
	{
		StopSession();
		ActiveEncounter current = ActiveEncounter.Current;
		if (current != null)
		{
			StartSession("after_encounter_" + current.Blueprint.name);
		}
	}

	public void OnAreaDidLoad()
	{
		string name = Game.Instance.SceneLoader.CurrentlyLoadedArea.name;
		StartSession(name);
	}

	public void OnAreaBeginUnloading()
	{
		StopSession();
	}
}
