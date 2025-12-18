using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

public class TimelineGateSignalReceiver : MonoBehaviour
{
	private CutscenePlayerData m_Player;

	private readonly List<TimelineGateSignal> m_Markers = new List<TimelineGateSignal>();

	private int m_LastTriggeredIndex;

	public void Setup(CutscenePlayerData player, TimelineAsset playable)
	{
		m_Player = player;
		if (playable == null)
		{
			return;
		}
		foreach (TrackAsset outputTrack in playable.GetOutputTracks())
		{
			IEnumerable<TimelineGateSignal> enumerable = outputTrack?.GetMarkers().OfType<TimelineGateSignal>();
			if (enumerable != null)
			{
				m_Markers.AddRange(enumerable);
			}
		}
		m_Markers.Sort((TimelineGateSignal x, TimelineGateSignal y) => x.time.CompareTo(y.time));
		m_LastTriggeredIndex = -1;
	}

	public void UpdateTime(double playTime)
	{
		for (int i = m_LastTriggeredIndex + 1; i < m_Markers.Count && playTime > m_Markers[i].time; i++)
		{
			m_Player.SignalGateExtra(m_Markers[i].GateId);
			m_LastTriggeredIndex = i;
			PFLog.Cutscene.Log($"Timeline marker triggered {m_Markers[i].time} | {m_Markers[i].Comment}");
		}
	}
}
