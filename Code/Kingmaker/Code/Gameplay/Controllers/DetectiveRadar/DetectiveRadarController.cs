using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers.DetectiveRadar;

public class DetectiveRadarController : IControllerTick, IController
{
	private List<DetectiveClueSignalPart> m_SignalSources = new List<DetectiveClueSignalPart>();

	private List<DetectiveClueSignalPart> m_Jammers = new List<DetectiveClueSignalPart>();

	private List<DetectiveClueSignalPart> m_AllSignals = new List<DetectiveClueSignalPart>();

	public float SignalPowerRaw;

	private DetectiveRadarState m_SignalState;

	public DetectiveClueSignalPart CurrentTrackedSignal;

	private Vector2 m_PlayerPosition => Game.Instance.Player.MainCharacterEntity.Position.To2D();

	public float SignalPowerClamped01
	{
		get
		{
			if (CurrentTrackedSignal == null)
			{
				return 0f;
			}
			return SignalPowerRaw / CurrentTrackedSignal.Settings.Power;
		}
	}

	private DetectiveClueSignalRoot m_Root => DetectiveClueSignalRoot.Instance;

	public DetectiveRadarState SignalState
	{
		get
		{
			return m_SignalState;
		}
		set
		{
			if (m_SignalState != value)
			{
				m_SignalState = value;
				EventBus.RaiseEvent(delegate(IDetectiveRadarHandler h)
				{
					h.HandleRadarModeChange(value);
				});
			}
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		UpdateSignals();
	}

	public void UpdateSignals()
	{
		float num = 0f;
		CurrentTrackedSignal = null;
		foreach (DetectiveClueSignalPart signalSource in m_SignalSources)
		{
			signalSource.IsJammed = IsSignalJammed(signalSource, out var _);
		}
		foreach (DetectiveClueSignalPart allSignal in m_AllSignals)
		{
			if (!allSignal.Enabled || !allSignal.Owner.IsInGame)
			{
				continue;
			}
			EntityViewBase entityViewBase = allSignal.View as EntityViewBase;
			if (entityViewBase == null)
			{
				continue;
			}
			DetectiveRadarSignalSettings settings = allSignal.Settings;
			float num2 = Vector2.Distance(entityViewBase.transform.position.To2D(), m_PlayerPosition);
			float radius = settings.Radius;
			if (radius < num2)
			{
				continue;
			}
			if (settings.IsJammer)
			{
				SignalState = DetectiveRadarState.Jammed;
				CurrentTrackedSignal = allSignal;
				break;
			}
			if (!settings.IsJammer)
			{
				float num3 = Mathf.Clamp(num2, m_Root.MinimalDistance, radius);
				float num4 = ((!(radius - m_Root.MinimalDistance < 0.01f)) ? ((m_Root.MinimalDistance - (num3 - m_Root.MinimalDistance) / (radius - m_Root.MinimalDistance)) * settings.Power) : settings.Power);
				if (num <= num4)
				{
					num = num4;
					CurrentTrackedSignal = allSignal;
				}
			}
		}
		SignalPowerRaw = num;
		if (CurrentTrackedSignal == null)
		{
			SignalState = DetectiveRadarState.NotActivated;
		}
		else if (CurrentTrackedSignal.Settings.IsJammer || CurrentTrackedSignal.IsJammed)
		{
			SignalState = DetectiveRadarState.Jammed;
		}
		else
		{
			SignalState = DetectiveRadarState.Activated;
		}
	}

	public void RegisterSignalSource(DetectiveClueSignalPart signalPart)
	{
		if (signalPart.Settings.IsJammer)
		{
			m_Jammers.Add(signalPart);
		}
		else
		{
			m_SignalSources.Add(signalPart);
		}
		m_AllSignals.Add(signalPart);
	}

	public void UnregisterSignalSource(DetectiveClueSignalPart clueView)
	{
		if (clueView.Settings.IsJammer)
		{
			m_Jammers.Remove(clueView);
		}
		else
		{
			m_SignalSources.Remove(clueView);
		}
		m_AllSignals.Remove(clueView);
	}

	public bool IsSignalJammed(DetectiveClueSignalPart signalPart, out DetectiveClueSignalComponent jammer)
	{
		jammer = null;
		if (signalPart.Settings.IsJammer)
		{
			return false;
		}
		foreach (DetectiveClueSignalPart jammer2 in m_Jammers)
		{
			if (jammer2.Enabled && jammer2.Owner.IsInGame && (signalPart.Owner.Position - jammer2.Owner.Position).magnitude <= jammer2.Settings.Radius)
			{
				jammer = jammer2.Source as DetectiveClueSignalComponent;
				return true;
			}
		}
		return false;
	}

	public void SignalInGameChanged(DetectiveClueSignalPart signalPart)
	{
		if (!signalPart.Enabled || !signalPart.View.IsInGame)
		{
			return;
		}
		UpdateSignals();
		if (CurrentTrackedSignal == signalPart && !signalPart.Settings.IsJammer)
		{
			EventBus.RaiseEvent(delegate(IDetectiveRadarHandler h)
			{
				h.HandleNearestSignalTurnedOn();
			});
		}
	}
}
