using System;
using System.Collections.Generic;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.Code.Framework.CutsceneSystem;

[Serializable]
public class CutsceneGate
{
	public enum GateTriggerType
	{
		AllTracks,
		AnyTrack
	}

	public enum ActivationModeType
	{
		AllTracks,
		FirstTrack,
		RandomTrack
	}

	public enum SkipTracksModeType
	{
		SignalGate,
		DoNotSignalGate
	}

	[SerializeField]
	private string m_Guid;

	[SerializeField]
	private EvaluationErrorHandlingPolicy m_EvaluationErrorHandlingPolicy;

	[SerializeField]
	private List<CutsceneTrack> m_Tracks = new List<CutsceneTrack>();

	[SerializeField]
	private GateTriggerType m_TriggerType;

	[SerializeField]
	private ActivationModeType m_ActivationMode;

	[ShowIf("CanSkipTracks")]
	[SerializeField]
	public SkipTracksModeType m_WhenTrackIsSkipped = SkipTracksModeType.DoNotSignalGate;

	private bool CanSkipTracks => m_ActivationMode != ActivationModeType.AllTracks;

	public string Guid => m_Guid;

	public EvaluationErrorHandlingPolicy EvaluationErrorHandlingPolicy => m_EvaluationErrorHandlingPolicy;

	public List<CutsceneTrack> Tracks => m_Tracks;

	public GateTriggerType TriggerType => m_TriggerType;

	public ActivationModeType ActivationMode => m_ActivationMode;

	public SkipTracksModeType WhenTrackIsSkipped => m_WhenTrackIsSkipped;
}
